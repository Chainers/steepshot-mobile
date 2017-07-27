using System;
using Android.App;
using Android.OS;
using Com.Lilarcor.Cheeseknife;
using Square.Picasso;
using Steepshot.Presenter;

namespace Steepshot
{
    [Activity(Label = "PostPreviewActivity", ScreenOrientation =Android.Content.PM.ScreenOrientation.Portrait)]
    public class PostPreviewActivity : BaseActivity, PostPreviewView
    {
        private PostPreviewPresenter presenter;
		private string path;

        protected override void CreatePresenter()
        {
            presenter = new PostPreviewPresenter(this);
        }

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.photo)] ScaleImageView photo;
#pragma warning restore 0649
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_post_preview);
            Cheeseknife.Inject(this);

            path = Intent.GetStringExtra("PhotoURL");
			try
			{
				Picasso.With(this).Load(path).NoFade().Resize(Resources.DisplayMetrics.WidthPixels, 0).Into(photo);
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex, BasePresenter.User.Login, BasePresenter.AppVersion);
			}
        }

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Cheeseknife.Reset(this);
		}
    }
}