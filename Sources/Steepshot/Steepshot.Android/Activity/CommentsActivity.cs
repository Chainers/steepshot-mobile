using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;

namespace Steepshot.Activity
{
    [Activity(Label = "CommentsActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CommentsActivity : BaseActivityWithPresenter<CommentsPresenter>
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
                    var text = _textInput.Text;
                    _textInput.Text = string.Empty;
                    _textInput.ClearFocus();
                    var imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
                    imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);
                    var resp = await _presenter.TryCreateComment(text, _uid);
                    if (resp.Success)
                    {
                        if (_textInput != null)
                        {
                            var errors = await _presenter.TryLoadNextComments(_uid);
                            if (errors != null)
                            {
                                _adapter?.NotifyDataSetChanged();
                                _comments.SmoothScrollToPosition(_presenter.Count - 1);
                            }
                        }
                    }
                    else
                    {
                        ShowAlert(Localization.Messages.RapidPosting, ToastLength.Short);
                    }
                    if (_sendSpinner != null)
                        _sendSpinner.Visibility = ViewStates.Invisible;
                    if (_post != null)
                        _post.Enabled = true;
                    if (_postImage != null)
                        _postImage.Visibility = ViewStates.Visible;
                }
            }
            else
            {
                ShowAlert(GetString(Resource.String.need_login), ToastLength.Short);
            }
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_comments);
            Cheeseknife.Inject(this);

            var font = Typeface.CreateFromAsset(Application.Context.Assets, "OpenSans-Regular.ttf");
            var semiboldFont = Typeface.CreateFromAsset(Application.Context.Assets, "OpenSans-Semibold.ttf");

            _textInput.Typeface = font;
            _viewTitle.Typeface = semiboldFont;
            _backButton.Visibility = ViewStates.Visible;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = "Post comments";

            _uid = Intent.GetStringExtra("uid");
            _manager = new LinearLayoutManager(this, LinearLayoutManager.Vertical, false);
            _comments.SetLayoutManager(_manager);
            _adapter = new CommentAdapter(this, _presenter, new[] { font, semiboldFont });
            if (_comments != null)
            {
                _comments.SetAdapter(_adapter);
                _adapter.LikeAction += FeedAdapter_LikeAction;
                _adapter.UserAction += FeedAdapter_UserAction;
            }

            var errors = await _presenter.TryLoadNextComments(_uid);
            if (errors == null)
                return;
            if (errors.Any())
                ShowAlert(errors, ToastLength.Short);
            else
            {
                _adapter?.NotifyDataSetChanged();
                _spinner.Visibility = ViewStates.Gone;
            }
        }

        private void FeedAdapter_UserAction(int position)
        {
            var user = _presenter[position];
            if (user == null)
                return;
            var intent = new Intent(this, typeof(ProfileActivity));
            intent.PutExtra("ID", user.Author);
            StartActivity(intent);
        }

        private async void FeedAdapter_LikeAction(int position)
        {
            try
            {
                if (BasePresenter.User.IsAuthenticated)
                {
                    var errors = await _presenter.TryVote(position);
                    if (errors == null)
                        return;

                    if (errors.Any())
                        ShowAlert(errors, ToastLength.Short);
                    else
                        _adapter?.NotifyDataSetChanged();
                }
                else
                {
                    var intent = new Intent(this, typeof(PreSignInActivity));
                    StartActivity(intent);
                }
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
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
    }
}
