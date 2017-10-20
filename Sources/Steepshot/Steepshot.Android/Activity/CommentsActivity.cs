using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(Label = "CommentsActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class CommentsActivity : BaseActivityWithPresenter<CommentsPresenter>
    {
        private CommentAdapter _adapter;
        private string _uid;
        private LinearLayoutManager _manager;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.comments_list)] private RecyclerView _comments;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _spinner;
        [InjectView(Resource.Id.text_input)] private EditText _textInput;
        [InjectView(Resource.Id.btn_post)] private RelativeLayout _post;
        [InjectView(Resource.Id.btn_back)] private ImageButton _backButton;
        [InjectView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [InjectView(Resource.Id.btn_settings)] private ImageButton _settings;
        [InjectView(Resource.Id.profile_login)] private TextView _viewTitle;
        [InjectView(Resource.Id.send_spinner)] private ProgressBar _sendSpinner;
        [InjectView(Resource.Id.btn_post_image)] private ImageView _postImage;
#pragma warning restore 0649


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_comments);
            Cheeseknife.Inject(this);

            _textInput.Typeface = Style.Regular;
            _viewTitle.Typeface = Style.Semibold;
            _backButton.Visibility = ViewStates.Visible;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = "Post comments";

            _uid = Intent.GetStringExtra("uid");
            _manager = new LinearLayoutManager(this, LinearLayoutManager.Vertical, false);
            _comments.SetLayoutManager(_manager);
            _adapter = new CommentAdapter(this, _presenter);
            if (_comments != null)
            {
                _comments.SetAdapter(_adapter);
                _adapter.LikeAction += LikeAction;
                _adapter.UserAction += UserAction;
            }
            LoadComments(_uid);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        protected override void CreatePresenter()
        {
            _presenter = new CommentsPresenter();
        }


        [InjectOnClick(Resource.Id.btn_back)]
        public void OnBack(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        [InjectOnClick(Resource.Id.btn_post)]
        public async void OnPost(object sender, EventArgs e)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                if (_textInput.Text != string.Empty)
                {
                    _sendSpinner.Visibility = ViewStates.Visible;
                    _post.Enabled = false;
                    _postImage.Visibility = ViewStates.Invisible;

                    var imm = (InputMethodManager)GetSystemService(InputMethodService);
                    imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);
                    var resp = await _presenter.TryCreateComment(_textInput.Text, _uid);
                    if (resp != null && resp.Success)
                    {
                        _textInput.Text = string.Empty;
                        _textInput.ClearFocus();

                        _presenter.Clear();
                        var errors = await _presenter.TryLoadNextComments(_uid);

                        ShowAlert(errors, ToastLength.Short);
                        _adapter.NotifyDataSetChanged();

                        var pos = _presenter.Count - 1;
                        if (pos < 0)
                            pos = 0;
                        _comments.SmoothScrollToPosition(pos);
                    }
                    else
                    {
                        ShowAlert(resp, ToastLength.Short);
                    }

                    _sendSpinner.Visibility = ViewStates.Invisible;
                    _post.Enabled = true;
                    _postImage.Visibility = ViewStates.Visible;
                }
            }
            else
            {
                ShowAlert(GetString(Resource.String.need_login), ToastLength.Short);
            }
        }


        private async void LoadComments(string postUrl)
        {
            if (_spinner != null)
                _spinner.Visibility = ViewStates.Visible;

            _presenter.Clear();
            var errors = await _presenter
                .TryLoadNextComments(postUrl);

            ShowAlert(errors, ToastLength.Short);
            _adapter.NotifyDataSetChanged();

            if (_spinner != null)
                _spinner.Visibility = ViewStates.Gone;
        }

        private void UserAction(int position)
        {
            var user = _presenter[position];
            if (user == null)
                return;

            var intent = new Intent(this, typeof(ProfileActivity));
            intent.PutExtra("ID", user.Author);
            StartActivity(intent);
        }

        private async void LikeAction(int position)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                var errors = await _presenter.TryVote(position);
                ShowAlert(errors, ToastLength.Short);
                _adapter.NotifyDataSetChanged();
            }
            else
            {
                var intent = new Intent(this, typeof(PreSignInActivity));
                StartActivity(intent);
            }
        }
    }
}
