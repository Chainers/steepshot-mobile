using System;
using System.Collections.Generic;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
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

        private GridImageAdapter<string> _adapter;
        private Java.IO.File _photo;

        private GridImageAdapter<string> Adapter
        {
            get
            {
                if (_adapter == null)
                {
                    _adapter = new GridImageAdapter<string>(Context, GetAllShownImagesPaths());
                    _adapter.Click += StartPost;
                }
                return _adapter;
            }
        }

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
                Intent i = new Intent(Context, typeof(PostDescriptionActivity));
                i.PutExtra("FILEPATH", Android.Net.Uri.FromFile(_photo).Path);
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
            _imagesList.SetAdapter(Adapter);
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
            _photo = new Java.IO.File(directory, Guid.NewGuid().ToString());

            Intent intent = new Intent(MediaStore.ActionImageCapture);
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_photo));
            StartActivityForResult(intent, 0);
        }

        private void StartPost(int obj)
        {
            var i = new Intent(Context, typeof(PostDescriptionActivity));
            i.PutExtra("FILEPATH", Adapter.GetItem(obj));
            Context.StartActivity(i);
        }

        private List<string> GetAllShownImagesPaths()
        {
            var listOfAllImages = new List<string>();
            AddMediaStoreImages(listOfAllImages);
            AddSteepshotPictures(listOfAllImages);
            listOfAllImages.Reverse();
            return listOfAllImages;
        }

        private void AddMediaStoreImages(List<string> listOfAllImages)
        {
            var uri = MediaStore.Images.Media.ExternalContentUri;
            string[] projection = { MediaStore.MediaColumns.Data, MediaStore.Images.Media.InterfaceConsts.BucketDisplayName };
            var loader = new CursorLoader(Context, uri, projection, null, null, MediaStore.Images.Media.InterfaceConsts.DateAdded);
            var cursor = (ICursor)loader.LoadInBackground();
            var columnIndexData = cursor.GetColumnIndexOrThrow(MediaStore.MediaColumns.Data);
            while (cursor.MoveToNext())
            {
                var absolutePathOfImage = cursor.GetString(columnIndexData);
                listOfAllImages.Add(absolutePathOfImage);
            }
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
