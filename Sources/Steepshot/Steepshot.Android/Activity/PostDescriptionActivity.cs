using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Transitions;
using Android.Util;
using Android.Views;
using Android.Widget;
using Autofac;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Core.Services;
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
        private TagsAdapter _localTagsAdapter;
        private TagsAdapter _tagsAdapter;

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
#pragma warning restore 0649


        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_post_description);
            Cheeseknife.Inject(this);
            
            _pageTitle.Typeface = Style.Semibold;
            _title.Typeface = Style.Regular;
            _description.Typeface = Style.Regular;
            _postButton.Typeface = Style.Semibold;

            _path = Intent.GetStringExtra(PhotoExtraPath);
            _shouldCompress = Intent.GetBooleanExtra(IsNeedCompressExtraPath, true);
            var photoUri = Android.Net.Uri.Parse(_path);

            if (!_shouldCompress)
                _photoFrame.SetImageURI(photoUri);
            else
            {
                Task.Run(() =>
                {
                    var fileDescriptor = ContentResolver.OpenFileDescriptor(photoUri, "r").FileDescriptor;
                    _btmp = BitmapUtils.DecodeSampledBitmapFromDescriptor(fileDescriptor, 1600, 1600);
                    _btmp = BitmapUtils.RotateImageIfRequired(_btmp, fileDescriptor, _path);
                    _photoFrame.SetImageBitmap(_btmp);
                });
            }

            _localTagsList.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
            _localTagsAdapter = new TagsAdapter();
            _localTagsList.SetAdapter(_localTagsAdapter);
            _localTagsList.AddItemDecoration(new ListItemDecoration((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 15, Resources.DisplayMetrics)));

            _tagsList.SetLayoutManager(new LinearLayoutManager(this));
            _tagsAdapter = new TagsAdapter(_presenter);
            _tagsList.SetAdapter(_tagsAdapter);

            _tagsAdapter.Click += (int obj) =>
            {
                AddTag(_presenter[obj].Name);
                _tag.Text = string.Empty;
            };

            _tag.TextChanged += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Text.ToString()))
                {
                    if (e.Text.Last() == ' ')
                    {
                        _tag.Text = string.Empty;
                        AddTag(e.Text.ToString());
                    }
                }
                _timer.Change(500, Timeout.Infinite);
            };

            _tag.KeyboardDownEvent += () =>
            {
                Window.SetSoftInputMode(SoftInput.AdjustPan);
                _tag.ClearFocus();
                AnimateTagsLayout(Resource.Id.description_layout);
            };

            _tag.FocusChange += (sender, e) =>
            {
                if (e.HasFocus)
                {
                    Window.SetSoftInputMode(SoftInput.AdjustResize);
                    AnimateTagsLayout(Resource.Id.toolbar);
                }
            };

            _timer = new Timer(OnTimer);
            await SearchTextChanged();
            _postButton.Enabled = true;
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
            GC.Collect();
        }

        protected override void CreatePresenter()
        {
            _presenter = new PostDescriptionPresenter();
        }


        [InjectOnClick(Resource.Id.btn_post)]
        public void OnPost(object sender, EventArgs e)
        {
            _postButton.Enabled = false;
            OnPostAsync();
        }

        [InjectOnClick(Resource.Id.btn_back)]
        public void OnBack(object sender, EventArgs e)
        {
            OnBackPressed();
        }


        private void AnimateTagsLayout(int subject)
        {
            TransitionManager.BeginDelayedTransition(_rootLayout);
            _tagsListLayout.Visibility = Resource.Id.toolbar == subject ? ViewStates.Visible : ViewStates.Gone;

            var layoutParameters = (LinearLayout.LayoutParams)_topMarginTagsLayout.LayoutParameters;
            layoutParameters.TopMargin = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, Resource.Id.toolbar == subject ? 5 : 45, Resources.DisplayMetrics);
            _topMarginTagsLayout.LayoutParameters = layoutParameters;

            RelativeLayout.LayoutParams currentButtonLayoutParameters = (RelativeLayout.LayoutParams)_tagsLayout.LayoutParameters;
            currentButtonLayoutParameters.AddRule(LayoutRules.Below, subject);
            _tagsLayout.LayoutParameters = currentButtonLayoutParameters;
        }

        private void AddTag(string tag)
        {
            tag = tag.TrimStart().TrimEnd();
            if (_localTagsAdapter.LocalTags.Count() >= 4 || _localTagsAdapter.LocalTags.Any(t => t.Name == tag))
                return;
            _localTagsAdapter.LocalTags.Add(new SearchResult() { Name = tag });
            RunOnUiThread(() =>
            {
                _localTagsAdapter.NotifyDataSetChanged();
                _localTagsList.SmoothScrollToPosition(_localTagsAdapter.LocalTags.Count() - 1);
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
            List<string> errors = null;
            _tagsList.ScrollToPosition(0);
            _presenter.Clear();
            if (_tag.Text.Length == 0)
                errors = await _presenter.TryGetTopTags();
            else if (_tag.Text.Length > 1)
                errors = await _presenter.TryLoadNext(_tag.Text);
            if (errors != null && errors.Count > 0)
                ShowAlert(errors);
            else
                _tagsAdapter?.NotifyDataSetChanged();
        }
        
        private async Task OnPostAsync()
        {
            try
            {
                if (!AppSettings.Container.Resolve<IConnectionService>().IsConnectionAvailable())
                    return;

                if (string.IsNullOrEmpty(_title.Text))
                {
                    ShowAlert(Localization.Errors.EmptyTitleField, ToastLength.Long);
                    return;
                }
                var arrayToUpload = await CompressPhoto(_path);
                if (arrayToUpload != null)
                {

                    var request = new Core.Models.Requests.UploadImageRequest(BasePresenter.User.UserInfo, _title.Text, arrayToUpload, _localTagsAdapter.GetLocalTags().ToArray())
                    {
                        Description = _description.Text
                    };
                    var resp = await _presenter.TryUpload(request);
                    if (resp == null)
                        return;

                    if (resp.Success)
                    {
                        BasePresenter.ShouldUpdateProfile = true;
                        Finish();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(resp.Errors[0]))
                            ShowAlert(resp, ToastLength.Long);
                    }
                }
                else
                {
                    ShowAlert(Localization.Errors.PhotoCompressingError, ToastLength.Long);
                }
            }
            catch (ApplicationExceptionBase ex)
            {
                ShowAlert(ex.Message);
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            finally
            {
                if (_postButton != null)
                    _postButton.Enabled = true;
            }
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
                                  var outbytes = stream.ToArray();
                                  _btmp.Recycle();
                                  return outbytes;
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
    }
}
