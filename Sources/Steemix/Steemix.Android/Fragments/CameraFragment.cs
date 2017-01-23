using System;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steemix.Droid.ViewModels;

namespace Steemix.Droid.Fragments
{
	public class CameraFragment: BaseFragment<CameraViewModel>
	{
		CameraPreview CameraPreview;

		Android.Hardware.Camera Camera;
		public static int Camera_Request_Code = 1488;

		public const string CameraPermission = Android.Manifest.Permission.Camera;
		public const string WritePermission = Android.Manifest.Permission.WriteExternalStorage;

		[InjectView(Resource.Id.camera_frame)]
		FrameLayout CameraContainer;

		[InjectOnClick(Resource.Id.take_photo)]
		public void TakePhotoClick(object sender, EventArgs e)
		{
			Camera.TakePicture(CameraPreview, null, null, CameraPreview);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var v = inflater.Inflate(Resource.Layout.lyt_fragment_take_photo, null);
			Cheeseknife.Inject(this, v);
			return v;
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			if ((int)Build.VERSION.SdkInt >= 23 && ((Context.CheckSelfPermission(CameraPermission) != (int)Permission.Granted) || (Context.CheckSelfPermission(WritePermission) != (int)Permission.Granted)))
			{
				RequestPermissions(new string[] { CameraPermission,WritePermission}, Camera_Request_Code);
			}
			else
			{
				InitCamera();
			}

			CameraPreview.PictureTaken += (sender, e) =>
			{
				StartPost(e);
			};
		}

		private void StartPost(string path)
		{ 
			Camera.StopPreview();
			Intent i = new Intent(Context, typeof(PostDescriptionActivity));
			i.PutExtra("FILEPATH", path);
			Context.StartActivity(i);
		}

		public override void OnActivityResult(int requestCode, int resultCode, Android.Content.Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			if (requestCode == Camera_Request_Code)
			{
				if (Context.CheckSelfPermission(CameraPermission) == (int)Permission.Granted && Context.CheckSelfPermission(WritePermission) == (int)Permission.Granted)
				{
					InitCamera();
				}
			}
		}

		public override void OnPause()
		{
			Camera.StopPreview();
			base.OnPause();
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			Camera.Release();
			Cheeseknife.Reset(this);
		}

		public void InitCamera()
		{
			Camera = GetCameraInstance();

			CameraPreview = new CameraPreview(Context, Camera);
			CameraContainer.AddView(CameraPreview);
		}

		public static Android.Hardware.Camera GetCameraInstance()
		{
			Android.Hardware.Camera c = null;
			try
			{
				c = Android.Hardware.Camera.Open();
			}
			catch { }
			return c;
		}
	}
}

