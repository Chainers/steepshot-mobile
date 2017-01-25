using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;

using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Square.Picasso;
using Steemix.Droid.Activities;

namespace Steemix.Droid
{
	[Activity(Label = "PostDescriptionActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.StateHidden | SoftInput.AdjustPan)]
	public class PostDescriptionActivity : BaseActivity<ViewModels.PostDescriptionViewModel>, ITarget
	{

		public static int TagRequestCode = 1225;

		[InjectOnClick(Resource.Id.btn_back)]
		public void OnBack(object sender, EventArgs e)
		{
			OnBackPressed();
		}

		[InjectView(Resource.Id.tag_container)]
		TagLayout tagLayout;

		[InjectView(Resource.Id.photo)]
		ScaleImageView PhotoView;
		string Path;

		List<string> tags = new List<string>();

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.lyt_post_description);
			Cheeseknife.Inject(this);

			PhotoView.SetBackgroundColor(Color.Black);

			Path = Intent.GetStringExtra("FILEPATH");
		}

		public void AddTags(List<string> tags)
		{
			FrameLayout _add = (FrameLayout)LayoutInflater.Inflate(Resource.Layout.lyt_add_tag, null, false);
			_add.Click += (sender, e) => OpenTags();
			tagLayout.AddView(_add);
			tagLayout.RequestLayout();

			foreach (var item in tags)
			{
				FrameLayout tag = (FrameLayout)LayoutInflater.Inflate(Resource.Layout.lyt_tag, null, false);
				tag.FindViewById<TextView>(Resource.Id.text).Text = string.Format("#tag{0}", item);
				tag.Click += (sender, e) => tagLayout.RemoveView(tag);
				tagLayout.AddView(tag);
				tagLayout.RequestLayout();
			}
		}

		public void OpenTags()
		{
			Intent intent = new Intent(this, typeof(TagsActivity));
			StartActivityForResult(intent, TagRequestCode);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (requestCode == TagRequestCode)
			{
				int i = 0;
			}
		}

		protected override void OnPostCreate(Bundle savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);
			Picasso.With(this).Load(new Java.IO.File(Path)).Into(this);
			AddTags(tags);
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
