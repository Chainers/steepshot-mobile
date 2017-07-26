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
    public class PostDescriptionActivity : BaseActivity, PostDescriptionView//, ITarget
    {
        private PostDescriptionPresenter presenter;
        public static int TagRequestCode = 1225;
        private string Path;

        private List<string> tags = new List<string>();
        private byte[] arrayToUpload;
        private Bitmap bitmapToUpload;

        //private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        //private CancellationToken ct;
        private FrameLayout _add;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.d_edit)] EditText description;
        [InjectView(Resource.Id.load_layout)] RelativeLayout loadLayout;
        [InjectView(Resource.Id.btn_post)] Button postButton;
        [InjectView(Resource.Id.description_scroll)] ScrollView descriptionScroll;
        [InjectView(Resource.Id.tag_container)] TagLayout tagLayout;
        [InjectView(Resource.Id.photo)] ImageView photoFrame;
#pragma warning restore 0649

        [InjectOnClick(Resource.Id.btn_post)]
        public void OnPost(object sender, EventArgs e)
        {
            loadLayout.Visibility = ViewStates.Visible;
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

            photoFrame.SetBackgroundColor(Color.Black);
            var parameters = photoFrame.LayoutParameters;
            parameters.Height = Resources.DisplayMetrics.WidthPixels;
            photoFrame.LayoutParameters = parameters;
            postButton.Enabled = true;
            Path = Intent.GetStringExtra("FILEPATH");

            Picasso.With(this).Load(new Java.IO.File(Path))
                   .MemoryPolicy(MemoryPolicy.NoCache, MemoryPolicy.NoStore)
                   .Resize(this.Resources.DisplayMetrics.WidthPixels, 0)
                   .Into(photoFrame);
        }


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
                tag.FindViewById<TextView>(Resource.Id.text).Text = item;
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

            AddTags(tags);
        }

        void HandleAction()
        {

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //_tokenSource.Cancel();
            //_tokenSource.Dispose();
            if (bitmapToUpload != null)
            {
                bitmapToUpload.Recycle();
                bitmapToUpload.Dispose();
                bitmapToUpload = null;
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

        private async Task OnPostAsync()
        {
            try
            {
                //photoComressionTask.Wait();
                var success = await CompressPhoto();
                if (!success)
                    ShowAlert("Photo upload error, please try again");

                var request = new Sweetshot.Library.Models.Requests.UploadImageRequest(BasePresenter.User.SessionId, description.Text, arrayToUpload, tags.ToArray());
                var resp = await presenter.Upload(request);
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
                if (loadLayout != null)
                    loadLayout.Visibility = ViewStates.Gone;
            }
        }

        private async Task<bool> CompressPhoto()
        {
            try
            {
                await Task.Run(() =>
              {
                  var absolutePath = (new Java.IO.File(Path)).AbsolutePath;
                  bitmapToUpload = BitmapFactory.DecodeFile(absolutePath);
                  bitmapToUpload = RotateImageIfRequired(bitmapToUpload, absolutePath);
              });
                if (bitmapToUpload == null)
                    return false;

                using (var stream = new MemoryStream())
                {
                    await bitmapToUpload.CompressAsync(Bitmap.CompressFormat.Jpeg, 80, stream);
                    arrayToUpload = stream.ToArray();
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
            presenter = new PostDescriptionPresenter(this);
        }

        public void OnPrepareLoad(Drawable p0)
        {

        }
    }
}
