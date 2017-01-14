using Android.Views;
using Android.OS;
using Com.Lilarcor.Cheeseknife;
using Android.Widget;
using Android.Content.PM;

namespace Steemix.Droid
{
	public class CameraFragment: BaseFragment<CameraViewModel>
	{
		CameraPreview CameraPreview;

		Android.Hardware.Camera Camera;
		public static int Camera_Request_Code = 1488;

		public const string CameraPermission = Android.Manifest.Permission.Camera;

		[InjectView(Resource.Id.camera_frame)]
		FrameLayout CameraContainer;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var v = inflater.Inflate(Resource.Layout.lyt_fragment_take_photo, null);
			Cheeseknife.Inject(this, v);
			return v;
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			if ((int)Build.VERSION.SdkInt >= 23 && (Context.CheckSelfPermission(CameraPermission) != (int)Permission.Granted))
			{
				RequestPermissions(new string[] { CameraPermission}, Camera_Request_Code);
			}
			else
			{
				InitCamera();
			}
		}

		public override void OnActivityResult(int requestCode, int resultCode, Android.Content.Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			if (requestCode == Camera_Request_Code)
			{
				if (Context.CheckSelfPermission(CameraPermission) == (int)Permission.Granted)
				{
					InitCamera();
				}
			}
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
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

