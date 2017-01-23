
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Square.Picasso;
using Steemix.Droid.Activities;

namespace Steemix.Droid
{
	[Activity(Label = "PostDescriptionActivity",ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait,WindowSoftInputMode = SoftInput.StateHidden | SoftInput.AdjustPan)]
	public class PostDescriptionActivity : BaseActivity<ViewModels.PostDescriptionViewModel>, ITarget
	{
		[InjectOnClick(Resource.Id.btn_back)]
		public void OnBack(object sender, EventArgs e)
		{
			OnBackPressed();
		}

		[InjectView(Resource.Id.photo)]
		ScaleImageView PhotoView;
		string Path;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.lyt_post_description);
			Cheeseknife.Inject(this);

			PhotoView.SetBackgroundColor(Color.Black);

			Path = Intent.GetStringExtra("FILEPATH");

		}

		protected override void OnPostCreate(Bundle savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);
			Picasso.With(this).Load(new Java.IO.File(Path)).Into(this);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			PhotoView?.Dispose();
			PhotoView = null;
			GC.Collect();
		}

		public void OnBitmapFailed(Drawable p0)
		{
			
		}

		public void OnBitmapLoaded(Bitmap p0, Picasso.LoadedFrom p1)
		{
			int dstWidth = 0;
			int dstHeight = 0;
			float coeff = 0;

				coeff = (float)p0.Height / (float)p0.Width;
				dstWidth = Resources.DisplayMetrics.WidthPixels;
				dstHeight = (int)(dstWidth * coeff);

			var b = Bitmap.CreateScaledBitmap(p0, dstWidth, dstHeight, true);
			RunOnUiThread(() =>
			{
				PhotoView.SetImageBitmap(b);
				PhotoView.SetZoom(1);
				PhotoView.Invalidate();
			});
		}

		public void OnPrepareLoad(Drawable p0)
		{
			
		}
	}
}
