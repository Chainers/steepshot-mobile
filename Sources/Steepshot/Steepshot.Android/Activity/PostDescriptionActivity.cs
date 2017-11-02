using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Transitions;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Java.IO;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(Label = "PostDescriptionActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.StateHidden | SoftInput.AdjustPan)]
    public sealed class PostDescriptionActivity : BaseActivityWithPresenter<PostDescriptionPresenter>
    {
        public const string PhotoExtraPath = "PhotoExtraPath";
        public const string IsNeedCompressExtraPath = "SHOULD_COMPRESS";

        private string _path;
        private bool _shouldCompress;
        private Timer _timer;
        private Bitmap _btmp;
        private SelectedTagsAdapter _localTagsAdapter;
        private TagsAdapter _tagsAdapter;
        private UploadImageRequest _request;
        private UploadResponse _response;
        private string _previousQuery;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.title)] private EditText _title;
        [InjectView(Resource.Id.description)] private EditText _description;
        [InjectView(Resource.Id.btn_post)] private Button _postButton;
        [InjectView(Resource.Id.local_tags_list)] private RecyclerView _localTagsList;
        [InjectView(Resource.Id.tags_list)] private RecyclerView _tagsList;
        [InjectView(Resource.Id.page_title)] private TextView _pageTitle;
        [InjectView(Resource.Id.photo)] private ImageView _photoFrame;
        [InjectView(Resource.Id.tag)] private NewTextEdit _tag;
        [InjectView(Resource.Id.root_layout)] private RelativeLayout _rootLayout;
        [InjectView(Resource.Id.tags_layout)] private LinearLayout _tagsLayout;
        [InjectView(Resource.Id.tags_list_layout)] private LinearLayout _tagsListLayout;
        [InjectView(Resource.Id.top_margin_tags_layout)] private LinearLayout _topMarginTagsLayout;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _loadingSpinner;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_post_description);
            Cheeseknife.Inject(this);

            _pageTitle.Typeface = Style.Semibold;
            _title.Typeface = Style.Regular;
            _description.Typeface = Style.Regular;
            _postButton.Typeface = Style.Semibold;

            _postButton.Text = Localization.Texts.PublishButtonText;
            _shouldCompress = Intent.GetBooleanExtra(IsNeedCompressExtraPath, true);
            _path = Intent.GetStringExtra(PhotoExtraPath);
            var photoUri = Android.Net.Uri.Parse(_path);

            _postButton.Enabled = true;
            if (_shouldCompress)
            {
                FileDescriptor fileDescriptor = null;
                try
                {
                    fileDescriptor = ContentResolver.OpenFileDescriptor(photoUri, "r").FileDescriptor;
                    _btmp = BitmapUtils.DecodeSampledBitmapFromDescriptor(fileDescriptor, 1600, 1600);
                    _btmp = BitmapUtils.RotateImageIfRequired(_btmp, fileDescriptor, _path);
                    _photoFrame.SetImageBitmap(_btmp);
                }
                catch (Exception ex)
                {
                    _postButton.Enabled = false;
                    this.ShowAlert(Localization.Errors.UnknownCriticalError);
                    AppSettings.Reporter.SendCrash(ex);
                }
                finally
                {
                    fileDescriptor?.Dispose();
                }
            }
            else
            {
                _photoFrame.SetImageURI(photoUri);
            }

            _localTagsList.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
            _localTagsAdapter = new SelectedTagsAdapter();
            _localTagsAdapter.Click += LocalTagsAdapterClick;
            _localTagsList.SetAdapter(_localTagsAdapter);
            _localTagsList.AddItemDecoration(new ListItemDecoration((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 15, Resources.DisplayMetrics)));

            _tagsList.SetLayoutManager(new LinearLayoutManager(this));
            Presenter.SourceChanged += PresenterSourceChanged;
            _tagsAdapter = new TagsAdapter(Presenter);
            _tagsAdapter.Click += OnTagsAdapterClick;
            _tagsList.SetAdapter(_tagsAdapter);

            _tag.TextChanged += OnTagOnTextChanged;
            _tag.KeyboardDownEvent += HideTagsList;
            _tag.OkKeyEvent += OnTagOnOkKeyEvent;
            _tag.FocusChange += OnTagOnFocusChange;

            _timer = new Timer(OnTimer);

            SearchTextChanged();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
            if (_btmp != null)
            {
                _btmp.Recycle();
                _btmp = null;
            }
            GC.Collect(0);
        }

        private void PresenterSourceChanged()
        {
            if (IsFinishing || IsDestroyed)
                return;

            RunOnUiThread(() =>
            {
                _tagsAdapter.NotifyDataSetChanged();
            });
        }


        private void LocalTagsAdapterClick(int i)
        {
            if (!_localTagsAdapter.Enabled)
                return;
            _localTagsAdapter.LocalTags.RemoveAt(i);
            _localTagsAdapter.NotifyDataSetChanged();
        }

        private void OnTagOnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                Window.SetSoftInputMode(SoftInput.AdjustResize);
                AnimateTagsLayout(Resource.Id.toolbar);
            }
        }

        private void OnTagOnOkKeyEvent()
        {
            HideTagsList();
            var imm = GetSystemService(InputMethodService) as InputMethodManager;
            imm?.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);
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

        private void OnTagsAdapterClick(int position)
        {
            var result = Presenter[position];
            if (result == null)
                return;

            AddTag(result.Name);
            _tag.Text = string.Empty;
        }

        [InjectOnClick(Resource.Id.btn_post)]
        public async void OnPost(object sender, EventArgs e)
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

        [InjectOnClick(Resource.Id.btn_back)]
        public void OnBack(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        [InjectOnClick(Resource.Id.top_margin_tags_layout)]
        public void OnTagsLayoutClick(object sender, EventArgs e)
        {
            if (!_tag.Enabled)
                return;
            _tag.RequestFocus();
            var imm = GetSystemService(InputMethodService) as InputMethodManager;
            imm?.ShowSoftInput(_tag, ShowFlags.Implicit);
        }

        private void AnimateTagsLayout(int subject)
        {
            TransitionManager.BeginDelayedTransition(_rootLayout);
            _tagsListLayout.Visibility = Resource.Id.toolbar == subject ? ViewStates.Visible : ViewStates.Gone;

            var topPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, Resource.Id.toolbar == subject ? 5 : 45, Resources.DisplayMetrics);
            _topMarginTagsLayout.SetPadding(0, topPadding, 0, 0);

            var currentButtonLayoutParameters = _tagsLayout.LayoutParameters as RelativeLayout.LayoutParams;
            if (currentButtonLayoutParameters != null)
            {
                currentButtonLayoutParameters.AddRule(LayoutRules.Below, subject);
                _tagsLayout.LayoutParameters = currentButtonLayoutParameters;
            }
        }

        private void AddTag(string tag)
        {
            tag = tag.Trim();
            if (_localTagsAdapter.LocalTags.Count >= 4 || _localTagsAdapter.LocalTags.Any(t => t == tag))
                return;
            _localTagsAdapter.LocalTags.Add(tag);
            RunOnUiThread(() =>
            {
                _localTagsAdapter.NotifyDataSetChanged();
                _localTagsList.MoveToPosition(_localTagsAdapter.LocalTags.Count - 1);
            });
        }

        private void OnTimer(object state)
        {
            RunOnUiThread(async () =>
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

            List<string> errors = null;
            if (_tag.Text.Length == 0)
                errors = await Presenter.TryGetTopTags();
            else if (_tag.Text.Length > 1)
                errors = await Presenter.TryLoadNext(_tag.Text);

            if (IsFinishing || IsDestroyed)
                return;

            this.ShowAlert(errors);
        }

        private async Task OnPostAsync()
        {
            var isConnected = BasePresenter.ConnectionService.IsConnectionAvailable();

            if (!isConnected)
            {
                this.ShowAlert(Localization.Errors.InternetUnavailable);
                OnUploadEnded();
                return;
            }

            if (string.IsNullOrEmpty(_title.Text))
            {
                this.ShowAlert(Localization.Errors.EmptyTitleField, ToastLength.Long);
                OnUploadEnded();
                return;
            }

            var photo = await CompressPhoto(_path);
            if (IsFinishing || IsDestroyed)
                return;

            if (photo == null)
            {
                SplashActivity.Cache.EvictAll();
                photo = await CompressPhoto(_path);
                if (IsFinishing || IsDestroyed)
                    return;
            }

            if (photo == null)
            {
                this.ShowAlert(Localization.Errors.PhotoProcessingError);
                OnUploadEnded();
                return;
            }

            _request = new UploadImageRequest(BasePresenter.User.UserInfo, _title.Text, photo, _localTagsAdapter.LocalTags.ToArray())
            {
                Description = _description.Text
            };
            var serverResp = await Presenter.TryUploadWithPrepare(_request);
            if (IsFinishing || IsDestroyed)
                return;

            if (serverResp != null && serverResp.Success)
            {
                _response = serverResp.Result;
            }
            else
            {
                this.ShowAlert(serverResp);
                OnUploadEnded();
                return;
            }

            TryUpload();
        }

        private Task<byte[]> CompressPhoto(string path)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (_shouldCompress)
                    {
                        using (var stream = new MemoryStream())
                        {
                            if (_btmp.Compress(Bitmap.CompressFormat.Jpeg, 90, stream))
                            {
                                return stream.ToArray();
                            }
                        }
                    }
                    else
                    {
                        var photo = new Java.IO.File(path);
                        var stream = new Java.IO.FileInputStream(photo);
                        var outbytes = new byte[photo.Length()];
                        stream.Read(outbytes);
                        stream.Close();
                        return outbytes;
                    }
                }
                catch (Exception ex)
                {
                    AppSettings.Reporter.SendCrash(ex);
                }
                return null;
            });
        }


        private async void TryUpload()
        {
            if (_request == null || _response == null)
            {
                OnUploadEnded();
                return;
            }

            var resp = await Presenter.TryUpload(_request, _response);
            if (IsFinishing || IsDestroyed)
                return;

            if (resp != null && resp.Success)
            {
                OnUploadEnded();
                BasePresenter.ShouldUpdateProfile = true;
                Finish();
            }
            else
            {
                var msg = Localization.Errors.Unknownerror;
                if (resp != null && resp.Errors.Any())
                    msg = resp.Errors[0];
                this.ShowInteractiveMessage(msg, TryAgainAction, ForgetAction);
            }
        }

        private void OnUploadEnded()
        {
            _postButton.Enabled = true;
            _postButton.Text = Localization.Texts.PublishButtonText;

            _loadingSpinner.Visibility = ViewStates.Gone;

            _title.Enabled = true;
            _description.Enabled = true;
            _tag.Enabled = true;
            _localTagsAdapter.Enabled = true;
        }

        private void ForgetAction(object o, DialogClickEventArgs dialogClickEventArgs)
        {
            _request = null;
            _response = null;
            OnUploadEnded();
        }

        private void TryAgainAction(object o, DialogClickEventArgs dialogClickEventArgs)
        {
            TryUpload();
        }

        private void HideTagsList()
        {
            Window.SetSoftInputMode(SoftInput.AdjustPan);
            _tag.ClearFocus();
            AnimateTagsLayout(Resource.Id.description_layout);
        }
    }
}
