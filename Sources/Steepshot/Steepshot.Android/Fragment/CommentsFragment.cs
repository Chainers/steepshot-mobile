using System;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Models;
using Steepshot.Activity;
using Android.Content;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Fragment
{
    public class CommentsFragment : BaseFragmentWithPresenter<CommentsPresenter>
    {
        private const string PostUrlExtraPath = "url";
        private const string PostNetVotesExtraPath = "count";

        public const string ResultString = "result";
        public const string CountString = "count";

        private Post _post;
        private CommentAdapter _adapter;
        private string _uid;
        private bool _openKeyboard;
        private LinearLayoutManager _manager;
        private int _counter = 0;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.comments_list)] private RecyclerView _comments;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _spinner;
        [InjectView(Resource.Id.text_input)] private EditText _textInput;
        [InjectView(Resource.Id.btn_post)] private RelativeLayout _postBtn;
        [InjectView(Resource.Id.btn_back)] private ImageButton _backButton;
        [InjectView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [InjectView(Resource.Id.btn_settings)] private ImageButton _settings;
        [InjectView(Resource.Id.profile_login)] private TextView _viewTitle;
        [InjectView(Resource.Id.send_spinner)] private ProgressBar _sendSpinner;
        [InjectView(Resource.Id.btn_post_image)] private ImageView _postImage;
        [InjectView(Resource.Id.message)] private RelativeLayout _messagePanel;
        [InjectView(Resource.Id.root_layout)] private RelativeLayout _rootLayout;
#pragma warning restore 0649

        public CommentsFragment()
        {
            //This is fix for crashing when app killed in background
        }

        public CommentsFragment(Post post, bool openKeyboard)
        {
            _post = post;
            _uid = post.Url;
            _openKeyboard = openKeyboard;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_comments, null);
                Cheeseknife.Inject(this, InflatedView);
            }
            ToggleTabBar(true);
            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            _textInput.Typeface = Style.Regular;
            _viewTitle.Typeface = Style.Semibold;
            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += OnBack;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = Localization.Messages.Comments;

            _postBtn.Click += OnPost;
            _rootLayout.Click += OnRootClick;

            _manager = new LinearLayoutManager(Context, LinearLayoutManager.Vertical, false);

            Presenter.SourceChanged += PresenterSourceChanged;
            _adapter = new CommentAdapter(Context, Presenter, _post);
            _adapter.LikeAction += LikeAction;
            _adapter.UserAction += UserAction;
            _adapter.VotersClick += VotersAction;
            _adapter.FlagAction += FlagAction;
            _adapter.HideAction += HideAction;
            _adapter.ReplyAction += ReplyAction;
            _adapter.DeleteAction += DeleteAction;
            _adapter.RootClickAction += HideKeyboard;
            _adapter.TagAction += TagAction;

            _comments.SetLayoutManager(_manager);
            _comments.SetAdapter(_adapter);
            _comments.Visibility = ViewStates.Visible;
            if (!BasePresenter.User.IsAuthenticated)
                _messagePanel.Visibility = ViewStates.Gone;

            LoadComments(_uid);
            if (_openKeyboard)
            {
                _openKeyboard = false;
                OpenKeyboard();
            }
        }

        private void PresenterSourceChanged(Status status)
        {
            if (!IsInitialized)
                return;

            Activity.RunOnUiThread(() =>
            {
                _adapter.NotifyDataSetChanged();
            });
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }

        private void OnBack(object sender, EventArgs e)
        {
            HideKeyboard();
            Activity.OnBackPressed();
        }

        private void OnRootClick(object sender, EventArgs e)
        {
            HideKeyboard();
        }

        private void TagAction(string tag)
        {
            if (tag != null)
            {
                Activity.Intent.PutExtra(SearchFragment.SearchExtra, tag);
                ((BaseActivity)Activity).OpenNewContentFragment(new PreSearchFragment());
            }
            else
                _adapter.NotifyDataSetChanged();
        }

        private async void OnPost(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_textInput.Text))
                return;

            _sendSpinner.Visibility = ViewStates.Visible;
            _postBtn.Enabled = false;
            _postImage.Visibility = ViewStates.Invisible;

            HideKeyboard();

            var resp = await Presenter.TryCreateComment(_textInput.Text, _uid);

            if (!IsInitialized)
                return;

            if (resp != null && resp.IsSuccess)
            {
                _textInput.Text = string.Empty;
                _textInput.ClearFocus();

                var error = await Presenter.TryLoadNextComments(_uid);

                if (!IsInitialized)
                    return;

                Context.ShowAlert(error, ToastLength.Short);
                _comments.MoveToPosition(Presenter.Count - 1);

                _counter++;

                Activity.Intent.PutExtra(ResultString, _uid);
                Activity.Intent.PutExtra(CountString, _counter);
            }
            else
            {
                Context.ShowAlert(resp, ToastLength.Short);
            }

            _sendSpinner.Visibility = ViewStates.Invisible;
            _postBtn.Enabled = true;
            _postImage.Visibility = ViewStates.Visible;
        }

        private async void LoadComments(string postUrl)
        {
            _spinner.Visibility = ViewStates.Visible;

            var error = await Presenter.TryLoadNextComments(postUrl);

            if (!IsInitialized)
                return;

            Context.ShowAlert(error, ToastLength.Short);

            _spinner.Visibility = ViewStates.Gone;
        }

        private void UserAction(Post post)
        {
            if (post == null)
                return;

            if (BasePresenter.User.Login != post.Author)
                ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(post.Author));
        }

        private void VotersAction(Post post, VotersType type)
        {
            if (post == null)
                return;

            var isLikers = type == VotersType.Likes;
            Activity.Intent.PutExtra(PostUrlExtraPath, post.Url.Substring(post.Url.LastIndexOf("@", StringComparison.Ordinal)));
            Activity.Intent.PutExtra(PostNetVotesExtraPath, isLikers ? post.NetLikes : post.NetFlags);
            Activity.Intent.PutExtra(VotersFragment.VotersType, isLikers);
            ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
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
            OpenKeyboard();
        }

        private async void LikeAction(Post post)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                var error = await Presenter.TryVote(post);

                if (!IsInitialized)
                    return;
                Context.ShowAlert(error, ToastLength.Short);
            }
            else
            {
                var intent = new Intent(Context, typeof(WelcomeActivity));
                StartActivity(intent);
            }
        }

        private async void FlagAction(Post post)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                var error = await Presenter.TryFlag(post);

                if (!IsInitialized)
                    return;
                Context.ShowAlert(error, ToastLength.Short);
            }
            else
            {
                var intent = new Intent(Context, typeof(WelcomeActivity));
                StartActivity(intent);
            }
        }

        private void HideAction(Post post)
        {
            Presenter.RemovePost(post);
        }

        private void HideKeyboard()
        {
            ((BaseActivity)Activity).HideKeyboard();
        }

        private void OpenKeyboard()
        {
            _textInput?.RequestFocus();
            _textInput?.Post(() => ((BaseActivity)Activity).OpenKeyboard(_textInput));
        }

        private async void DeleteAction(Post post)
        {
            var error = await Presenter.TryDeletePost(post);
            if (!IsInitialized)
                return;

            Context.ShowAlert(error);
        }
    }
}
