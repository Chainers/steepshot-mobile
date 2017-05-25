using System;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;

namespace Steepshot
{
	public delegate void VoidDelegate(); 
	public class PhotoFragment : BaseFragment, PhotoView
	{
		PhotoPresenter presenter;

		[InjectView(Resource.Id.container)]
		FrameLayout Container;

		[InjectView(Resource.Id.Title)]
		TextView ViewTitle;

		[InjectView(Resource.Id.btn_switch)]
		ImageButton SwitchButton;

		private Java.IO.File photo;
		public event VoidDelegate UpdateProfile;

		private string stringPath;

		[InjectOnClick(Resource.Id.btn_switch)]
		public void OnSwitcherClick(object sender, EventArgs e)
		{
			var directory = GetDirectoryForPictures();
			photo = new Java.IO.File(directory, System.Guid.NewGuid().ToString());

			Intent intent = new Intent(MediaStore.ActionImageCapture);
			intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(photo));
			StartActivityForResult(intent, 0);

			/*if (ChildFragmentManager.FindFragmentByTag(GridFragmentId) != null)
			{
				OpenCamera();
			}
			else if (ChildFragmentManager.FindFragmentByTag(CameraFragmentId) != null)
			{
				OpenGrid();
			}*/
		}

		public const string CameraFragmentId = "CameraFragmentId";
		public const string GridFragmentId = "GridFragmentId";

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
				i.PutExtra("FILEPATH", Android.Net.Uri.FromFile(photo).Path);
				StartActivity(i);
			}
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			OpenGrid();
		}

		private void OpenGrid()
		{
			SwitchButton.SetImageResource(Resource.Drawable.ic_camera);
			SwitchButton.SetColorFilter(Color.White,PorterDuff.Mode.SrcIn);
			ChildFragmentManager.BeginTransaction().Replace(Resource.Id.container, new PhotoGridFragment(), GridFragmentId).Commit();
		}

		private void OpenCamera()
		{ 
			SwitchButton.SetImageResource(Resource.Drawable.ic_grid);
			SwitchButton.SetColorFilter(Color.White, PorterDuff.Mode.SrcIn);
			ChildFragmentManager.BeginTransaction().Replace(Resource.Id.container, new CameraFragment(),CameraFragmentId).Commit();
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			Cheeseknife.Reset(this);
		}

		private Java.IO.File GetDirectoryForPictures()
		{
			var _dir = new Java.IO.File(
				Android.OS.Environment.GetExternalStoragePublicDirectory(
					Android.OS.Environment.DirectoryPictures), "SteepShot");
			if (!_dir.Exists())
				_dir.Mkdirs();
			
			return _dir;
		}

		protected override void CreatePresenter()
		{
			presenter = new PhotoPresenter(this);
		}
	}
}
