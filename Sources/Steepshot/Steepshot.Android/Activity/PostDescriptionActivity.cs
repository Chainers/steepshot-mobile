using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using Autofac;
using Com.Lilarcor.Cheeseknife;
using Java.IO;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(Label = "PostDescriptionActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.StateHidden | SoftInput.AdjustPan)]
    public class PostDescriptionActivity : BaseActivityWithPresenter<PostDescriptionPresenter>
    {
        public static int TagRequestCode = 1225;
        private string _path;

        private string[] _tags = new string[0];
        private FrameLayout _add;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.d_edit)] EditText _description;
        [InjectView(Resource.Id.load_layout)] RelativeLayout _loadLayout;
        [InjectView(Resource.Id.btn_post)] Button _postButton;
        [InjectView(Resource.Id.description_scroll)] ScrollView _descriptionScroll;
        [InjectView(Resource.Id.tag_container)] TagLayout _tagLayout;
        [InjectView(Resource.Id.photo)] ImageView _photoFrame;

        [InjectView(Resource.Id.description_title)] private TextView _descriptionTitle;
        [InjectView(Resource.Id.description_edit)] private EditText _editTextDescription;
#pragma warning restore 0649

        [InjectOnClick(Resource.Id.btn_post)]
        public void OnPost(object sender, EventArgs e)
        {
            _postButton.Enabled = false;
            _loadLayout.Visibility = ViewStates.Visible;
            OnPostAsync();
        }

        [InjectOnClick(Resource.Id.btn_back)]
        public void OnBack(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        [InjectOnClick(Resource.Id.toggle_description)]
        public void ToggleDescription(object sender, EventArgs e)
        {
            _descriptionTitle.Visibility = _descriptionTitle.Visibility == ViewStates.Gone ? ViewStates.Visible : ViewStates.Gone;
            _editTextDescription.Visibility = _editTextDescription.Visibility == ViewStates.Gone ? ViewStates.Visible : ViewStates.Gone;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_post_description);
            Cheeseknife.Inject(this);

            _photoFrame.SetBackgroundColor(Color.Black);
            var parameters = _photoFrame.LayoutParameters;
            parameters.Height = Resources.DisplayMetrics.WidthPixels;
            _photoFrame.LayoutParameters = parameters;
            _postButton.Enabled = true;
            _path = Intent.GetStringExtra("FILEPATH");

            Cache.Clear();
            GC.Collect();

            Picasso.With(this).Load(_path.ToFilePath())
                   .MemoryPolicy(MemoryPolicy.NoCache, MemoryPolicy.NoStore)
                   .Resize(Resources.DisplayMetrics.WidthPixels, 0)
                   .Into(_photoFrame);
        }


        public void AddTags(string[] tags)
        {
            _tags = tags;
            _tagLayout.RemoveAllViews();

            _add = (FrameLayout)LayoutInflater.Inflate(Resource.Layout.lyt_add_tag, null, false);
            _add.Click += (sender, e) => OpenTags();
            _tagLayout.AddView(_add);
            _tagLayout.RequestLayout();

            foreach (var item in tags)
            {
                var tag = (FrameLayout)LayoutInflater.Inflate(Resource.Layout.lyt_tag, null, false);
                tag.FindViewById<TextView>(Resource.Id.text).Text = item;
                tag.Click += (sender, e) => _tagLayout.RemoveView(tag);
                _tagLayout.AddView(tag);
                _tagLayout.RequestLayout();
            }
            _descriptionScroll.RequestLayout();
        }

        public void OpenTags()
        {
            var intent = new Intent(this, typeof(TagsActivity));
            var b = new Bundle();
            b.PutStringArray("TAGS", _tags);
            intent.PutExtra("TAGS", b);
            StartActivityForResult(intent, TagRequestCode);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == TagRequestCode && resultCode == Result.Ok)
            {
                var b = data.GetBundleExtra("TAGS");
                _tags = b.GetStringArray("TAGS").Distinct().ToArray();
                AddTags(_tags);
            }
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            AddTags(_tags);
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
            GC.Collect();
        }

        private async void OnPostAsync()
        {
            try
            {
                if (!AppSettings.Container.Resolve<IConnectionService>().IsConnectionAvailable())
                    return;

                if (string.IsNullOrEmpty(_description.Text))
                {
                    Toast.MakeText(this, Localization.Errors.EmptyDescription, ToastLength.Long).Show();
                    return;
                }
                var arrayToUpload = await CompressPhoto(_path);
                if (arrayToUpload != null)
                {
                    var request = new Core.Models.Requests.UploadImageRequest(BasePresenter.User.UserInfo, _description.Text, arrayToUpload, _tags.ToArray());
                    if(!string.IsNullOrEmpty(_editTextDescription.Text))
                        request.Description = _editTextDescription.Text; 
                    var resp = await _presenter.Upload(request);

                    if (resp.Errors.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(resp.Errors[0]))
                            Toast.MakeText(this, resp.Errors[0], ToastLength.Long).Show();
                    }
                    else
                    {
                        BasePresenter.ShouldUpdateProfile = true;
                        Finish();
                    }
                }
                else
                {
                    Toast.MakeText(this, Localization.Errors.PhotoCompressingError, ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            finally
            {
                if (_loadLayout != null)
                {
                    _loadLayout.Visibility = ViewStates.Gone;
                    _postButton.Enabled = true;
                }
            }
        }

        private Task<byte[]> CompressPhoto(string path)
        {
            return Task.Run(() =>
              {
                  try
                  {
                      var bitmap = BitmapUtils.DecodeSampledBitmapFromResource(path, 1200, 1200);
                      bitmap = BitmapUtils.RotateImageIfRequired(bitmap, path);

                      if (bitmap == null)
                          return null;

                      using (var stream = new MemoryStream())
                      {
                          if (bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, stream))
                          {
                              var outbytes = stream.ToArray();
                              bitmap.Recycle();
                              return outbytes;
                          }
                      }
                  }
                  catch (Exception ex)
                  {
                      AppSettings.Reporter.SendCrash(ex);
                  }
                  return null;
              });
        }
        
        protected override void CreatePresenter()
        {
            _presenter = new PostDescriptionPresenter();
        }
    }
}
