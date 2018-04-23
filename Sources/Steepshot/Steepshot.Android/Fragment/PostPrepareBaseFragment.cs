using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.Transitions;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Apmem;
using CheeseBind;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Errors;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.CustomViews;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public abstract class PostPrepareBaseFragment : BaseFragmentWithPresenter<PostDescriptionPresenter>
    {
        #region Fields

        protected readonly TimeSpan PostingLimit = TimeSpan.FromMinutes(5);
        protected Timer _timer;
        protected GalleryHorizontalAdapter _galleryAdapter;
        protected SelectedTagsAdapter _localTagsAdapter;
        protected TagsAdapter _tagsAdapter;
        protected PreparePostModel _model;
        protected string _previousQuery;


#pragma warning disable 0649, 4014
        [BindView(Resource.Id.btn_back)] protected ImageButton _backButton;
        [BindView(Resource.Id.root_layout)] protected RelativeLayout _rootLayout;
        [BindView(Resource.Id.photos)] protected RecyclerView _photos;
        [BindView(Resource.Id.ratio_switch)] protected ImageButton _ratioBtn;
        [BindView(Resource.Id.rotate)] protected ImageButton _rotateBtn;
        [BindView(Resource.Id.photo_preview)] protected CropView _preview;
        [BindView(Resource.Id.photo_preview_container)] protected RelativeLayout _previewContainer;
        [BindView(Resource.Id.photos_layout)] protected RelativeLayout _photosContainer;
        [BindView(Resource.Id.title)] protected EditText _title;
        [BindView(Resource.Id.title_layout)] protected RelativeLayout _titleContainer;
        [BindView(Resource.Id.description)] protected EditText _description;
        [BindView(Resource.Id.description_layout)] protected RelativeLayout _descriptionContainer;
        [BindView(Resource.Id.scroll_container)] protected ScrollView _descriptionScrollContainer;
        [BindView(Resource.Id.tag)] protected NewTextEdit _tag;
        [BindView(Resource.Id.local_tags_list)] protected RecyclerView _localTagsList;
        [BindView(Resource.Id.flow_tags)] protected FlowLayout _tagsFlow;
        [BindView(Resource.Id.tags_layout)] protected LinearLayout _tagsContainer;
        [BindView(Resource.Id.tags_list)] protected RecyclerView _tagsList;
        [BindView(Resource.Id.tags_list_layout)] protected LinearLayout _tagsListContainer;
        [BindView(Resource.Id.btn_post)] protected Button _postButton;
        [BindView(Resource.Id.loading_spinner)] protected ProgressBar _loadingSpinner;
        [BindView(Resource.Id.btn_post_layout)] protected RelativeLayout _postBtnContainer;
        [BindView(Resource.Id.page_title)] protected TextView _pageTitle;
        [BindView(Resource.Id.top_margin_tags_layout)] protected RelativeLayout _topMarginTagsLayout;
        [BindView(Resource.Id.toolbar)] protected LinearLayout _topPanel;

        #endregion

        #region Properties

        protected TagsAdapter TagsAdapter => _tagsAdapter ?? (_tagsAdapter = new TagsAdapter(Presenter));
        protected SelectedTagsAdapter LocalTagsAdapter => _localTagsAdapter ?? (_localTagsAdapter = new SelectedTagsAdapter());

        #endregion


        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            _tag.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.AddHashtag);
            _title.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.EnterPostTitle);
            _description.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.EnterPostDescription);
            _postButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PublishButtonText);
            _pageTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PostSettings);

            _pageTitle.Typeface = Style.Semibold;
            _title.Typeface = Style.Regular;
            _description.Typeface = Style.Regular;
            _postButton.Typeface = Style.Semibold;

            _postButton.Click += OnPost;
            _postButton.Enabled = true;

            _topPanel.BringToFront();

            _localTagsList.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
            LocalTagsAdapter.Click += LocalTagsAdapterClick;
            _localTagsList.SetAdapter(LocalTagsAdapter);
            _localTagsList.AddItemDecoration(new ListItemDecoration((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 15, Resources.DisplayMetrics)));

            _tagsList.SetLayoutManager(new LinearLayoutManager(Activity));
            Presenter.SourceChanged += PresenterSourceChanged;
            TagsAdapter.Click += OnTagsAdapterClick;
            _tagsList.SetAdapter(TagsAdapter);

            _tag.TextChanged += OnTagOnTextChanged;
            _tag.KeyboardDownEvent += HideTagsList;
            _tag.OkKeyEvent += HideTagsList;
            _tag.FocusChange += OnTagOnFocusChange;

            _topMarginTagsLayout.Click += OnTagsLayoutClick;
            _backButton.Click += OnBack;
            _rootLayout.Click += OnRootLayoutClick;

            _timer = new Timer(OnTimer);
            _model = new PreparePostModel(BasePresenter.User.UserInfo, AppSettings.AppInfo.GetModel());
            SetPostingTimer();


        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_post_description, null);
                Cheeseknife.Bind(this, InflatedView);
            }

            return InflatedView;
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
            GC.Collect(0);
        }


        protected async void OnPost(object sender, EventArgs e)
        {
            _postButton.Enabled = false;
            _title.Enabled = false;
            _description.Enabled = false;
            _tag.Enabled = false;
            _localTagsAdapter.Enabled = false;
            _postButton.Text = string.Empty;
            _loadingSpinner.Visibility = ViewStates.Visible;
            await OnPostAsync();
        }

        protected abstract Task OnPostAsync();

        protected async void SetPostingTimer()
        {
            var timepassed = DateTime.Now - BasePresenter.User.UserInfo.LastPostTime;
            _postButton.Enabled = false;
            while (timepassed < PostingLimit)
            {
                _postButton.Text = (PostingLimit - timepassed).ToString("mm\\:ss");
                await Task.Delay(1000);
                if (!IsInitialized)
                    return;
                timepassed = DateTime.Now - BasePresenter.User.UserInfo.LastPostTime;
            }
            _postButton.Enabled = true;
            _postButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PublishButtonText);
        }

        protected void PresenterSourceChanged(Status status)
        {
            if (!IsInitialized)
                return;

            Activity.RunOnUiThread(() =>
            {
                _tagsAdapter.NotifyDataSetChanged();
            });
        }

        protected void EnabledPost()
        {
            _postButton.Enabled = true;
            _postButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PublishButtonText);

            _loadingSpinner.Visibility = ViewStates.Gone;

            _title.Enabled = true;
            _description.Enabled = true;
            _tag.Enabled = true;
            _localTagsAdapter.Enabled = true;
        }

        protected void ForgetAction(object o, DialogClickEventArgs dialogClickEventArgs)
        {
            EnabledPost();
        }

        protected void TryAgainAction(object o, DialogClickEventArgs dialogClickEventArgs)
        {
            TryCreateOrEditPost();
        }

        protected async void TryCreateOrEditPost()
        {
            if (_model.Media == null)
            {
                EnabledPost();
                return;
            }

            var resp = await Presenter.TryCreateOrEditPost(_model);
            if (!IsInitialized)
                return;

            if (resp.IsSuccess)
            {
                BasePresenter.User.UserInfo.LastPostTime = DateTime.Now;
                EnabledPost();
                BasePresenter.ProfileUpdateType = ProfileUpdateType.Full;
                Activity.ShowAlert(LocalizationKeys.PostDelay, ToastLength.Long);
                if (Activity is SplashActivity || Activity is CameraActivity)
                    Activity.Finish();
                else
                    ((BaseActivity)Activity).OnBackPressed();
            }
            else
            {
                Activity.ShowInteractiveMessage(resp.Error, TryAgainAction, ForgetAction);
            }
        }

        protected void LocalTagsAdapterClick(string tag)
        {
            if (!_localTagsAdapter.Enabled)
                return;

            _localTagsAdapter.LocalTags.Remove(tag);
            _localTagsAdapter.NotifyDataSetChanged();
            RemoveFlowTag(tag);
        }

        protected void OnTagOnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                Activity.Window.SetSoftInputMode(SoftInput.AdjustResize);
                AnimateTagsLayout(true);
            }
            else
                AnimateTagsLayout(false);
        }

        protected void OnTagOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = e.Text.ToString();
            if (!string.IsNullOrWhiteSpace(txt))
            {
                if (txt.EndsWith(" "))
                {
                    _tag.Text = string.Empty;
                    AddTag(txt);
                }
            }
            _timer.Change(500, Timeout.Infinite);
        }

        protected void OnTagsAdapterClick(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return;

            AddTag(tag);
            _tag.Text = string.Empty;
        }

        protected void OnTagsLayoutClick(object sender, EventArgs e)
        {
            if (!_tag.Enabled)
                return;
            _tag.RequestFocus();
            var imm = Activity.GetSystemService(Context.InputMethodService) as InputMethodManager;
            imm?.ShowSoftInput(_tag, ShowFlags.Implicit);
        }

        protected void AnimateTagsLayout(bool openTags)
        {
            TransitionManager.BeginDelayedTransition(_rootLayout);
            _localTagsList.Visibility = _tagsListContainer.Visibility = openTags ? ViewStates.Visible : ViewStates.Gone;
            _photosContainer.Visibility = _titleContainer.Visibility =
               _descriptionContainer.Visibility = _tagsFlow.Visibility = _postBtnContainer.Visibility = openTags ? ViewStates.Gone : ViewStates.Visible;
        }

        protected void AddTag(string tag)
        {
            tag = tag.NormalizeTag();
            tag = tag.Trim();
            if (string.IsNullOrWhiteSpace(tag) || _localTagsAdapter.LocalTags.Count >= 20 || _localTagsAdapter.LocalTags.Any(t => t == tag))
                return;

            AddFlowTag(tag);
            _localTagsAdapter.LocalTags.Add(tag);
            Activity.RunOnUiThread(() =>
            {
                _localTagsAdapter.NotifyDataSetChanged();
                _localTagsList.MoveToPosition(_localTagsAdapter.LocalTags.Count - 1);
                if (_localTagsAdapter.LocalTags.Count == 1)
                    _localTagsList.Visibility = ViewStates.Visible;
            });
        }

        protected void AddFlowTag(string tag)
        {
            var flowView = LayoutInflater.From(Activity).Inflate(Resource.Layout.lyt_local_tags_item, null);
            var flowViewTag = flowView.FindViewById<TextView>(Resource.Id.tag);
            flowView.Tag = tag;
            flowViewTag.Text = tag;
            flowViewTag.Typeface = Style.Light;
            flowView.Click += (sender, args) =>
            {
                _localTagsAdapter.LocalTags.Remove(tag);
                _localTagsAdapter.NotifyDataSetChanged();
                RemoveFlowTag(tag);
            };
            var margin = (int)BitmapUtils.DpToPixel(5, Resources);
            var layoutParams = new FlowLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            layoutParams.SetMargins(0, margin, margin, margin);
            flowView.LayoutParameters = layoutParams;
            _tagsFlow.AddView(flowView, layoutParams);
        }

        protected void RemoveFlowTag(string tag)
        {
            var flowView = _tagsFlow.FindViewWithTag(tag);
            if (flowView != null)
                _tagsFlow.RemoveView(flowView);
        }

        protected void OnTimer(object state)
        {
            Activity?.RunOnUiThread(async () =>
            {
                await SearchTextChanged();
            });
        }

        protected async Task SearchTextChanged()
        {
            var text = _tag.Text;
            text = text.NormalizeTag();

            if (_previousQuery == text || text.Length == 1)
                return;

            _previousQuery = text;
            _tagsList.ScrollToPosition(0);
            Presenter.Clear();

            ErrorBase error = null;
            if (text.Length == 0)
                error = await Presenter.TryGetTopTags();
            else if (text.Length > 1)
                error = await Presenter.TryLoadNext(text);

            if (IsInitialized)
                return;

            Activity.ShowAlert(error);
        }

        protected void HideTagsList()
        {
            var txt = _tag.Text = _tag.Text.Trim();
            if (!string.IsNullOrEmpty(txt))
            {
                _tag.Text = string.Empty;
                AddTag(txt);
            }

            Activity.Window.SetSoftInputMode(SoftInput.AdjustPan);
            ((BaseActivity)Activity).HideKeyboard();
            _tag.ClearFocus();
            _description.RequestFocus();
        }

        protected void OnBack(object sender, EventArgs e)
        {
            if (_tag.HasFocus)
                HideTagsList();
            else
                ((BaseActivity)Activity).OnBackPressed();
        }

        protected void OnRootLayoutClick(object sender, EventArgs e)
        {
            ((BaseActivity)Activity).HideKeyboard();
        }
    }
}