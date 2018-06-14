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
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
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

        private readonly Post _post;
        private Post _editComment;
        private CommentAdapter _adapter;
        private bool _openKeyboard;
        private LinearLayoutManager _manager;
        private int _counter = 0;
        private GradientDrawable _textInputShape;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.comments_list)] private RecyclerView _comments;
        [BindView(Resource.Id.loading_spinner)] private ProgressBar _spinner;
        [BindView(Resource.Id.text_input)] private EditText _textInput;
        [BindView(Resource.Id.btn_post)] private RelativeLayout _postBtn;
        [BindView(Resource.Id.btn_back)] private ImageButton _backButton;
        [BindView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [BindView(Resource.Id.btn_settings)] private ImageButton _settings;
        [BindView(Resource.Id.profile_login)] private TextView _viewTitle;
        [BindView(Resource.Id.send_spinner)] private ProgressBar _sendSpinner;
        [BindView(Resource.Id.btn_post_image)] private ImageView _postImage;
        [BindView(Resource.Id.message)] private RelativeLayout _messagePanel;
        [BindView(Resource.Id.root_layout)] private RelativeLayout _rootLayout;
        [BindView(Resource.Id.edit_controls)] private RelativeLayout _editControls;
        [BindView(Resource.Id.cancel)] private Button _cancel;
        [BindView(Resource.Id.save)] private Button _save;
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

            _cancel.Text = "Cancel";
            _save.Text = "Save";
            _textInput.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.PutYourComment);
            _viewTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Comments);

            _textInputShape = new GradientDrawable();
            _textInputShape.SetCornerRadius(BitmapUtils.DpToPixel(20, Resources));
            _textInputShape.SetColor(Color.White);
            _textInputShape.SetStroke((int)BitmapUtils.DpToPixel(1, Resources), Style.R244G244B246);
            _textInput.Background = _textInputShape;
            _textInput.TextChanged += TextInputOnTextChanged;

            _cancel.Typeface = Style.Semibold;
            _save.Typeface = Style.Semibold;
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
            _adapter.AutoLinkAction += AutoLinkAction;

            _comments.SetLayoutManager(_manager);
            _comments.SetAdapter(_adapter);
            _comments.Visibility = ViewStates.Visible;

            if (!AppSettings.User.IsAuthenticated)
                _messagePanel.Visibility = ViewStates.Gone;

            _cancel.Click += CommentEditCancelBtnOnClick;
            _save.Click += SaveOnClick;

            LoadComments(_post);
            if (_openKeyboard)
            {
                _openKeyboard = false;
                OpenKeyboard();
            }
        }

        private void TextInputOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_textInput.LineCount <= 2)
            {
                _textInputShape.SetCornerRadius(BitmapUtils.DpToPixel(20, Resources) / _textInput.LineCount);
                _textInput.Background = _textInputShape;
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
            CommentEditCancelBtnOnClick(null, null);
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

        private async void OnPost(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_textInput.Text) || _editComment != null && _editComment.Editing && _editComment.Body.Equals(_textInput.Text))
                return;

            _sendSpinner.Visibility = ViewStates.Visible;
            _postBtn.Enabled = false;
            _postImage.Visibility = ViewStates.Invisible;

            HideKeyboard();

            if (_editControls.Visibility == ViewStates.Visible)
            {
                var error = await Presenter.TryEditComment(AppSettings.User.UserInfo, _post, _editComment, _textInput.Text, AppSettings.AppInfo);

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
                    _comments.MoveToPosition(Presenter.Count);

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
                        if (AppSettings.User.IsAuthenticated)
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

                        if (AppSettings.User.Login != post.Author)
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
                        if (AppSettings.User.IsAuthenticated)
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
                        _editComment.Editing = true;
                        _textInput.Text = post.Body;
                        _textInput.SetSelection(post.Body.Length);
                        _textInputShape.SetColor(Style.R254G249B229);
                        _textInput.Background = _textInputShape;
                        _editControls.Visibility = ViewStates.Visible;
                        _postBtn.Visibility = ViewStates.Gone;
                        _rootLayout.ViewTreeObserver.GlobalLayout += ViewTreeObserverOnGlobalLayout;
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

        private void ViewTreeObserverOnGlobalLayout(object sender, EventArgs e)
        {
            if (_rootLayout.RootView.Height - _rootLayout.Height > BitmapUtils.DpToPixel(128, Resources))
            {
                _adapter.MItemManager.CloseAllItems();
                _adapter.SwipeEnabled = false;
                _adapter.NotifyDataSetChanged();
                var editPos = Presenter.IndexOf(_editComment) + 1;
                _comments.ScrollToPosition(editPos);
                _rootLayout.ViewTreeObserver.GlobalLayout -= ViewTreeObserverOnGlobalLayout;
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

        public override bool OnBackPressed()
        {
            if (_editComment != null && _editComment.Editing)
            {
                CommentEditCancelBtnOnClick(null, null);
                return true;
            }
            return base.OnBackPressed();
        }

        private void CommentEditCancelBtnOnClick(object sender, EventArgs eventArgs)
        {
            if (_editComment != null)
                _editComment.Editing = false;
            _textInput.Text = string.Empty;
            _textInputShape.SetColor(Color.White);
            _textInput.Background = _textInputShape;
            _editControls.Visibility = ViewStates.Gone;
            _postBtn.Visibility = ViewStates.Visible;
            _adapter.SwipeEnabled = true;
            _adapter.NotifyDataSetChanged();
            _rootLayout.ViewTreeObserver.GlobalLayout -= ViewTreeObserverOnGlobalLayout;
            HideKeyboard();
        }

        private void SaveOnClick(object sender, EventArgs e)
        {
            OnPost(null, null);
            CommentEditCancelBtnOnClick(null, null);
        }
    }
}
