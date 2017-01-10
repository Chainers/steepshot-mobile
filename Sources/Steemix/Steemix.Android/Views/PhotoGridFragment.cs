using Android.Support.V4.App;
using Android.Views;
using Android.OS;
using Com.Lilarcor.Cheeseknife;
using Steemix.Droid.Activity;
using Android.Widget;
using Android.Support.V7.Widget;
using Steemix.Droid.Adapter;
using System.Collections.Generic;
using Android.Database;
using Android.Provider;
using Android.Net;

namespace Steemix.Droid
{
	public class PhotoGridFragment : BaseFragment<PhotoGridViewModel>
	{
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
			Adapter = new GalleryAdapter(Context, GetAllShownImagesPaths());
			ImagesList.SetAdapter(Adapter);
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			Cheeseknife.Reset(this);
		}

		private List<string> GetAllShownImagesPaths()
		{
			Uri uri;
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
			return listOfAllImages;
		}
	}
}
