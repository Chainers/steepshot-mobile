using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
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
using Steepshot.Core;
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

        private SelectedTagsAdapter _localTagsAdapter;
        private PostSearchTagsAdapter _postSearchTagsAdapter;
        private TagPickerFacade _tagPickerFacade;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.btn_back)] protected ImageButton BackButton;
        [BindView(Resource.Id.root_layout)] protected RelativeLayout RootLayout;
        [BindView(Resource.Id.photos)] protected RecyclerView Photos;
        [BindView(Resource.Id.ratio_switch)] protected ImageButton RatioBtn;
        [BindView(Resource.Id.rotate)] protected ImageButton RotateBtn;
        [BindView(Resource.Id.photo_preview)] protected CropView Preview;
        [BindView(Resource.Id.photo_preview_container)] protected RelativeLayout PreviewContainer;
        [BindView(Resource.Id.photos_layout)] protected RelativeLayout PhotosContainer;
        [BindView(Resource.Id.title)] protected EditText Title;
        [BindView(Resource.Id.title_layout)] protected RelativeLayout TitleContainer;
        [BindView(Resource.Id.description)] protected EditText Description;
        [BindView(Resource.Id.description_layout)] protected RelativeLayout DescriptionContainer;
        [BindView(Resource.Id.scroll_container)] protected ScrollView DescriptionScrollContainer;
        [BindView(Resource.Id.tag)] protected NewTextEdit TagEdit;
        [BindView(Resource.Id.add_tag)] protected TextView TagLabel;
        [BindView(Resource.Id.clear_edit)] protected ImageView ClearEdit;
        [BindView(Resource.Id.tag_label)] protected RelativeLayout TagLabelContainer;
        [BindView(Resource.Id.local_tags_list)] protected RecyclerView LocalTagsList;
        [BindView(Resource.Id.flow_tags)] protected FlowLayout TagsFlow;
        [BindView(Resource.Id.tags_list)] protected RecyclerView TagsList;
        [BindView(Resource.Id.tags_list_layout)] protected LinearLayout TagsListContainer;
        [BindView(Resource.Id.btn_post)] protected Button PostButton;
        [BindView(Resource.Id.loading_spinner)] protected ProgressBar LoadingSpinner;
        [BindView(Resource.Id.btn_post_layout)] protected RelativeLayout PostBtnContainer;
        [BindView(Resource.Id.page_title)] protected TextView PageTitle;
        [BindView(Resource.Id.toolbar)] protected LinearLayout TopPanel;

        [BindView(Resource.Id.tags_layout)] protected LinearLayout TagsContainer;
        [BindView(Resource.Id.top_margin_tags_layout)] protected RelativeLayout TopMarginTagsLayout;

        #endregion

        #region Properties
        
        protected Timer Timer { get; set; }
        protected PreparePostModel Model { get; set; }
        protected string PreviousQuery { get; set; }
        protected bool? IsSpammer { get; set; }

        protected SelectedTagsAdapter LocalTagsAdapter => _localTagsAdapter ?? (_localTagsAdapter = new SelectedTagsAdapter());

        #endregion

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_post_description, null);
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

            TagEdit.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.Hashtag);
            TagEdit.SetFilters(new IInputFilter[] { new TextInputFilter(TextInputFilter.TagFilter), new InputFilterLengthFilter(40) });
            TagLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Hashtag);
            Title.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.EnterPostTitle);
            Description.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.EnterPostDescription);
            PostButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PublishButtonText);
            PageTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PostSettings);

            PageTitle.Typeface = Style.Semibold;
            Title.Typeface = Style.Regular;
            Description.Typeface = Style.Regular;
            PostButton.Typeface = Style.Semibold;
            TagLabel.Typeface = Style.Regular;

            PostButton.Click += OnPost;
            PostButton.Enabled = true;

            TopPanel.BringToFront();

            LocalTagsList.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
            LocalTagsAdapter.Click += LocalTagsAdapterClick;
            LocalTagsList.SetAdapter(LocalTagsAdapter);
            LocalTagsList.AddItemDecoration(new ListItemDecoration((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 15, Resources.DisplayMetrics)));

            var client = App.MainChain == KnownChains.Steem ? App.SteemClient : App.GolosClient;
            _tagPickerFacade = new TagPickerFacade(_localTagsAdapter.LocalTags);
            _tagPickerFacade.SetClient(client);
            _tagPickerFacade.SourceChanged += TagPickerFacadeOnSourceChanged;

            _postSearchTagsAdapter = new PostSearchTagsAdapter(_tagPickerFacade);

            TagsList.SetLayoutManager(new LinearLayoutManager(Activity));
            _postSearchTagsAdapter.Click += OnTagsAdapterClick;
            TagsList.SetAdapter(_postSearchTagsAdapter);

            TagLabel.Click += TagLabelOnClick;
            TagEdit.TextChanged += OnTagOnTextChanged;
            TagEdit.KeyboardDownEvent += HideTagsList;
            TagEdit.OkKeyEvent += HideTagsList;
            ClearEdit.Click += (sender, args) => TagEdit.Text = string.Empty;

            BackButton.Click += OnBack;
            RootLayout.Click += OnRootLayoutClick;

            Timer = new Timer(OnTimer);
            Model = new PreparePostModel(AppSettings.User.UserInfo, AppSettings.AppInfo.GetModel());
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
                _postSearchTagsAdapter.NotifyDataSetChanged();
            });
        }

        private async void OnPost(object sender, EventArgs e)
        {
            EnablePostAndEdit(false, false);

            if (string.IsNullOrEmpty(Title.Text))
            {
                Activity.ShowAlert(LocalizationKeys.EmptyTitleField, ToastLength.Long);
                EnabledPost();
                return;
            }

            var isConnected = AppSettings.ConnectionService.IsConnectionAvailable();

            if (!isConnected)
            {
                Activity.ShowAlert(LocalizationKeys.InternetUnavailable);
                EnabledPost();
                return;
            }

            await OnPostAsync();

            if (IsSpammer != true)
                EnablePostAndEdit(true, true);
        }

        protected abstract Task OnPostAsync();

        protected void EnablePostAndEdit(bool enabled, bool enableFields)
        {
            if (enabled)
            {
                LoadingSpinner.Visibility = ViewStates.Gone;
            }
            else
            {
                PostButton.Text = string.Empty;
                LoadingSpinner.Visibility = ViewStates.Visible;
            }

            PostButton.Enabled = enabled;

            if (enableFields)
            {
                Title.Enabled = enabled;
                Description.Enabled = enabled;
                TagEdit.Enabled = enabled;
                _localTagsAdapter.Enabled = enabled;
                TagLabel.Enabled = enabled;
            }
        }

        protected void EnabledPost()
        {
            PostButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PublishButtonText);
            EnablePostAndEdit(true, true);
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
            if (Model.Media == null)
                return false;

            var resp = await Presenter.TryCreateOrEditPostAsync(Model);

            if (!IsInitialized)
                return false;

            if (resp.IsSuccess)
            {
                AppSettings.User.UserInfo.LastPostTime = DateTime.Now;
                EnabledPost();
                AppSettings.ProfileUpdateType = ProfileUpdateType.Full;
                OnPostSuccess();
                if (Activity is SplashActivity || Activity is CameraActivity)
                    Activity.Finish();
                else
                    ((BaseActivity)Activity).OnBackPressed();
                return true;
            }

            Activity.ShowInteractiveMessage(resp.Exception, TryAgainAction, ForgetAction);
            return false;
        }

        protected virtual void OnPostSuccess()
        {
            //todo nothing
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == (int)Result.Ok)
            {
                EnablePostAndEdit(false, true);
                TryCreateOrEditPost();
            }
            else if (resultCode == (int)Result.Canceled)
            {
                EnabledPost();
                EnablePostAndEdit(true, true);
            }
        }

        private void TagLabelOnClick(object sender, EventArgs e)
        {
            AnimateTagsLayout(true);
            TagEdit.RequestFocus();
            ((BaseActivity)Activity).OpenKeyboard(TagEdit);
        }

        protected void LocalTagsAdapterClick(string tag)
        {
            if (!_localTagsAdapter.Enabled)
                return;

            RemoveTag(tag);
            var index = _postSearchTagsAdapter.IndexOfTag(tag);
            if (index != -1)
                _postSearchTagsAdapter.NotifyItemInserted(index);
        }

        protected void OnTagOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = e.Text.ToString();
            if (!string.IsNullOrWhiteSpace(txt))
            {
                if (txt.EndsWith(" "))
                {
                    TagEdit.Text = string.Empty;
                    AddTag(txt);
                }

                ClearEdit.Visibility = ViewStates.Visible;
            }
            else
            {
                ClearEdit.Visibility = ViewStates.Invisible;
            }
            Timer.Change(1300, Timeout.Infinite);
        }

        protected void OnTagsAdapterClick(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return;

            var index = _postSearchTagsAdapter.IndexOfTag(tag);
            if (AddTag(tag) && index != -1)
                _postSearchTagsAdapter.NotifyItemRemoved(index);

            TagEdit.Text = string.Empty;
        }

        protected void AnimateTagsLayout(bool openTags)
        {
            PageTitle.Text = AppSettings.LocalizationManager.GetText(openTags ? LocalizationKeys.AddHashtag : LocalizationKeys.PostSettings);
            TagEdit.Visibility = TagsListContainer.Visibility = openTags ? ViewStates.Visible : ViewStates.Gone;
            PhotosContainer.Visibility = TitleContainer.Visibility = DescriptionContainer.Visibility = TagLabelContainer.Visibility = TagsFlow.Visibility = PostBtnContainer.Visibility = openTags ? ViewStates.Gone : ViewStates.Visible;
            LocalTagsList.Visibility = openTags && _localTagsAdapter.LocalTags.Count > 0 ? ViewStates.Visible : ViewStates.Gone;
            if (!openTags)
            {
                DescriptionScrollContainer.Post(() =>
                DescriptionScrollContainer.FullScroll(FocusSearchDirection.Down));
            }
        }

        protected bool AddTag(string tag)
        {
            tag = tag.Trim();
            if (string.IsNullOrWhiteSpace(tag) || _localTagsAdapter.LocalTags.Count >= 20 || _localTagsAdapter.LocalTags.Any(t => t == tag))
                return false;

            AddFlowTag(tag);
            _localTagsAdapter.LocalTags.Add(tag);
            _localTagsAdapter.NotifyDataSetChanged();
            LocalTagsList.MoveToPosition(_localTagsAdapter.LocalTags.Count - 1);
            if (_localTagsAdapter.LocalTags.Count == 1)
                LocalTagsList.Visibility = ViewStates.Visible;
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
            TagsFlow.AddView(flowView, layoutParams);
        }

        protected void RemoveTag(string tag)
        {
            _localTagsAdapter.LocalTags.Remove(tag);
            _localTagsAdapter.NotifyDataSetChanged();
            RemoveFlowTag(tag);
            if (_localTagsAdapter.LocalTags.Count == 0)
                LocalTagsList.Visibility = ViewStates.Gone;
        }

        protected void RemoveFlowTag(string tag)
        {
            var flowView = TagsFlow.FindViewWithTag(tag);
            if (flowView != null)
                TagsFlow.RemoveView(flowView);
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
            var text = TagEdit.Text;

            if (PreviousQuery == text || text.Length == 1)
                return;

            PreviousQuery = text;
            TagsList.ScrollToPosition(0);
            _tagPickerFacade.Clear();

            Exception exception = null;
            if (text.Length == 0)
                exception = await _tagPickerFacade.TryGetTopTagsAsync();
            else if (text.Length > 1)
                exception = await _tagPickerFacade.TryLoadNextAsync(text);

            if (IsInitialized)
                return;

            Activity.ShowAlert(exception, ToastLength.Short);
        }

        protected void HideTagsList()
        {
            var txt = TagEdit.Text = TagEdit.Text.Trim();
            if (!string.IsNullOrEmpty(txt))
            {
                TagEdit.Text = string.Empty;
                AddTag(txt);
            }

            ((BaseActivity)Activity).HideKeyboard();
            if (TagEdit.HasFocus)
                TagsList.RequestFocus();
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
            if (TagEdit.Visibility == ViewStates.Visible)
            {
                HideTagsList();
                return true;
            }

            return base.OnBackPressed();
        }
    }
}