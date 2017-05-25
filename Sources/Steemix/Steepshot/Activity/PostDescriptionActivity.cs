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
using System.Linq;
using Java.IO;
using System.IO;
using Android.Media;
using System.Threading.Tasks;
using System.Threading;

namespace Steepshot
{
	[Activity(Label = "PostDescriptionActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.StateHidden | SoftInput.AdjustPan)]
	public class PostDescriptionActivity : BaseActivity, ITarget, PostDescriptionView
	{
		PostDescriptionPresenter presenter;

		public static int TagRequestCode = 1225;

		[InjectOnClick(Resource.Id.btn_back)]
		public void OnBack(object sender, EventArgs e)
		{
			OnBackPressed();
		}

		[InjectView(Resource.Id.d_edit)]
		EditText description;

		[InjectView(Resource.Id.load_layout)]
		RelativeLayout loadLayout;

		[InjectView(Resource.Id.btn_post)]
		Button postButton;

		[InjectView(Resource.Id.description_scroll)]
		ScrollView descriptionScroll;

		[InjectOnClick(Resource.Id.btn_post)]
		public void OnPost(object sender, EventArgs e)
		{
			loadLayout.Visibility = ViewStates.Visible;
			Task.Run(async () =>
			   {
				   await OnPostAsync();
			   });
		}

		[InjectView(Resource.Id.tag_container)]
		TagLayout tagLayout;

		[InjectView(Resource.Id.photo)]
		ImageView PhotoView;

		private string Path;

		private List<string> tags = new List<string>();
		private Task<byte[]> photoComressionTask;
		private Bitmap bitmapToUpload;

		private CancellationTokenSource _tokenSource = new CancellationTokenSource();
		private CancellationToken ct;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			ct = _tokenSource.Token;

			SetContentView(Resource.Layout.lyt_post_description);
			Cheeseknife.Inject(this);

			PhotoView.SetBackgroundColor(Color.Black);

			var photoFrame = FindViewById<ImageView>(Resource.Id.photo);
			var parameters = photoFrame.LayoutParameters;
			parameters.Height = Resources.DisplayMetrics.WidthPixels;
			photoFrame.LayoutParameters = parameters;

			Path = Intent.GetStringExtra("FILEPATH");
		}

		FrameLayout _add;
		public void AddTags(List<string> tags)
		{
			this.tags = tags;
			tagLayout.RemoveAllViews();

			_add = (FrameLayout)LayoutInflater.Inflate(Resource.Layout.lyt_add_tag, null, false);
			_add.Click += (sender, e) => OpenTags();
			tagLayout.AddView(_add);
			tagLayout.RequestLayout();

			foreach (var item in tags)
			{
				FrameLayout tag = (FrameLayout)LayoutInflater.Inflate(Resource.Layout.lyt_tag, null, false);
				tag.FindViewById<TextView>(Resource.Id.text).Text = string.Format("#{0}", item);
				tag.Click += (sender, e) => tagLayout.RemoveView(tag);
				tagLayout.AddView(tag);
				tagLayout.RequestLayout();
			}
			descriptionScroll.RequestLayout();
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
				tags = b.GetStringArray("TAGS").Distinct().ToList();
				AddTags(tags);
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
			_tokenSource.Cancel();
			_tokenSource.Dispose();
			PhotoView?.Dispose();
			PhotoView = null;
			if (bitmapToUpload != null)
			{
				bitmapToUpload.Recycle();
				bitmapToUpload.Dispose();
				bitmapToUpload = null;
			}
			GC.Collect();
		}

		public void OnBitmapFailed(Drawable p0)
		{

		}

		public void OnBitmapLoaded(Bitmap p0, Picasso.LoadedFrom p1)
		{
			RunOnUiThread(() =>
			{
				PhotoView?.SetImageBitmap(p0);
				postButton.Enabled = true;
			});
			photoComressionTask = Task.Factory.StartNew(CompressPhoto, ct);
		}

		private async Task OnPostAsync()
		{
			try
			{
				photoComressionTask.Wait();
				var bitmapData = photoComressionTask.Result;
				var resp = await presenter.Upload(new Sweetshot.Library.Models.Requests.UploadImageRequest(
					UserPrincipal.Instance.CurrentUser.SessionId,
					description.Text,
					bitmapData,
					tags.ToArray()));
				if (resp.Errors.Count > 0)
				{
					RunOnUiThread(() =>
				   {
					   Toast.MakeText(this, resp.Errors[0], ToastLength.Long).Show();
				   });
				}
				else
				{
					bitmapToUpload.Recycle();
					bitmapToUpload.Dispose();
					bitmapToUpload = null;
					Finish();
				}
			}
			catch (Exception ex)
			{

			}
			finally
			{
				loadLayout.Visibility = ViewStates.Gone;
			}
		}

		private byte[] CompressPhoto()
		{
			try
			{
				if (ct.IsCancellationRequested)
				{
					ct.ThrowIfCancellationRequested();
					photoComressionTask.Dispose();
				}
				bitmapToUpload = ((BitmapDrawable)PhotoView.Drawable).Bitmap;
				using (var stream = new MemoryStream())
				{
					if (ct.IsCancellationRequested)
					{
						ct.ThrowIfCancellationRequested();
						photoComressionTask.Dispose();
					}
					bitmapToUpload.Compress(Bitmap.CompressFormat.Jpeg, 80, stream);
					var streamArray = stream.ToArray();
					//var photoSize = streamArray.Length / 1024f / 1024f;
					return streamArray;
				}
			}
			catch (Exception ex)
			{
				return new byte[0];
			}
		}

		/*private static Bitmap RotateImageIfRequired(Bitmap img, string selectedImage)
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
		}*/

		public void OnPrepareLoad(Drawable p0)
		{

		}

		protected override void CreatePresenter()
		{
			presenter = new PostDescriptionPresenter(this);
		}
	}
}
