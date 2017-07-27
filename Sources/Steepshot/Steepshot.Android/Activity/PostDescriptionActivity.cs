using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Core.Utils;
using Steepshot.Presenter;

using Steepshot.Utils;
using Steepshot.View;

namespace Steepshot.Activity
{
    [Activity(Label = "PostDescriptionActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.StateHidden | SoftInput.AdjustPan)]
    public class PostDescriptionActivity : BaseActivity, IPostDescriptionView//, ITarget
    {
        private PostDescriptionPresenter _presenter;
        public static int TagRequestCode = 1225;
        private string _path;

        private List<string> _tags = new List<string>();
        private byte[] _arrayToUpload;
        private Bitmap _bitmapToUpload;

        //private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        //private CancellationToken ct;
        private FrameLayout _add;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.d_edit)] EditText _description;
        [InjectView(Resource.Id.load_layout)] RelativeLayout _loadLayout;
        [InjectView(Resource.Id.btn_post)] Button _postButton;
        [InjectView(Resource.Id.description_scroll)] ScrollView _descriptionScroll;
        [InjectView(Resource.Id.tag_container)] TagLayout _tagLayout;
        [InjectView(Resource.Id.photo)] ImageView _photoFrame;
#pragma warning restore 0649

        [InjectOnClick(Resource.Id.btn_post)]
        public void OnPost(object sender, EventArgs e)
        {
            _loadLayout.Visibility = ViewStates.Visible;
            OnPostAsync();
        }

        [InjectOnClick(Resource.Id.btn_back)]
        public void OnBack(object sender, EventArgs e)
        {
            OnBackPressed();
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

            Picasso.With(this).Load(new Java.IO.File(_path))
                   .MemoryPolicy(MemoryPolicy.NoCache, MemoryPolicy.NoStore)
                   .Resize(Resources.DisplayMetrics.WidthPixels, 0)
                   .Into(_photoFrame);
        }


        public void AddTags(List<string> tags)
        {
            _tags = tags;
            _tagLayout.RemoveAllViews();

            _add = (FrameLayout)LayoutInflater.Inflate(Resource.Layout.lyt_add_tag, null, false);
            _add.Click += (sender, e) => OpenTags();
            _tagLayout.AddView(_add);
            _tagLayout.RequestLayout();

            foreach (var item in tags)
            {
                FrameLayout tag = (FrameLayout)LayoutInflater.Inflate(Resource.Layout.lyt_tag, null, false);
                tag.FindViewById<TextView>(Resource.Id.text).Text = item;
                tag.Click += (sender, e) => _tagLayout.RemoveView(tag);
                _tagLayout.AddView(tag);
                _tagLayout.RequestLayout();
            }
            _descriptionScroll.RequestLayout();
        }

        public void OpenTags()
        {
            Intent intent = new Intent(this, typeof(TagsActivity));
            StartActivityForResult(intent, TagRequestCode);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == TagRequestCode && resultCode == Result.Ok)
            {
                var b = data.GetBundleExtra("TAGS");
                _tags = b.GetStringArray("TAGS").Distinct().ToList();
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
            //_tokenSource.Cancel();
            //_tokenSource.Dispose();
            if (_bitmapToUpload != null)
            {
                _bitmapToUpload.Recycle();
                _bitmapToUpload.Dispose();
                _bitmapToUpload = null;
            }
            Cheeseknife.Reset(this);
            GC.Collect();
        }
        /*
		public void OnBitmapFailed(Drawable p0)
		{

		}

		public void OnBitmapLoaded(Bitmap p0, Picasso.LoadedFrom p1)
		{
			RunOnUiThread(() =>
			{
				photoFrame?.SetImageBitmap(p0);
				postButton.Enabled = true;
			});
			photoComressionTask = Task.Factory.StartNew(CompressPhoto, ct);
		}*/

        private async void OnPostAsync()
        {
            try
            {
                //photoComressionTask.Wait();
                var success = await CompressPhoto();
                if (!success)
                    ShowAlert("Photo upload error, please try again");

                var request = new Core.Models.Requests.UploadImageRequest(BasePresenter.User.CurrentUser, _description.Text, _arrayToUpload, _tags.ToArray());
                var resp = await _presenter.Upload(request);
                if (resp.Errors.Count > 0)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, resp.Errors[0], ToastLength.Long).Show();
                    });
                }
                else
                {
                    _bitmapToUpload.Recycle();
                    _bitmapToUpload.Dispose();
                    _bitmapToUpload = null;
                    BasePresenter.ShouldUpdateProfile = true;
                    Finish();
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, BasePresenter.User.Login, BasePresenter.AppVersion);
            }
            finally
            {
                if (_loadLayout != null)
                    _loadLayout.Visibility = ViewStates.Gone;
            }
        }

        private async Task<bool> CompressPhoto()
        {
            try
            {
                await Task.Run(() =>
              {
                  var absolutePath = (new Java.IO.File(_path)).AbsolutePath;
                  _bitmapToUpload = BitmapFactory.DecodeFile(absolutePath);
                  _bitmapToUpload = RotateImageIfRequired(_bitmapToUpload, absolutePath);
              });
                if (_bitmapToUpload == null)
                    return false;

                using (var stream = new MemoryStream())
                {
                    await _bitmapToUpload.CompressAsync(Bitmap.CompressFormat.Jpeg, 80, stream);
                    _arrayToUpload = stream.ToArray();
                    //var photoSize = streamArray.Length / 1024f / 1024f;
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, BasePresenter.User.Login, BasePresenter.AppVersion);
                return false;
            }
        }

        private static Bitmap RotateImageIfRequired(Bitmap img, string selectedImage)
        {
            ExifInterface ei = new ExifInterface(selectedImage);
            int orientation = ei.GetAttributeInt(ExifInterface.TagOrientation, (int)Android.Media.Orientation.Normal);

            switch ((Android.Media.Orientation)orientation)
            {
                case Android.Media.Orientation.Rotate90:
                    return RotateImage(img, 90);
                case Android.Media.Orientation.Rotate180:
                    return RotateImage(img, 180);
                case Android.Media.Orientation.Rotate270:
                    return RotateImage(img, 270);
                default:
                    return img;
            }
        }

        private static Bitmap RotateImage(Bitmap img, int degree)
        {
            Matrix matrix = new Matrix();
            matrix.PostRotate(degree);
            Bitmap rotatedImg = Bitmap.CreateBitmap(img, 0, 0, img.Width, img.Height, matrix, true);
            return rotatedImg;
        }

        protected override void CreatePresenter()
        {
            _presenter = new PostDescriptionPresenter(this);
        }

        public void OnPrepareLoad(Drawable p0)
        {

        }
    }
}
