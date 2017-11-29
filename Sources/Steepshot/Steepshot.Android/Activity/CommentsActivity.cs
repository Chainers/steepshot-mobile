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
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Models;

namespace Steepshot.Activity
{
    [Activity(Label = "CommentsActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class CommentsActivity : BaseActivityWithPresenter<CommentsPresenter>
    {
        public const string PostExtraPath = "uid";
        public const int RequestCode = 124;
        public const string ResultString = "result";
        public const string CountString = "count";

        private CommentAdapter _adapter;
        private string _uid;
        private LinearLayoutManager _manager;
        private int _counter = 0;

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
        [InjectView(Resource.Id.message)] private RelativeLayout _messagePanel;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_comments);
            Cheeseknife.Inject(this);

            _textInput.Typeface = Style.Regular;
            _viewTitle.Typeface = Style.Semibold;
            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += OnBack;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = Localization.Messages.PostComments;

            _post.Click += OnPost;

            _uid = Intent.GetStringExtra(PostExtraPath);
            _manager = new LinearLayoutManager(this, LinearLayoutManager.Vertical, false);

            Presenter.SourceChanged += PresenterSourceChanged;
            _adapter = new CommentAdapter(this, Presenter);
            _adapter.LikeAction += LikeAction;
            _adapter.UserAction += UserAction;
            _adapter.FlagAction += FlagAction;
            _adapter.HideAction += HideAction;
            _adapter.ReplyAction += ReplyAction;

            _comments.SetLayoutManager(_manager);
            _comments.SetAdapter(_adapter);
            if (!BasePresenter.User.IsAuthenticated)
                _messagePanel.Visibility = ViewStates.Gone;

            LoadComments(_uid);
        }

        private void PresenterSourceChanged(Status status)
        {
            if (IsDestroyed || IsFinishing)
                return;

            RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        private void OnBack(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        private async void OnPost(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_textInput.Text))
                return;

            _sendSpinner.Visibility = ViewStates.Visible;
            _post.Enabled = false;
            _postImage.Visibility = ViewStates.Invisible;

            HideKeyboard();

            var resp = await Presenter.TryCreateComment(_textInput.Text, _uid);

            if (IsFinishing || IsDestroyed)
                return;

            if (resp != null && resp.Success)
            {
                _textInput.Text = string.Empty;
                _textInput.ClearFocus();

                var errors = await Presenter.TryLoadNextComments(_uid);

                if (IsFinishing || IsDestroyed)
                    return;

                this.ShowAlert(errors, ToastLength.Short);
                _comments.MoveToPosition(Presenter.Count - 1);

                _counter++;
                Intent returnIntent = new Intent();
                returnIntent.PutExtra(ResultString, _uid);
                returnIntent.PutExtra(CountString, _counter);
                SetResult(Result.Ok, returnIntent);
            }
            else
            {
                this.ShowAlert(resp, ToastLength.Short);
            }

            _sendSpinner.Visibility = ViewStates.Invisible;
            _post.Enabled = true;
            _postImage.Visibility = ViewStates.Visible;
        }

        private async void LoadComments(string postUrl)
        {
            _spinner.Visibility = ViewStates.Visible;

            var errors = await Presenter.TryLoadNextComments(postUrl);

            if (IsFinishing || IsDestroyed)
                return;

            this.ShowAlert(errors, ToastLength.Short);

            _spinner.Visibility = ViewStates.Gone;
        }

        private void UserAction(Post post)
        {
            if (post == null)
                return;

            var intent = new Intent(this, typeof(ProfileActivity));
            intent.PutExtra(ProfileActivity.UserExtraName, post.Author);
            StartActivity(intent);
        }

        private void ReplyAction(Post post)
        {
            if (post == null)
                return;
            if (!_textInput.Text.StartsWith($"@{post.Author}"))
            {
                _textInput.Text = $"@{post.Author} {_textInput.Text}";
                _textInput.SetSelection(_textInput.Text.Length);
            }

            _textInput.RequestFocus();
            var imm = GetSystemService(InputMethodService) as InputMethodManager;
            imm?.ShowSoftInput(_textInput, ShowFlags.Implicit);
        }

        private async void LikeAction(Post post)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                var errors = await Presenter.TryVote(post);

                if (IsFinishing || IsDestroyed)
                    return;
                this.ShowAlert(errors, ToastLength.Short);
            }
            else
            {
                var intent = new Intent(this, typeof(WelcomeActivity));
                StartActivity(intent);
            }
        }

        private async void FlagAction(Post post)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                var errors = await Presenter.TryFlag(post);

                if (IsFinishing || IsDestroyed)
                    return;
                this.ShowAlert(errors, ToastLength.Short);
            }
            else
            {
                var intent = new Intent(this, typeof(WelcomeActivity));
                StartActivity(intent);
            }
        }

        private void HideAction(Post post)
        {
            Presenter.RemovePost(post);
        }
    }
}
