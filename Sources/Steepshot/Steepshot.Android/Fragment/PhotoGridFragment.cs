using System.Collections.Generic;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Views;
using Com.Lilarcor.Cheeseknife;
using Java.IO;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Presenter;
using Steepshot.Utils;
using Steepshot.View;

namespace Steepshot.Fragment
{
	public class PhotoGridFragment : BaseFragment, IPhotoGridView
	{
		PhotoGridPresenter _presenter;
#pragma warning disable 0649, 4014
		[InjectView(Resource.Id.images_list)] RecyclerView _imagesList;
#pragma warning restore 0649

		GalleryAdapter _adapter;
		
		public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var v = inflater.Inflate(Resource.Layout.lyt_fragment_photo_grid, null);
			Cheeseknife.Inject(this, v);
			return v;
		}

		public override void OnViewCreated(Android.Views.View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			_imagesList.SetLayoutManager(new GridLayoutManager(Context,3));
			_imagesList.AddItemDecoration(new GridItemdecoration(2, 3));
			_adapter = new GalleryAdapter(Context);
			_imagesList.SetAdapter(_adapter);
			_adapter.Reset(GetAllShownImagesPaths());
			_adapter.PhotoClick += Adapter_Click;
		}

		void Adapter_Click(int obj)
		{
			StartPost(obj);
		}

		private void StartPost(int obj)
		{
			Intent i = new Intent(Context, typeof(PostDescriptionActivity));
			i.PutExtra("FILEPATH", _adapter.GetItem(obj));
			Context.StartActivity(i);
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			Cheeseknife.Reset(this);
		}
		/*
		public override void OnResume()
		{
			base.OnResume();
			//Adapter.Reset(GetAllShownImagesPaths());
		}

		public override void OnPause()
		{
			//Adapter.Clear();
			base.OnPause();
		}
*/
		private File GetDirectoryForPictures()
		{
			var dir = new File(
				Environment.GetExternalStoragePublicDirectory(
					Environment.DirectoryPictures), "Steepshot");
			if (!dir.Exists())
			{
				dir.Mkdirs();
			}

			return dir;
		}

		private void GetAppPictures(List<string> listOfAllImages)
		{
			File sdCardRoot = GetDirectoryForPictures();
			foreach (File f in sdCardRoot.ListFiles())
			{
				if (f.IsFile && f.AbsolutePath.Contains(".jpg"))
					listOfAllImages.Insert(0,f.AbsolutePath);
			}
		}

		private List<string> GetAllShownImagesPaths()
		{
			Android.Net.Uri uri;
			ICursor cursor;
			int columnIndexData, columnIndexFolderName;

			List<string> listOfAllImages = new List<string>();

			string absolutePathOfImage = null;

			uri = MediaStore.Images.Media.ExternalContentUri;

			//String selection = MediaStore.Images.Media.InterfaceConsts.BucketDisplayName;
			//String[] selectionArgs = new String[] {"DCIM"};

			string[] projection = { MediaStore.MediaColumns.Data, MediaStore.Images.Media.InterfaceConsts.BucketDisplayName };
			var loader = new CursorLoader(Context, uri, projection, null, null, MediaStore.Images.Media.InterfaceConsts.DateAdded);
			//cursor = Context.ContentResolver.Query(uri, projection, null, null, MediaStore.Images.Media.InterfaceConsts.DateAdded);
			cursor = (ICursor)loader.LoadInBackground();
			columnIndexData = cursor.GetColumnIndexOrThrow(MediaStore.MediaColumns.Data);
			columnIndexFolderName = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.BucketDisplayName);
			while (cursor.MoveToNext())
			{
				absolutePathOfImage = cursor.GetString(columnIndexData);
				//if (AbsolutePathOfImage.)
				//{
					listOfAllImages.Add(absolutePathOfImage);
				//}
			}
            listOfAllImages.Reverse();
            GetAppPictures(listOfAllImages);
            return listOfAllImages;
		}

		protected override void CreatePresenter()
		{
			_presenter = new PhotoGridPresenter(this);
		}
	}
}
