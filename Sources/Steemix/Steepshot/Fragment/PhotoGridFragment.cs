using System;
using System.Collections.Generic;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Views;
using Com.Lilarcor.Cheeseknife;
using Java.IO;
using System.Linq;

namespace Steepshot
{
	public class PhotoGridFragment : BaseFragment, PhotoGridView
	{
		PhotoGridPresenter presenter;

		[InjectView(Resource.Id.images_list)]
		RecyclerView ImagesList;

		GalleryAdapter Adapter;
		
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var v = inflater.Inflate(Resource.Layout.lyt_fragment_photo_grid, null);
			Cheeseknife.Inject(this, v);
			return v;
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			ImagesList.SetLayoutManager(new GridLayoutManager(Context,3));
			ImagesList.AddItemDecoration(new GridItemdecoration(2, 3));
			Adapter = new GalleryAdapter(Context);
			ImagesList.SetAdapter(Adapter);

			Adapter.PhotoClick += Adapter_Click;
		}

		void Adapter_Click(int obj)
		{
			StartPost(obj);
		}

		private void StartPost(int obj)
		{
			Intent i = new Intent(Context, typeof(PostDescriptionActivity));
			i.PutExtra("FILEPATH", Adapter.GetItem(obj));
			Context.StartActivity(i);
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			Cheeseknife.Reset(this);
		}

		public override void OnResume()
		{
			base.OnResume();
			Adapter.Reset(GetAllShownImagesPaths());
		}

		public override void OnPause()
		{
			Adapter.Clear();
			base.OnPause();
		}

		private Java.IO.File GetDirectoryForPictures()
		{
			var _dir = new Java.IO.File(
				Android.OS.Environment.GetExternalStoragePublicDirectory(
					Android.OS.Environment.DirectoryPictures), "SteepShot");
			if (!_dir.Exists())
			{
				_dir.Mkdirs();
			}

			return _dir;
		}

		private void GetAppPictures(List<string> listOfAllImages)
		{
			File sdCardRoot = GetDirectoryForPictures();
			foreach (File f in sdCardRoot.ListFiles())
			{
				if (f.IsFile)
					listOfAllImages.Insert(0,f.AbsolutePath);
			}
		}

		private List<string> GetAllShownImagesPaths()
		{
			Android.Net.Uri uri;
			ICursor cursor;
			int column_index_data, Column_index_folder_name;

			List<string> listOfAllImages = new List<string>();

			string AbsolutePathOfImage = null;

			uri = MediaStore.Images.Media.ExternalContentUri;

			string[] projection = { MediaStore.MediaColumns.Data,MediaStore.Images.Media.InterfaceConsts.BucketDisplayName};

			cursor = Context.ContentResolver.Query(uri, projection, null, null, null);

			column_index_data = cursor.GetColumnIndexOrThrow(MediaStore.MediaColumns.Data);
			Column_index_folder_name = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.BucketDisplayName);
			while (cursor.MoveToNext())
			{
				AbsolutePathOfImage = cursor.GetString(column_index_data);
				listOfAllImages.Add(AbsolutePathOfImage);
			}
            listOfAllImages.Reverse();
            GetAppPictures(listOfAllImages);
            return listOfAllImages ;
		}

		protected override void CreatePresenter()
		{
			presenter = new PhotoGridPresenter(this);
		}
	}
}
