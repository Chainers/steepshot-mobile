using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using CheeseBind;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.CustomViews;

namespace Steepshot.Activity
{
    [Activity(Label = "PostPreviewActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class PostPreviewActivity : BaseActivity, ITarget
    {
        public const string PhotoExtraPath = "PhotoExtraPath";
        private string path;

#pragma warning disable 0649, 4014
        [CheeseBind.BindView(Resource.Id.photo)] private ScaleImageView _photo;
#pragma warning restore 0649


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_post_preview);
            Cheeseknife.Bind(this);

            path = Intent.GetStringExtra(PhotoExtraPath);
            if (!string.IsNullOrWhiteSpace(path))
                Picasso.With(this)
                       .Load(path)
                       .NoFade()
                       .Resize(Resources.DisplayMetrics.WidthPixels, 0)
                       .Into(_photo, OnSuccess, OnError);
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
            _photo.SetImageBitmap(p0);
        }

        public void OnPrepareLoad(Drawable p0)
        {
        }

        private void OnSuccess()
        {
        }

        private void OnError()
        {
            Picasso.With(this).Load(path).NoFade().Into(this);
        }
    }
}
