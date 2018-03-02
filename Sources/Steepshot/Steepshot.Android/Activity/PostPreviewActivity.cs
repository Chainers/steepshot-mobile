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
        public const string IsNeedCompressExtraPath = "SHOULD_COMPRESS";
        
        private string path;

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
                i.PutExtra(PostDescriptionActivity.IsNeedCompressExtraPath, false);

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
            path = PathHelper.GetFilePath(this, Android.Net.Uri.Parse(Intent.GetStringExtra(PhotoExtraPath)));

            var photoUri = Android.Net.Uri.Parse(path);
            _photoFrame.SetImageURI(photoUri);
        }

        public void OnPrepareLoad(Drawable p0)
        {
        }
    }
}
