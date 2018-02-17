//using System;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Support.V4.Content;
//using Android.Views;
//using Android.Widget;
//using Com.Lilarcor.Cheeseknife;
//using Steepshot.Activity;
//using Steepshot.Base;
//using Steepshot.Utils;

//namespace Steepshot.Fragment
//{
//    //TODO:KOA - In the development. Needed for replace PhotoFragment
//    public class CameraFragment : BaseFragment
//    {
//        CameraPreview _cameraPreview;

//        Android.Hardware.Camera _camera;
//        public static int CameraRequestCode = 1488;

//        public const string CameraPermission = Android.Manifest.Permission.Camera;
//        public const string WritePermission = Android.Manifest.Permission.WriteExternalStorage;

//        [InjectView(Resource.Id.camera_frame)]
//        FrameLayout _cameraContainer;

//        [InjectOnClick(Resource.Id.take_photo)]
//        public void TakePhotoClick(object sender, EventArgs e)
//        {
//            _camera.TakePicture(_cameraPreview, null, null, _cameraPreview);
//        }

//        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
//        {
//            var v = inflater.Inflate(Resource.Layout.lyt_fragment_take_photo, null);
//            Cheeseknife.Inject(this, v);
//            return v;
//        }

//        public override void OnViewCreated(View view, Bundle savedInstanceState)
//        {
//            base.OnViewCreated(view, savedInstanceState);
//            if ((int)Build.VERSION.SdkInt >= 23 && ((ContextCompat.CheckSelfPermission(Context, CameraPermission) != (int)Permission.Granted) || (ContextCompat.CheckSelfPermission(Context, WritePermission) != (int)Permission.Granted)))
//            {
//                RequestPermissions(new[] { CameraPermission, WritePermission }, CameraRequestCode);
//            }
//            else
//            {
//                InitCamera();
//            }

//            _cameraPreview.PictureTaken += (sender, e) =>
//            {
//                StartPost(e);
//            };
//        }

//        private void StartPost(string path)
//        {
//            _camera.StopPreview();
//            var i = new Intent(Context, typeof(PostDescriptionActivity));
//            i.PutExtra(PostDescriptionActivity.MediaPathExtra, path);
//            Context.StartActivity(i);
//        }

//        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
//        {
//            base.OnActivityResult(requestCode, resultCode, data);
//            if (requestCode == CameraRequestCode)
//            {
//                if (ContextCompat.CheckSelfPermission(Context, CameraPermission) == (int)Permission.Granted && ContextCompat.CheckSelfPermission(Context, WritePermission) == (int)Permission.Granted)
//                {
//                    InitCamera();
//                }
//            }
//        }

//        public override void OnPause()
//        {
//            _camera.StopPreview();
//            base.OnPause();
//        }

//        public override void OnDestroyView()
//        {
//            base.OnDestroyView();
//            _camera.Release();
//            Cheeseknife.Reset(this);
//        }

//        public void InitCamera()
//        {
//            _camera = GetCameraInstance();

//            _cameraPreview = new CameraPreview(Context, _camera);
//            _cameraContainer.AddView(_cameraPreview);
//        }

//        public static Android.Hardware.Camera GetCameraInstance()
//        {
//            Android.Hardware.Camera c = null;
//            try
//            {
//                c = Android.Hardware.Camera.Open();
//            }
//            catch { }
//            return c;
//        }
//    }
//}