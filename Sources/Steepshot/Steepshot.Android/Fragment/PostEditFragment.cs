using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
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
using Square.Picasso;
using Steepshot.Activity;
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
using Steepshot.CustomViews;
using Steepshot.Utils;
using Path = System.IO.Path;
using Uri = Android.Net.Uri;

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
        [BindView(Resource.Id.btn_back)] private ImageButton _backButton;
        [BindView(Resource.Id.root_layout)] private RelativeLayout _rootLayout;
        [BindView(Resource.Id.photos)] private RecyclerView _photos;
        [BindView(Resource.Id.ratio_switch)] private ImageButton _ratioBtn;
        [BindView(Resource.Id.rotate)] private ImageButton _rotateBtn;
        [BindView(Resource.Id.photo_preview)] private CropView _preview;
        [BindView(Resource.Id.photo_preview_container)] private RelativeLayout _previewContainer;
        [BindView(Resource.Id.photos_layout)] private RelativeLayout _photosContainer;
        [BindView(Resource.Id.title)] private EditText _title;
        [BindView(Resource.Id.title_layout)] private RelativeLayout _titleContainer;
        [BindView(Resource.Id.description)] private EditText _description;
        [BindView(Resource.Id.description_layout)] private RelativeLayout _descriptionContainer;
        [BindView(Resource.Id.scroll_container)] private ScrollView _descriptionScrollContainer;
        [BindView(Resource.Id.tag)] private NewTextEdit _tag;
        [BindView(Resource.Id.local_tags_list)] private RecyclerView _localTagsList;
        [BindView(Resource.Id.flow_tags)] private FlowLayout _tagsFlow;
        [BindView(Resource.Id.tags_layout)] private LinearLayout _tagsContainer;
        [BindView(Resource.Id.tags_list)] private RecyclerView _tagsList;
        [BindView(Resource.Id.tags_list_layout)] private LinearLayout _tagsListContainer;
        [BindView(Resource.Id.btn_post)] private Button _postButton;
        [BindView(Resource.Id.loading_spinner)] private ProgressBar _loadingSpinner;
        [BindView(Resource.Id.btn_post_layout)] private RelativeLayout _postBtnContainer;
        [BindView(Resource.Id.page_title)] private TextView _pageTitle;
        [BindView(Resource.Id.top_margin_tags_layout)] private RelativeLayout _topMarginTagsLayout;
        [BindView(Resource.Id.toolbar)] private LinearLayout _topPanel;

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
            _model = new PreparePostModel(BasePresenter.User.UserInfo);
            SetPostingTimer();

            if (_editPost != null)
                SetEditPost();

            if (_media?.Count > 1 || _editPost?.Media.Length > 1)
            {
                _photos.Visibility = ViewStates.Visible;
                _previewContainer.Visibility = ViewStates.Gone;
                _photos.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                _photos.SetAdapter(GalleryAdapter);
                _photos.AddItemDecoration(new ListItemDecoration((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Resources.DisplayMetrics)));
            }
            else
            {
                _photos.Visibility = ViewStates.Gone;
                _previewContainer.Visibility = ViewStates.Visible;
                var margin = (int)BitmapUtils.DpToPixel(15, Resources);
                var layoutParams = new RelativeLayout.LayoutParams(Resources.DisplayMetrics.WidthPixels - margin * 2, Resources.DisplayMetrics.WidthPixels - margin * 2);
                layoutParams.SetMargins(margin, 0, margin, margin);
                _previewContainer.LayoutParameters = layoutParams;
                _preview.CornerRadius = BitmapUtils.DpToPixel(5, Resources);
                if (_media?[0].PreparedBitmap == null)
                {
                    _preview.SetImageUri(Uri.Parse(_media?[0].Path), _media?[0].Parameters);
                }
                else
                {
                    _ratioBtn.Visibility = _rotateBtn.Visibility = ViewStates.Gone;
                    if (_editPost != null)
                    {
                        var url = _editPost.Media[0].Thumbnails.Mini;
                        Picasso.With(Activity).Load(url)
                            .Resize(_previewContainer.LayoutParameters.Width, _previewContainer.LayoutParameters.Height)
                            .Into(_preview);
                    }
                    else if (_media?[0].PreparedBitmap != null)
                    {
                        _preview.SetImageBitmap(_media[0].PreparedBitmap);
                    }
                }
                _preview.Touch += PreviewOnTouch;
                _ratioBtn.Click += RatioBtnOnClick;
                _rotateBtn.Click += RotateBtnOnClick;
            }

            SearchTextChanged();
        }
        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
            GC.Collect(0);
        }

        private void SetEditPost()
        {
            _model = new PreparePostModel(BasePresenter.User.UserInfo, _editPost.Permlink);
            _title.Text = _editPost.Title;
            _title.SetSelection(_editPost.Title.Length);
            _description.Text = _editPost.Description;
            _description.SetSelection(_editPost.Description.Length);
            foreach (var editPostTag in _editPost.Tags)
            {
                AddTag(editPostTag);
            }
        }
        private void PreviewOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            if (_editPost != null || _media?[0].PreparedBitmap != null)
            {
                _descriptionScrollContainer.OnTouchEvent(touchEventArgs.Event);
                return;
            }
            _preview.OnTouchEvent(touchEventArgs.Event);
            if (touchEventArgs.Event.Action == MotionEventActions.Down)
                _descriptionScrollContainer.RequestDisallowInterceptTouchEvent(true);
            else if (touchEventArgs.Event.Action == MotionEventActions.Up)
                _descriptionScrollContainer.RequestDisallowInterceptTouchEvent(false);
        }
        private void RatioBtnOnClick(object sender, EventArgs eventArgs) => _preview.SwitchScale();
        private void RotateBtnOnClick(object sender, EventArgs eventArgs) => _preview.Rotate(_preview.DrawableImageParameters.Rotation + 90f);

        public PostEditFragment(List<GalleryMediaModel> media)
        {
            _media = media;
        }
        public PostEditFragment(GalleryMediaModel media)
        {
            _media = new List<GalleryMediaModel> { media };
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
                if (_media.Count == 1 && _media[0].PreparedBitmap == null)
                    _media[0].PreparedBitmap = _preview.Crop(Uri.Parse(_media[0].Path), _preview.DrawableImageParameters);
                for (int i = 0; i < _media.Count; i++)
                {
                    var bitmapStream = new MemoryStream();
                    _media[i].PreparedBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, bitmapStream);

                    var operationResult = await UploadPhoto(bitmapStream);
                    if (!IsInitialized)
                        return;

                    if (!operationResult.IsSuccess)
                    {
                        //((SplashActivity)Activity).Cache.EvictAll();
                        operationResult = await UploadPhoto(bitmapStream);

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
        private async Task<OperationResult<MediaModel>> UploadPhoto(Stream photoStream)
        {
            try
            {
                photoStream.Position = 0;
                var request = new UploadMediaModel(BasePresenter.User.UserInfo, photoStream, Path.GetExtension(".jpg"));
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
                photoStream?.Close();
                photoStream?.Dispose();
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
            else
                AnimateTagsLayout(false);
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
            _photosContainer.Visibility = _titleContainer.Visibility =
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
        private void RemoveFlowTag(string tag)
        {
            var flowView = _tagsFlow.FindViewWithTag(tag);
            if (flowView != null)
                _tagsFlow.RemoveView(flowView);
        }
        private void OnTimer(object state)
        {
            Activity?.RunOnUiThread(async () =>
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
            ((BaseActivity)Activity).HideKeyboard();
            _tag.ClearFocus();
            _description.RequestFocus();
        }

        private void OnBack(object sender, EventArgs e)
        {
            if (_tag.HasFocus)
                HideTagsList();
            else
                ((BaseActivity)Activity).OnBackPressed();
        }
        private void OnRootLayoutClick(object sender, EventArgs e) => ((BaseActivity)Activity).HideKeyboard();
    }
}