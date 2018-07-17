using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Apmem;
using CheeseBind;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Errors;
using Steepshot.Core.Facades;
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

        protected TimeSpan PostingLimit;
        protected Timer _timer;
        protected GalleryHorizontalAdapter _galleryAdapter;
        protected SelectedTagsAdapter _localTagsAdapter;
        protected TagsAdapter _tagsAdapter;
        protected PreparePostModel _model;
        protected string _previousQuery;
        protected TagPickerFacade _tagPickerFacade;
        protected bool isSpammer;

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
        [BindView(Resource.Id.add_tag)] protected TextView _tagLabel;
        [BindView(Resource.Id.clear_edit)] protected ImageView _clearEdit;
        [BindView(Resource.Id.tag_label)] protected RelativeLayout _tagLabelContainer;
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

        protected TagsAdapter TagsAdapter => _tagsAdapter ?? (_tagsAdapter = new TagsAdapter(_tagPickerFacade));
        protected SelectedTagsAdapter LocalTagsAdapter => _localTagsAdapter ?? (_localTagsAdapter = new SelectedTagsAdapter());

        #endregion

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_post_description, null);
                Cheeseknife.Bind(this, InflatedView);
            }

            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            _tag.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.Hashtag);
            _tag.SetFilters(new IInputFilter[] { new TextInputFilter(TextInputFilter.TagFilter), new InputFilterLengthFilter(40) });
            _tagLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Hashtag);
            _title.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.EnterPostTitle);
            _description.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.EnterPostDescription);
            _postButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PublishButtonText);
            _pageTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PostSettings);

            _pageTitle.Typeface = Style.Semibold;
            _title.Typeface = Style.Regular;
            _description.Typeface = Style.Regular;
            _postButton.Typeface = Style.Semibold;
            _tagLabel.Typeface = Style.Regular;

            _postButton.Click += OnPost;
            _postButton.Enabled = true;

            _topPanel.BringToFront();

            _localTagsList.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
            LocalTagsAdapter.Click += LocalTagsAdapterClick;
            _localTagsList.SetAdapter(LocalTagsAdapter);
            _localTagsList.AddItemDecoration(new ListItemDecoration((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 15, Resources.DisplayMetrics)));

            _tagPickerFacade = new TagPickerFacade(_localTagsAdapter.LocalTags);
            _tagPickerFacade.SourceChanged += TagPickerFacadeOnSourceChanged;

            _tagsList.SetLayoutManager(new LinearLayoutManager(Activity));
            TagsAdapter.Click += OnTagsAdapterClick;
            _tagsList.SetAdapter(TagsAdapter);

            _tagLabel.Click += TagLabelOnClick;
            _tag.TextChanged += OnTagOnTextChanged;
            _tag.KeyboardDownEvent += HideTagsList;
            _tag.OkKeyEvent += HideTagsList;
            _clearEdit.Click += (sender, args) => _tag.Text = string.Empty;

            _backButton.Click += OnBack;
            _rootLayout.Click += OnRootLayoutClick;

            _timer = new Timer(OnTimer);
            _model = new PreparePostModel(AppSettings.User.UserInfo, AppSettings.AppInfo.GetModel());
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
            GC.Collect(0);
        }

        private void TagPickerFacadeOnSourceChanged(Status obj)
        {
            if (!IsInitialized)
                return;

            Activity.RunOnUiThread(() =>
            {
                _tagsAdapter.NotifyDataSetChanged();
            });
        }

        private async void OnPost(object sender, EventArgs e)
        {
            _postButton.Text = string.Empty;
            EnablePostAndEdit(false);

            var isConnected = BasePresenter.ConnectionService.IsConnectionAvailable();

            if (!isConnected)
            {
                Activity.ShowAlert(LocalizationKeys.InternetUnavailable);
                EnabledPost();
                return;
            }

            if (string.IsNullOrEmpty(_title.Text))
            {
                Activity.ShowAlert(LocalizationKeys.EmptyTitleField, ToastLength.Long);
                EnabledPost();
                return;
            }

            await OnPostAsync();
        }

        protected abstract Task OnPostAsync();

        protected void EnablePostAndEdit(bool enabled)
        {
            if (enabled)
                _loadingSpinner.Visibility = ViewStates.Gone;
            else
                _loadingSpinner.Visibility = ViewStates.Visible;

            _postButton.Enabled = enabled;
            _title.Enabled = enabled;
            _description.Enabled = enabled;
            _tag.Enabled = enabled;
            _localTagsAdapter.Enabled = enabled;
            _tagLabel.Enabled = enabled;
        }

        protected void EnabledPost()
        {
            _postButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PublishButtonText);
            EnablePostAndEdit(true);
        }

        protected void ForgetAction(object o, DialogClickEventArgs dialogClickEventArgs)
        {
            EnabledPost();
        }

        protected void TryAgainAction(object o, DialogClickEventArgs dialogClickEventArgs)
        {
            TryCreateOrEditPost();
        }

        protected async Task<bool> TryCreateOrEditPost()
        {
            if (_model.Media == null)
            {
                EnabledPost();
                return false;
            }

            var resp = await Presenter.TryCreateOrEditPost(_model);
            if (!IsInitialized)
                return false;

            if (resp.IsSuccess)
            {
                AppSettings.User.UserInfo.LastPostTime = DateTime.Now;
                EnabledPost();
                BasePresenter.ProfileUpdateType = ProfileUpdateType.Full;
                if (Activity is SplashActivity || Activity is CameraActivity)
                    Activity.Finish();
                else
                    ((BaseActivity)Activity).OnBackPressed();
                return true;
            }
            else
            {
                Activity.ShowInteractiveMessage(resp.Error, TryAgainAction, ForgetAction);
                return false;
            }
        }

        private void TagLabelOnClick(object sender, EventArgs e)
        {
            AnimateTagsLayout(true);
            _tag.RequestFocus();
            ((BaseActivity)Activity).OpenKeyboard(_tag);
        }

        protected void LocalTagsAdapterClick(string tag)
        {
            if (!_localTagsAdapter.Enabled)
                return;

            RemoveTag(tag);
            var index = _tagsAdapter.IndexOfTag(tag);
            if (index != -1)
                _tagsAdapter.NotifyItemInserted(index);
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

                _clearEdit.Visibility = ViewStates.Visible;
            }
            else
            {
                _clearEdit.Visibility = ViewStates.Invisible;
            }
            _timer.Change(500, Timeout.Infinite);
        }

        protected void OnTagsAdapterClick(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return;

            var index = _tagsAdapter.IndexOfTag(tag); ;
            if (AddTag(tag) && index != -1)
                _tagsAdapter.NotifyItemRemoved(index);

            _tag.Text = string.Empty;
        }

        protected void AnimateTagsLayout(bool openTags)
        {
            _pageTitle.Text = AppSettings.LocalizationManager.GetText(openTags ? LocalizationKeys.AddHashtag : LocalizationKeys.PostSettings);
            _tag.Visibility = _tagsListContainer.Visibility = openTags ? ViewStates.Visible : ViewStates.Gone;
            _photosContainer.Visibility = _titleContainer.Visibility = _descriptionContainer.Visibility = _tagLabelContainer.Visibility = _tagsFlow.Visibility = _postBtnContainer.Visibility = openTags ? ViewStates.Gone : ViewStates.Visible;
            _localTagsList.Visibility = openTags && _localTagsAdapter.LocalTags.Count > 0 ? ViewStates.Visible : ViewStates.Gone;
        }

        protected bool AddTag(string tag)
        {
            tag = tag.Trim();
            if (string.IsNullOrWhiteSpace(tag) || _localTagsAdapter.LocalTags.Count >= 20 || _localTagsAdapter.LocalTags.Any(t => t == tag))
                return false;

            AddFlowTag(tag);
            _localTagsAdapter.LocalTags.Add(tag);
            _localTagsAdapter.NotifyDataSetChanged();
            _localTagsList.MoveToPosition(_localTagsAdapter.LocalTags.Count - 1);
            if (_localTagsAdapter.LocalTags.Count == 1)
                _localTagsList.Visibility = ViewStates.Visible;
            return true;
        }

        protected void AddFlowTag(string tag)
        {
            var flowView = LayoutInflater.From(Activity).Inflate(Resource.Layout.lyt_local_tags_item, null);
            var flowViewTag = flowView.FindViewById<TextView>(Resource.Id.tag);
            flowView.Tag = tag;
            flowViewTag.Text = tag;
            flowViewTag.Typeface = Style.Light;
            flowView.Click += (sender, args) => RemoveTag(tag);
            var margin = (int)BitmapUtils.DpToPixel(5, Resources);
            var layoutParams = new FlowLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            layoutParams.SetMargins(0, margin, margin, margin);
            flowView.LayoutParameters = layoutParams;
            _tagsFlow.AddView(flowView, layoutParams);
        }

        protected void RemoveTag(string tag)
        {
            _localTagsAdapter.LocalTags.Remove(tag);
            _localTagsAdapter.NotifyDataSetChanged();
            RemoveFlowTag(tag);
            if (_localTagsAdapter.LocalTags.Count == 0)
                _localTagsList.Visibility = ViewStates.Gone;
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

            if (_previousQuery == text || text.Length == 1)
                return;

            _previousQuery = text;
            _tagsList.ScrollToPosition(0);
            _tagPickerFacade.Clear();

            ErrorBase error = null;
            if (text.Length == 0)
                error = await _tagPickerFacade.TryGetTopTags();
            else if (text.Length > 1)
                error = await _tagPickerFacade.TryLoadNext(text);

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

            ((BaseActivity)Activity).HideKeyboard();
            if (_tag.HasFocus)
                _tagsList.RequestFocus();
            else
                AnimateTagsLayout(false);
        }

        protected void OnBack(object sender, EventArgs e)
        {
            if (!OnBackPressed())
                ((BaseActivity)Activity).OnBackPressed();
        }

        protected void OnRootLayoutClick(object sender, EventArgs e)
        {
            ((BaseActivity)Activity).HideKeyboard();
        }

        public override bool OnBackPressed()
        {
            if (_tag.Visibility == ViewStates.Visible)
            {
                HideTagsList();
                return true;
            }

            return base.OnBackPressed();
        }
    }
}