using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Java.IO;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using System;
using System.Threading.Tasks;

namespace Steepshot.Activity
{
    [Activity(Label = "PostPreviewActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class PostPreviewActivity : BaseActivity, ITarget
    {
        public const string PhotoExtraPath = "PhotoExtraPath";
        public const string IsNeedParseExtraPath = "SHOULD_COMPRESS";
        
        private string path;
        private bool shouldParse;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.photo)] private ImageView _photoFrame;
        [InjectView(Resource.Id.btn_post_back)] private ImageButton _backButton;
        [InjectView(Resource.Id.btn_accept_post)] private ImageButton _acceptButton;
        [InjectView(Resource.Id.rotate_preview)] private ImageView _rotate;
#pragma warning restore 0649


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_post_preview);

            Cheeseknife.Inject(this);

            _photoFrame.Clickable = true;
            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += GoBackClick;
            _acceptButton.Click += AcceptClick;
            _rotate.Click += RotateOnClick;

            InitPhoto();
        }

        private void RotateOnClick(object sender, EventArgs e)
        {
            if (!_photoFrame.Clickable)
                return;

            _photoFrame.Clickable = false;
            var btmp = BitmapFactory.DecodeFile(path);

            btmp = BitmapUtils.RotateImage(btmp, 90);
            using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Create))
            {
                btmp.Compress(Bitmap.CompressFormat.Png, 100, stream);
            }

            btmp.Recycle();
            btmp.Dispose();

            var photoUri = Android.Net.Uri.Parse(path);
            _photoFrame.SetImageURI(null);
            _photoFrame.SetImageURI(photoUri);
            _photoFrame.Clickable = true;
        }

        private void AcceptClick(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var i = new Intent(this, typeof(PostDescriptionActivity));
                i.PutExtra(PostDescriptionActivity.PhotoExtraPath, path);
                i.PutExtra(PostDescriptionActivity.IsNeedCompressExtraPath, true);

                RunOnUiThread(() =>
                {
                    StartActivity(i);
                    Finish();
                });
            });
        }

        private void GoBackClick(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        public void OnBitmapFailed(Drawable p0)
        {
        }

        public void OnBitmapLoaded(Bitmap p0, Picasso.LoadedFrom p1)
        {
            //_photo.SetImageBitmap(p0);
        }

        private void InitPhoto()
        {
            shouldParse = Intent.GetBooleanExtra(IsNeedParseExtraPath, false);
            path = shouldParse ? PathHelper.GetFilePath(this, Android.Net.Uri.Parse(Intent.GetStringExtra(PhotoExtraPath))) : Intent.GetStringExtra(PhotoExtraPath);

            if (shouldParse)
                path = $"file://{path}"; // for photos taken from the camera

            path = Compress(path);

            var photoUri = Android.Net.Uri.Parse(path);
            _photoFrame.SetImageURI(photoUri);
        }
        
        private string Compress(string path)
        {
            var photoUri = Android.Net.Uri.Parse(path);

            FileDescriptor fileDescriptor = null;
            Bitmap btmp = null;
            System.IO.FileStream stream = null;

            try
            {
                fileDescriptor = ContentResolver.OpenFileDescriptor(photoUri, "r").FileDescriptor;
                //btmp = BitmapUtils.DecodeSampledBitmapFromUri(PathHelper.GetFilePath(this, photoUri), 1200, 1200);

                btmp = Android.Provider.MediaStore.Images.Media.GetBitmap(this.ContentResolver, photoUri);

                var bitmapScalled = Bitmap.CreateScaledBitmap(btmp, 1200, 1200, true);
                btmp = bitmapScalled;

                btmp = BitmapUtils.RotateImageIfRequired(btmp, fileDescriptor, path);
                var quality = BitmapUtils.GetCompressionQuality(btmp, 1024 * 1024);
                
                var directoryPictures = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
                var directory = new Java.IO.File(directoryPictures, Constants.Steepshot);
                if (!directory.Exists())
                    directory.Mkdirs();

                path = $"{directory}/{Guid.NewGuid()}.jpeg";
                
                using (var fs = new System.IO.FileStream(path, System.IO.FileMode.CreateNew))
                {
                    btmp.Compress(Bitmap.CompressFormat.Jpeg, quality, fs);
                }

                return path;
            }
            catch (Exception ex)
            {
                //_postButton.Enabled = false;
                this.ShowAlert(LocalizationKeys.UnknownCriticalError);
                AppSettings.Reporter.SendCrash(ex);
            }
            finally
            {
                fileDescriptor?.Dispose();
                btmp?.Recycle();
                btmp?.Dispose();
                stream?.Dispose();
            }

            return path;
        }

        public void OnPrepareLoad(Drawable p0) { }
    }
}
