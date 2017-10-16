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
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PhotoFragment : BaseFragment
    {
#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.images_list)] RecyclerView _imagesList;
        [InjectView(Resource.Id.btn_switch)] ImageButton _switchButton;
        //[InjectView(Resource.Id.spinner_photoDir)] Spinner _photoDir;
#pragma warning restore 0649

        private GridImageAdapter _adapter;
        private string _photoUri;
        private List<string> _userPhotos = new List<string>();
        private List<KeyValuePair<string, string>> PhotoDirectories { get; set; } = new List<KeyValuePair<string, string>>();

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
            InitPhotoDirectories();
            //var adapter = new ArrayAdapter<String>(Context, Resource.Drawable.spinner_item, PhotoDirectories.Select(i => i.Key).ToArray());
            //_photoDir.Adapter = adapter;

            var index = 0;
            for (int i = 0; i < PhotoDirectories.Count; i++)
            {
                if (PhotoDirectories[i].Key.LastIndexOf(BasePresenter.User.DefaultPhotoDirectory, StringComparison.OrdinalIgnoreCase) != -1)
                    index = i;
            }

            //_photoDir.SetSelection(index);
            //_photoDir.ItemSelected += _photoDir_ItemSelected;
        }

        //private void _photoDir_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        //{
        //    BasePresenter.User.DefaultPhotoDirectory = _photoDir.SelectedItem.ToString();
        //    SwitchPhotoDir();
        //}

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            Cheeseknife.Reset(this);
        }

        //[InjectOnClick(Resource.Id.spinner_container)]
        //public void SpinnerClick(object sender, EventArgs e)
        //{
        //    _photoDir.PerformClick();
        //}

        [InjectOnClick(Resource.Id.btn_switch)]
        public void OnSwitcherClick(object sender, EventArgs e)
        {
            var directoryPictures = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
            var directory = new File(directoryPictures, Constants.Steepshot);
            if (!directory.Exists())
                directory.Mkdirs();

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

        private void InitPhotoDirectories()
        {
            var dcim = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim);
            AddPathToPhotoDirectories(dcim, dcim.Name);
            var pic = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
            AddPathToPhotoDirectories(pic, pic.Name);
        }
        
        private IEnumerable<string> GetImages(string name = null)
        {
            if (!PhotoDirectories.Any())
                return new List<string>();

            var photos = new List<File>();

            //    var dir = string.IsNullOrEmpty(name)
            //        ? PhotoDirectories.FirstOrDefault()
            //        : PhotoDirectories.FirstOrDefault(i => i.Key.LastIndexOf(name, StringComparison.OrdinalIgnoreCase) != -1);
            //    if (string.IsNullOrEmpty(dir.Value))
            //        return new List<string>();
            //    AddPhotos(photos, dir.Value);

            foreach (var dir in PhotoDirectories)
            {
                if (string.IsNullOrEmpty(dir.Value))
                    return new List<string>();
                AddPhotos(photos, dir.Value);
            }
            return photos.OrderByDescending(f => f.LastModified()).Select(i => i.AbsolutePath);
        }

        private void SwitchPhotoDir()
        {
            //    var newImages = GetImages(BasePresenter.User.DefaultPhotoDirectory);
            var newImages = GetImages();
            if (!_userPhotos.SequenceEqual(newImages))
            {
                _userPhotos.Clear();
                _userPhotos.AddRange(newImages);
                _adapter.ClearCache();
                _adapter?.NotifyDataSetChanged();
            }
        }

        private void AddPathToPhotoDirectories(File file, string fileName)
        {
            if (!file.IsDirectory || file.Name.StartsWith("."))
                return;

            if (file.Name.Equals(Constants.Steepshot, StringComparison.OrdinalIgnoreCase) || file.ListFiles().Any(IsImage))
                PhotoDirectories.Add(new KeyValuePair<string, string>(fileName, file.AbsolutePath));

            foreach (var difFile in file.ListFiles())
            {
                if (file.IsDirectory)
                {
                    var name = difFile.AbsolutePath.Remove(0, file.AbsolutePath.Length);
                    AddPathToPhotoDirectories(difFile, name);
                }
            }
        }

        private void AddPhotos(List<File> list, string dir)
        {
            var file = new File(dir);
            if (!file.Exists())
                return;

            var dcimPhotosList = file.ListFiles();
            if (dcimPhotosList != null)
                list.AddRange(dcimPhotosList.Where(IsImage));

        }

        private void AddPhotos(List<File> list, IEnumerable<string> dirs)
        {
            foreach (var dir in dirs)
                AddPhotos(list, dir);
        }

        private bool IsImage(File f)
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
            SwitchPhotoDir();
        }
    }
}
