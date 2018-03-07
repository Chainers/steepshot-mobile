using System;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Models;
using Steepshot.Activity;
using Android.Content;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;

namespace Steepshot.Fragment
{
    public sealed class CommentsFragment : BaseFragmentWithPresenter<CommentsPresenter>
    {
        private const string PostUrlExtraPath = "url";
        private const string PostNetVotesExtraPath = "count";

        public const string ResultString = "result";
        public const string CountString = "count";

        private Post _post, _editComment;
        private CommentAdapter _adapter;
        private bool _openKeyboard;
        private LinearLayoutManager _manager;
        private int _counter = 0;

#pragma warning disable 0649, 4014
        [CheeseBind.BindView(Resource.Id.comments_list)] private RecyclerView _comments;
        [CheeseBind.BindView(Resource.Id.loading_spinner)] private ProgressBar _spinner;
        [CheeseBind.BindView(Resource.Id.text_input)] private EditText _textInput;
        [CheeseBind.BindView(Resource.Id.btn_post)] private RelativeLayout _postBtn;
        [CheeseBind.BindView(Resource.Id.btn_back)] private ImageButton _backButton;
        [CheeseBind.BindView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [CheeseBind.BindView(Resource.Id.btn_settings)] private ImageButton _settings;
        [CheeseBind.BindView(Resource.Id.profile_login)] private TextView _viewTitle;
        [CheeseBind.BindView(Resource.Id.send_spinner)] private ProgressBar _sendSpinner;
        [CheeseBind.BindView(Resource.Id.btn_post_image)] private ImageView _postImage;
        [CheeseBind.BindView(Resource.Id.message)] private RelativeLayout _messagePanel;
        [CheeseBind.BindView(Resource.Id.root_layout)] private RelativeLayout _rootLayout;
        [CheeseBind.BindView(Resource.Id.comment_edit)] private RelativeLayout _commentEditBlock;
        [CheeseBind.BindView(Resource.Id.comment_cancel_edit)] private ImageButton _commentEditCancelBtn;
        [CheeseBind.BindView(Resource.Id.comment_edit_message)] private TextView _commentEditMessage;
        [CheeseBind.BindView(Resource.Id.comment_edit_text)] private TextView _commentEditText;
#pragma warning restore 0649

        public CommentsFragment()
        {
            //This is fix for crashing when app killed in background
        }

        public CommentsFragment(Post post, bool openKeyboard)
        {
            _post = post;
            _openKeyboard = openKeyboard;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_comments, null);
                Cheeseknife.Bind(this, InflatedView);
            }
            ToggleTabBar(true);
            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            _commentEditMessage.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.EditComment);
            _textInput.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.PutYourComment);
            _viewTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Comments);

            _commentEditMessage.Typeface = Style.Semibold;
            _commentEditText.Typeface = Style.Regular;
            _textInput.Typeface = Style.Regular;
            _viewTitle.Typeface = Style.Semibold;
            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += OnBack;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;

            _postBtn.Click += OnPost;
            _rootLayout.Click += OnRootClick;

            _manager = new LinearLayoutManager(Context, LinearLayoutManager.Vertical, false);

            Presenter.SourceChanged += PresenterSourceChanged;
            _adapter = new CommentAdapter(Context, Presenter, _post);
            _adapter.CommentAction += CommentAction;
            _adapter.RootClickAction += HideKeyboard;
            _adapter.TagAction += TagAction;

            _comments.SetLayoutManager(_manager);
            _comments.SetAdapter(_adapter);
            _comments.Visibility = ViewStates.Visible;
            if (!BasePresenter.User.IsAuthenticated)
                _messagePanel.Visibility = ViewStates.Gone;

            _commentEditCancelBtn.Click += CommentEditCancelBtnOnClick;

            LoadComments(_post);
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

            if (_commentEditBlock.Visibility == ViewStates.Visible)
            {
                var error = await Presenter.TryEditComment(BasePresenter.User.UserInfo, _post, _editComment, _textInput.Text, AppSettings.AppInfo);

                if (!IsInitialized)
                    return;

                Context.ShowAlert(error);
                CommentEditCancelBtnOnClick(null, null);
            }
            else
            {
                var resp = await Presenter.TryCreateComment(_post, _textInput.Text);

                if (!IsInitialized)
                    return;

                if (resp.IsSuccess)
                {
                    _textInput.Text = string.Empty;
                    _textInput.ClearFocus();

                    var error = await Presenter.TryLoadNextComments(_post);

                    if (!IsInitialized)
                        return;

                    Context.ShowAlert(error, ToastLength.Short);
                    _comments.MoveToPosition(Presenter.Count - 1);

                    _counter++;

                    Activity.Intent.PutExtra(ResultString, _post.Url);
                    Activity.Intent.PutExtra(CountString, _counter);
                }
                else
                {
                    Context.ShowAlert(resp.Error, ToastLength.Short);
                }
            }

            _sendSpinner.Visibility = ViewStates.Invisible;
            _postBtn.Enabled = true;
            _postImage.Visibility = ViewStates.Visible;
        }

        private async void LoadComments(Post post)
        {
            _spinner.Visibility = ViewStates.Visible;

            var error = await Presenter.TryLoadNextComments(post);

            if (!IsInitialized)
                return;

            Context.ShowAlert(error, ToastLength.Short);

            _spinner.Visibility = ViewStates.Gone;
        }

        private async void CommentAction(ActionType type, Post post)
        {
            switch (type)
            {
                case ActionType.Like:
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
                        break;
                    }
                case ActionType.Profile:
                    {
                        if (post == null)
                            return;

                        if (BasePresenter.User.Login != post.Author)
                            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(post.Author));
                        break;
                    }
                case ActionType.VotersLikes:
                case ActionType.VotersFlags:
                    {
                        if (post == null)
                            return;

                        var isLikers = type == ActionType.VotersLikes;
                        Activity.Intent.PutExtra(PostUrlExtraPath, post.Url.Substring(post.Url.LastIndexOf("@", StringComparison.Ordinal)));
                        Activity.Intent.PutExtra(PostNetVotesExtraPath, isLikers ? post.NetLikes : post.NetFlags);
                        Activity.Intent.PutExtra(VotersFragment.VotersType, isLikers);
                        ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
                        break;
                    }
                case ActionType.Flag:
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
                        break;
                    }
                case ActionType.Hide:
                    {
                        Presenter.HidePost(post);
                        break;
                    }
                case ActionType.Reply:
                    {
                        if (post == null)
                            return;
                        if (!_textInput.Text.StartsWith($"@{post.Author}"))
                        {
                            _textInput.Text = $"@{post.Author} {_textInput.Text}";
                            _textInput.SetSelection(_textInput.Text.Length);
                        }
                        OpenKeyboard();
                        break;
                    }
                case ActionType.Edit:
                    {
                        _editComment = post;
                        _textInput.Text = _commentEditText.Text = post.Body;
                        _textInput.SetSelection(post.Body.Length);
                        _commentEditBlock.Visibility = ViewStates.Visible;
                        OpenKeyboard();
                        break;
                    }
                case ActionType.Delete:
                    {
                        var error = await Presenter.TryDeleteComment(post, _post);
                        if (!IsInitialized)
                            return;

                        Context.ShowAlert(error);
                        break;
                    }
            }
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

        private void CommentEditCancelBtnOnClick(object sender, EventArgs eventArgs)
        {
            _textInput.Text = _commentEditText.Text = string.Empty;
            _commentEditBlock.Visibility = ViewStates.Gone;
            HideKeyboard();
        }
    }
}
