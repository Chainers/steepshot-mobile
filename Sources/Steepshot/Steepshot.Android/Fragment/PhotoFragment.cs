using System;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Activity;
using Steepshot.Base;
using Steepshot.Presenter;

using Steepshot.View;

namespace Steepshot.Fragment
{
	public delegate void VoidDelegate(); 
	public class PhotoFragment : BaseFragment, IPhotoView
	{
		PhotoPresenter _presenter;

#pragma warning disable 0649,4014
		//[InjectView(Resource.Id.container)] FrameLayout Container;
		//[InjectView(Resource.Id.Title)] TextView ViewTitle;
		[InjectView(Resource.Id.btn_switch)] ImageButton _switchButton;
#pragma warning restore 0649

		private Java.IO.File _photo;
		//public event VoidDelegate UpdateProfile;
		//private string stringPath;

		[InjectOnClick(Resource.Id.btn_switch)]
		public void OnSwitcherClick(object sender, EventArgs e)
		{
			var directory = GetDirectoryForPictures();
			_photo = new Java.IO.File(directory, Guid.NewGuid().ToString());

			Intent intent = new Intent(MediaStore.ActionImageCapture);
			intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_photo));
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

		public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
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

		public override void OnViewCreated(Android.Views.View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			OpenGrid();
		}

		private void OpenGrid()
		{
			_switchButton.SetImageResource(Resource.Drawable.ic_camera);
			_switchButton.SetColorFilter(Color.White,PorterDuff.Mode.SrcIn);
			ChildFragmentManager.BeginTransaction().Replace(Resource.Id.container, new PhotoGridFragment(), GridFragmentId).Commit();
		}

		private void OpenCamera()
		{ 
			_switchButton.SetImageResource(Resource.Drawable.ic_grid);
			_switchButton.SetColorFilter(Color.White, PorterDuff.Mode.SrcIn);
			ChildFragmentManager.BeginTransaction().Replace(Resource.Id.container, new CameraFragment(),CameraFragmentId).Commit();
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			Cheeseknife.Reset(this);
		}

		private Java.IO.File GetDirectoryForPictures()
		{
			var dir = new Java.IO.File(
				Android.OS.Environment.GetExternalStoragePublicDirectory(
					Android.OS.Environment.DirectoryPictures), "Steepshot");
			if (!dir.Exists())
				dir.Mkdirs();
			
			return dir;
		}

		protected override void CreatePresenter()
		{
			_presenter = new PhotoPresenter(this);
		}
	}
}
