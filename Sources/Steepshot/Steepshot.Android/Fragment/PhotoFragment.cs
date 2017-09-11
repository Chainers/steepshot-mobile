using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Java.IO;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public delegate void VoidDelegate();
    public class PhotoFragment : BaseFragment
    {
#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.images_list)] RecyclerView _imagesList;
        [InjectView(Resource.Id.btn_switch)] ImageButton _switchButton;
#pragma warning restore 0649

        private GridImageAdapter _adapter;
        private string _photoUri;
        private List<string> _userPhotos = new List<string>();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.lyt_fragment_photo, null);
            Cheeseknife.Inject(this, v);
            return v;
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == -1 && requestCode == 0)
            {
                var i = new Intent(Context, typeof(PostDescriptionActivity));
                i.PutExtra("FILEPATH", _photoUri);
                StartActivity(i);
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            _switchButton.SetImageResource(Resource.Drawable.ic_camera);
            _switchButton.SetColorFilter(Color.White, PorterDuff.Mode.SrcIn);
            _imagesList.SetLayoutManager(new GridLayoutManager(Context, 3));
            _imagesList.AddItemDecoration(new GridItemdecoration(2, 3));
            _adapter = new GridImageAdapter(Context, _userPhotos);
            _adapter.Click += StartPost;
            _imagesList.SetAdapter(_adapter);
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            Cheeseknife.Reset(this);
        }

        [InjectOnClick(Resource.Id.btn_switch)]
        public void OnSwitcherClick(object sender, EventArgs e)
        {
            var directory = GetSteepshotDirectory();
            _photoUri = $"{directory}/{Guid.NewGuid()}.jpeg";
            var intent = new Intent(MediaStore.ActionImageCapture);
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.Parse(_photoUri.ToFilePath()));
            StartActivityForResult(intent, 0);
        }

        private void StartPost(int obj)
        {
            var i = new Intent(Context, typeof(PostDescriptionActivity));
            i.PutExtra("FILEPATH", _adapter.GetItem(obj));
            Context.StartActivity(i);
        }

        private List<string> GetImages()
        {
            String dcimPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim).AbsolutePath;
            String picturePath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;

            var dcimPhotos = new File(dcimPath);
            var cameraPhotos = new File(dcimPath, "Camera");
            var screenshots = new File(dcimPath, "Screenshots");
            var steepshotPhotos = new File(picturePath, "Steepshot");

            var photos = new List<File>();

            var dcimPhotosList = dcimPhotos.ListFiles();
            if (dcimPhotosList != null)
                photos.AddRange(dcimPhotosList);

            var cameraPhotosList = cameraPhotos.ListFiles();
            if (cameraPhotosList != null)
                photos.AddRange(cameraPhotosList);

            var screenshotsList = screenshots.ListFiles();
            if (screenshotsList != null)
                photos.AddRange(screenshotsList);

            var steepshotPhotosList = steepshotPhotos.ListFiles();
            if (steepshotPhotosList != null)
                photos.AddRange(steepshotPhotosList);

            return photos.Where(f => IsImage(f))
                          .OrderByDescending(f => f.LastModified())
                          .Select(i => i.AbsolutePath).ToList();
        }

        private void AddSteepshotPictures(List<string> listOfAllImages)
        {
            var sdCardRoot = GetSteepshotDirectory();
            foreach (var f in sdCardRoot.ListFiles())
            {
                if (IsImage(f))
                    listOfAllImages.Add(f.AbsolutePath);
            }
        }

        private bool IsImage(Java.IO.File f)
        {
            return f.IsFile && (
                       f.AbsolutePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                       || f.AbsolutePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                       || f.AbsolutePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                       || f.AbsolutePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase));
        }

        public override bool CustomUserVisibleHint
        {
            get => base.CustomUserVisibleHint;
            set
            {
                if (!value)
                    _adapter?.ClearCache();
                UserVisibleHint = value;
            }
        }

        public override void OnResume()
        {
            base.OnResume();
            var newImages = GetImages();
            if (!_userPhotos.SequenceEqual(newImages))
            {
                _userPhotos.Clear();
                _userPhotos.AddRange(newImages);
                _adapter.ClearCache();
                _adapter?.NotifyDataSetChanged();
            }
        }

        private Java.IO.File GetSteepshotDirectory()
        {
            var dir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
            var file = new Java.IO.File(dir, "Steepshot");
            if (!file.Exists())
                file.Mkdirs();
            return file;
        }
    }
}
