using System;
using System.Collections.Generic;
using System.IO;
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
using Com.Lilarcor.Cheeseknife;
using Java.IO;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Errors;
using Steepshot.Core.Localization;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PostEditFragment : BaseFragmentWithPresenter<PostDescriptionPresenter>
    {
        private readonly TimeSpan PostingLimit = TimeSpan.FromMinutes(5);

        private readonly List<GalleryMediaModel> _media;
        private readonly Post _editPost;
        private Timer _timer;
        private GalleryHorizontalAdapter _galleryAdapter;

        private GalleryHorizontalAdapter GalleryAdapter => _galleryAdapter ?? (_galleryAdapter = _media == null ? new GalleryHorizontalAdapter(_editPost) : new GalleryHorizontalAdapter(_media));
        private SelectedTagsAdapter _localTagsAdapter;
        private SelectedTagsAdapter LocalTagsAdapter => _localTagsAdapter ?? (_localTagsAdapter = new SelectedTagsAdapter());
        private TagsAdapter _tagsAdapter;
        private TagsAdapter TagsAdapter => _tagsAdapter ?? (_tagsAdapter = new TagsAdapter(Presenter));
        private PreparePostModel _model;
        private string _previousQuery;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.btn_back)] private ImageButton _backButton;
        [InjectView(Resource.Id.root_layout)] private RelativeLayout _rootLayout;
        [InjectView(Resource.Id.photos)] private RecyclerView _photos;
        [InjectView(Resource.Id.photos_layout)] private RelativeLayout _photosContainer;
        [InjectView(Resource.Id.title)] private EditText _title;
        [InjectView(Resource.Id.title_layout)] private RelativeLayout _titleContainer;
        [InjectView(Resource.Id.description)] private EditText _description;
        [InjectView(Resource.Id.description_layout)] private RelativeLayout _descriptionContainer;
        [InjectView(Resource.Id.tag)] private NewTextEdit _tag;
        [InjectView(Resource.Id.local_tags_list)] private RecyclerView _localTagsList;
        [InjectView(Resource.Id.flow_tags)] private FlowLayout _tagsFlow;
        [InjectView(Resource.Id.tags_layout)] private LinearLayout _tagsContainer;
        [InjectView(Resource.Id.tags_list)] private RecyclerView _tagsList;
        [InjectView(Resource.Id.tags_list_layout)] private LinearLayout _tagsListContainer;
        [InjectView(Resource.Id.btn_post)] private Button _postButton;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _loadingSpinner;
        [InjectView(Resource.Id.btn_post_layout)] private RelativeLayout _postBtnContainer;
        [InjectView(Resource.Id.page_title)] private TextView _pageTitle;
        [InjectView(Resource.Id.top_margin_tags_layout)] private RelativeLayout _topMarginTagsLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_post_description, null);
                Cheeseknife.Inject(this, InflatedView);
            }

            return InflatedView;
        }
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

            _photos.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
            _photos.SetAdapter(GalleryAdapter);
            _photos.AddItemDecoration(new ListItemDecoration((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Resources.DisplayMetrics)));

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
            _model = new PreparePostModel(BasePresenter.User.UserInfo);
            SetPostingTimer();

            SearchTextChanged();
        }
        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
            GC.Collect(0);
        }

        public PostEditFragment(List<GalleryMediaModel> media)
        {
            _media = media;
        }

        public PostEditFragment(Post post)
        {
            _editPost = post;
        }

        private async void SetPostingTimer()
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
        private void PresenterSourceChanged(Status status)
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
            _postButton.Enabled = false;
            _title.Enabled = false;
            _description.Enabled = false;
            _tag.Enabled = false;
            _localTagsAdapter.Enabled = false;
            _postButton.Text = string.Empty;
            _loadingSpinner.Visibility = ViewStates.Visible;
            await OnPostAsync();
        }
        private async Task OnPostAsync()
        {
            var isConnected = BasePresenter.ConnectionService.IsConnectionAvailable();

            if (!isConnected)
            {
                Activity.ShowAlert(LocalizationKeys.InternetUnavailable);
                OnUploadEnded();
                return;
            }

            if (string.IsNullOrEmpty(_title.Text))
            {
                Activity.ShowAlert(LocalizationKeys.EmptyTitleField, ToastLength.Long);
                OnUploadEnded();
                return;
            }

            if (_editPost == null)
            {
                _model.Media = new MediaModel[_media.Count];
                for (int i = 0; i < _media.Count; i++)
                {
                    var operationResult = await UploadPhoto(Android.Net.Uri.Parse(_media[i].Path).Path);
                    if (!IsInitialized)
                        return;

                    if (!operationResult.IsSuccess)
                    {
                        //((SplashActivity)Activity).Cache.EvictAll();
                        operationResult = await UploadPhoto(_media[i].Path);

                        if (!IsInitialized)
                            return;
                    }

                    if (!operationResult.IsSuccess)
                    {
                        Activity.ShowAlert(operationResult.Error);
                        OnUploadEnded();
                        return;
                    }

                    _model.Media[i] = operationResult.Result;
                }
            }
            else
            {
                _model.Media = _editPost.Media;
            }

            _model.Title = _title.Text;
            _model.Description = _description.Text;
            _model.Tags = _localTagsAdapter.LocalTags.ToArray();
            TryCreateOrEditPost();
        }
        private async Task<OperationResult<MediaModel>> UploadPhoto(string path)
        {
            Stream stream = null;
            FileInputStream fileInputStream = null;

            try
            {
                var photo = new Java.IO.File(path);
                fileInputStream = new FileInputStream(photo);
                stream = new StreamConverter(fileInputStream, null);

                var request = new UploadMediaModel(BasePresenter.User.UserInfo, stream, Path.GetExtension(path));
                var serverResult = await Presenter.TryUploadMedia(request);
                return serverResult;
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
                return new OperationResult<MediaModel>(new AppError(LocalizationKeys.PhotoProcessingError));
            }
            finally
            {
                fileInputStream?.Close(); // ??? change order?
                stream?.Flush();
                fileInputStream?.Dispose();
                stream?.Dispose();
            }
        }
        private async void TryCreateOrEditPost()
        {
            if (_model.Media == null)
            {
                OnUploadEnded();
                return;
            }

            var resp = await Presenter.TryCreateOrEditPost(_model);
            if (!IsInitialized)
                return;

            if (resp.IsSuccess)
            {
                BasePresenter.User.UserInfo.LastPostTime = DateTime.Now;
                OnUploadEnded();
                BasePresenter.ProfileUpdateType = ProfileUpdateType.Full;
                Activity.ShowAlert(LocalizationKeys.PostDelay, ToastLength.Long);
                Activity.Finish();
            }
            else
            {
                Activity.ShowInteractiveMessage(resp.Error, TryAgainAction, ForgetAction);
            }
        }
        private void OnUploadEnded()
        {
            _postButton.Enabled = true;
            _postButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PublishButtonText);

            _loadingSpinner.Visibility = ViewStates.Gone;

            _title.Enabled = true;
            _description.Enabled = true;
            _tag.Enabled = true;
            _localTagsAdapter.Enabled = true;
        }
        private void ForgetAction(object o, DialogClickEventArgs dialogClickEventArgs)
        {
            OnUploadEnded();
        }
        private void TryAgainAction(object o, DialogClickEventArgs dialogClickEventArgs)
        {
            TryCreateOrEditPost();
        }

        private void LocalTagsAdapterClick(string tag)
        {
            if (!_localTagsAdapter.Enabled)
                return;

            _localTagsAdapter.LocalTags.Remove(tag);
            _localTagsAdapter.NotifyDataSetChanged();
            RemoveFlowTag(tag);
        }
        private void OnTagOnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                Activity.Window.SetSoftInputMode(SoftInput.AdjustResize);
                AnimateTagsLayout(true);
            }
        }
        private void OnTagOnTextChanged(object sender, TextChangedEventArgs e)
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
        private void OnTagsAdapterClick(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return;

            AddTag(tag);
            _tag.Text = string.Empty;
        }
        private void OnTagsLayoutClick(object sender, EventArgs e)
        {
            if (!_tag.Enabled)
                return;
            _tag.RequestFocus();
            var imm = Activity.GetSystemService(Context.InputMethodService) as InputMethodManager;
            imm?.ShowSoftInput(_tag, ShowFlags.Implicit);
        }
        private void AnimateTagsLayout(bool openTags)
        {
            TransitionManager.BeginDelayedTransition(_rootLayout);
            _localTagsList.Visibility = _tagsListContainer.Visibility = openTags ? ViewStates.Visible : ViewStates.Gone;
            _photos.Visibility = _titleContainer.Visibility =
               _descriptionContainer.Visibility = _tagsFlow.Visibility = _postBtnContainer.Visibility = openTags ? ViewStates.Gone : ViewStates.Visible;
        }
        private void AddTag(string tag)
        {
            tag = tag.Trim();
            if (_localTagsAdapter.LocalTags.Count >= 20 || _localTagsAdapter.LocalTags.Any(t => t == tag))
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
        private void AddFlowTag(string tag)
        {
            var flowView = LayoutInflater.From(Activity).Inflate(Resource.Layout.lyt_local_tags_item, null);
            var flowViewTag = flowView.FindViewById<TextView>(Resource.Id.tag);
            flowView.Tag = tag;
            flowViewTag.Text = tag;
            _tagsFlow.AddView(flowView);
        }
        private void RemoveFlowTag(string tag)
        {
            var flowView = _tagsFlow.FindViewWithTag(tag);
            if (flowView != null)
                _tagsFlow.RemoveView(flowView);
        }
        private void OnTimer(object state)
        {
            Activity.RunOnUiThread(async () =>
            {
                await SearchTextChanged();
            });
        }
        private async Task SearchTextChanged()
        {
            if (_previousQuery == _tag.Text || _tag.Text.Length == 1)
                return;

            _previousQuery = _tag.Text;
            _tagsList.ScrollToPosition(0);
            Presenter.Clear();

            ErrorBase error = null;
            if (_tag.Text.Length == 0)
                error = await Presenter.TryGetTopTags();
            else if (_tag.Text.Length > 1)
                error = await Presenter.TryLoadNext(_tag.Text);

            if (IsInitialized)
                return;

            Activity.ShowAlert(error);
        }
        private void HideTagsList()
        {
            var txt = _tag.Text = _tag.Text.Trim();
            if (!string.IsNullOrEmpty(txt))
            {
                _tag.Text = string.Empty;
                AddTag(txt);
            }

            Activity.Window.SetSoftInputMode(SoftInput.AdjustPan);
            AnimateTagsLayout(false);
            ((BaseActivity)Activity).HideKeyboard();
            _tag.ClearFocus();
        }

        private void OnBack(object sender, EventArgs e)
        {
            if (_tag.HasFocus)
                HideTagsList();
            else
                Activity.OnBackPressed();
        }
        private void OnRootLayoutClick(object sender, EventArgs e)
        {
            ((BaseActivity)Activity).HideKeyboard();
        }
    }
}