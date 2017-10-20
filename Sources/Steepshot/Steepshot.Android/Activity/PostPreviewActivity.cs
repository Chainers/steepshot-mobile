using Android.App;
using Android.OS;
using Com.Lilarcor.Cheeseknife;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(Label = "PostPreviewActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class PostPreviewActivity : BaseActivity
    {
        public const string PhotoExtraPath = "PhotoExtraPath";

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.photo)] private ScaleImageView _photo;
#pragma warning restore 0649


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_post_preview);
            Cheeseknife.Inject(this);

            var path = Intent.GetStringExtra(PhotoExtraPath);
            if (!string.IsNullOrWhiteSpace(path))
                Picasso.With(this).Load(path).NoFade().Resize(Resources.DisplayMetrics.WidthPixels, 0).Into(_photo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }
    }
}
