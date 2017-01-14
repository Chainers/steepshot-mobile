using System;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;

namespace Steemix.Droid.Fragments
{
	public class PhotoFragment : Fragment
	{
		[InjectView(Resource.Id.container)]
		FrameLayout Container;

		[InjectView(Resource.Id.Title)]
		TextView ViewTitle;

		[InjectView(Resource.Id.btn_switch)]
		ImageButton SwitchButton;

		[InjectOnClick(Resource.Id.btn_switch)]
		public void OnSwitcherClick(object sender, EventArgs e)
		{
			if (ChildFragmentManager.FindFragmentByTag(GridFragmentId) != null)
			{
				OpenCamera();
			}
			else if (ChildFragmentManager.FindFragmentByTag(CameraFragmentId) != null)
			{
				OpenGrid();
			}
		}

		public const string CameraFragmentId = "CameraFragmentId";
		public const string GridFragmentId = "GridFragmentId";

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var v = inflater.Inflate(Resource.Layout.lyt_fragment_photo, null);
			Cheeseknife.Inject(this, v);
			return v;
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
	}
}
