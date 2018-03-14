using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Newtonsoft.Json;
using Refractored.Controls;
using Steepshot.Activity;
using Steepshot.Base;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Camera = Android.Hardware.Camera;

namespace Steepshot.Fragment
{
#pragma warning disable 0649, 4014, 0618
    public sealed class OldCameraFragment : BaseFragment, ISurfaceHolderCallback, Camera.IPictureCallback, Camera.IShutterCallback
    {
        private const bool FullScreen = true;
        private const int GalleryRequestCode = 228;

        private ISurfaceHolder _holder;
        private Camera _camera;
        private int _cameraId;
        private int _currentRotation;
        private int _rotationOnShutter;
        private float _dist;
        private CameraOrientationEventListener _orientationListner;

        [InjectView(Resource.Id.surfaceView)] private SurfaceView _sv;
        [InjectView(Resource.Id.flash_button)] private ImageButton _flashButton;
        [InjectView(Resource.Id.shot_button)] private ImageButton _shotButton;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _progressBar;
        [InjectView(Resource.Id.revert_button)] private ImageButton _revertButton;
        [InjectView(Resource.Id.close_button)] private ImageButton _closeButton;
        [InjectView(Resource.Id.gallery_button)] private RelativeLayout _galleryButton;
        [InjectView(Resource.Id.gallery_icon)] private CircleImageView _galleryIcon;

#pragma warning restore 0649

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.lyt_old_camera, null);
            Cheeseknife.Inject(this, v);
            return v;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (Camera.NumberOfCameras < 2)
                _revertButton.Visibility = ViewStates.Gone;

            _flashButton.Click += FlashClick;
            _shotButton.Click += TakePhotoClick;
            _closeButton.Click += GoBack;
            _galleryButton.Click += OpenGallery;
            _revertButton.Click += SwitchCamera;
            _sv.Touch += SvOnTouch;

            _orientationListner = new CameraOrientationEventListener(Activity, SensorDelay.Normal);
            _orientationListner.OrientationChanged += OnOrientationChanged;
            GetGalleryIcon();
        }

        private void SvOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            Camera.Parameters cameraParams = _camera.GetParameters();
            var action = touchEventArgs.Event.Action;

            if (touchEventArgs.Event.PointerCount > 1)
            {
                if (action == MotionEventActions.PointerDown)
                {
                    _dist = GetFingerSpacing(touchEventArgs.Event);
                }
                else if (action == MotionEventActions.Move && cameraParams.IsZoomSupported)
                {
                    _camera.CancelAutoFocus();
                    HandleZoom(touchEventArgs.Event, cameraParams);
                }
            }
            touchEventArgs.Handled = true;
        }

        private void HandleZoom(MotionEvent e, Camera.Parameters p)
        {
            int maxZoom = p.MaxZoom;
            int zoom = p.Zoom;
            float newDist = GetFingerSpacing(e);
            if (newDist > _dist)
            {
                if (zoom < maxZoom)
                    zoom++;
            }
            else if (newDist < _dist)
            {
                if (zoom > 0)
                    zoom--;
            }
            _dist = newDist;
            p.Zoom = zoom;
            _camera.SetParameters(p);
        }

        private float GetFingerSpacing(MotionEvent e)
        {
            float x = e.GetX(0) - e.GetX(1);
            float y = e.GetY(0) - e.GetY(1);
            return (float)Math.Sqrt(x * x + y * y);
        }

        public override void OnResume()
        {
            base.OnResume();
            _holder = _sv.Holder;
            _holder.SetType(SurfaceType.PushBuffers);
            _holder.AddCallback(this);
            EnableCamera(_cameraId);
            _orientationListner.Enable();
        }

        public override void OnPause()
        {
            base.OnPause();

            _camera?.Release();
            _camera = null;

            _orientationListner.Disable();

            _holder.RemoveCallback(this);
            _holder.Dispose();
            _holder = null;
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }

        private void FlashClick(object sender, EventArgs e)
        {
            var parameters = _camera.GetParameters();
            if (parameters.SupportedFlashModes != null
                && parameters.SupportedFlashModes.Contains(Camera.Parameters.FlashModeOff)
                && parameters.SupportedFlashModes.Contains(Camera.Parameters.FlashModeOn))
            {
                var mode = parameters.FlashMode != Camera.Parameters.FlashModeOff ? Camera.Parameters.FlashModeOff : Camera.Parameters.FlashModeOn;
                parameters.FlashMode = mode;
                _camera.SetParameters(parameters);
                _flashButton.SetImageResource(mode == Camera.Parameters.FlashModeOff ? Resource.Drawable.ic_flash_off : Resource.Drawable.ic_flash);
            }
        }

        private void TakePhotoClick(object sender, EventArgs e)
        {
            _camera?.TakePicture(this, null, this);
        }

        private void GoBack(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

        private void OpenGallery(object sender, EventArgs e)
        {
            ((BaseActivity)Activity).OpenNewContentFragment(new GalleryFragment());
        }

        private void OnOrientationChanged(int orientation)
        {
            if (_camera == null)
                return;

            var parameters = _camera.GetParameters();
            var info = new Camera.CameraInfo();
            Camera.GetCameraInfo(_cameraId, info);

            orientation = (orientation + 45) / 90 * 90;
            var rotation = info.Facing == Camera.CameraInfo.CameraFacingFront
                ? (info.Orientation - orientation + 360) % 360
                : (info.Orientation + orientation) % 360;

            _currentRotation = rotation;
            parameters.SetRotation(rotation);
            _camera.SetParameters(parameters);
        }


        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            SetCameraDisplayOrientation(_cameraId);
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            if (_camera == null)
                return;
            try
            {
                _camera.SetPreviewDisplay(holder);
                _camera.StartPreview();
            }
            catch (Java.IO.IOException ex)
            {
                AppSettings.Reporter.SendCrash(ex);
                Activity.ShowAlert(LocalizationKeys.CameraSettingError, ToastLength.Short);
            }
        }

        private void SetPreviewSize(bool fullScreen)
        {
            var parameters = _camera.GetParameters();

            if (parameters.SupportedFlashModes == null || parameters.SupportedFlashModes.Count == 1)
                _flashButton.Visibility = ViewStates.Gone;
            else
                _flashButton.Visibility = ViewStates.Visible;

            var cameraSizes = GetSizes(parameters.SupportedPreviewSizes, parameters.SupportedPictureSizes);
            parameters.SetPictureSize(cameraSizes.Item2.Width, cameraSizes.Item2.Height);
            parameters.SetPreviewSize(cameraSizes.Item1.Width, cameraSizes.Item1.Height);
            _camera.SetParameters(parameters);
            var size = _camera.GetParameters().PreviewSize;

            var rectDisplay = new RectF();
            var rectPreview = new RectF();

            var display = Activity.WindowManager.DefaultDisplay;
            rectDisplay.Set(0, 0, display.Width, display.Height);

            if (display.Width > display.Height)
                rectPreview.Set(0, 0, size.Width, size.Height);
            else
                rectPreview.Set(0, 0, size.Height, size.Width);

            var matrix = new Matrix();
            if (fullScreen)
            {
                matrix.SetRectToRect(rectDisplay, rectPreview, Matrix.ScaleToFit.Start);
                matrix.Invert(matrix);
            }
            else
            {
                matrix.SetRectToRect(rectPreview, rectDisplay, Matrix.ScaleToFit.Start);
            }

            matrix.MapRect(rectPreview);
            _sv.LayoutParameters.Height = (int)rectPreview.Bottom;
            _sv.LayoutParameters.Width = (int)rectPreview.Right;
        }

        private Tuple<Camera.Size, Camera.Size> GetSizes(IList<Camera.Size> supportedPreviewSizes, IList<Camera.Size> supportedPictureSizes)
        {
            var previewSizes = supportedPreviewSizes.OrderByDescending(arg => arg.Width).ToArray();
            var pictureSizes = supportedPictureSizes.OrderByDescending(arg => arg.Width).ToArray();

            Tuple<Camera.Size, Camera.Size> rez = null;
            var difference = int.MaxValue;

            foreach (var previewSize in previewSizes)
            {
                var previewCoeff = (double)previewSize.Height / previewSize.Width;

                foreach (var pictureSize in pictureSizes)
                {
                    var picCoeff = (double)pictureSize.Height / pictureSize.Width;
                    if (Math.Abs(picCoeff - previewCoeff) < 0.001)
                    {
                        var t = Math.Abs(1600 - pictureSize.Width);
                        if (t < difference)
                        {
                            difference = t;
                            rez = new Tuple<Camera.Size, Camera.Size>(previewSize, pictureSize);
                        }
                    }
                }
                if (rez != null)
                    break;
            }
            return rez ?? new Tuple<Camera.Size, Camera.Size>(previewSizes[0], pictureSizes[0]);
        }

        private void SetCameraDisplayOrientation(int cameraId)
        {
            var rotation = Activity.WindowManager.DefaultDisplay.Rotation;
            var degrees = 0;
            switch (rotation)
            {
                case SurfaceOrientation.Rotation0:
                    degrees = 0;
                    break;
                case SurfaceOrientation.Rotation90:
                    degrees = 90;
                    break;
                case SurfaceOrientation.Rotation180:
                    degrees = 180;
                    break;
                case SurfaceOrientation.Rotation270:
                    degrees = 270;
                    break;
            }

            var result = 0;
            var info = new Camera.CameraInfo();
            Camera.GetCameraInfo(cameraId, info);
            if (info.Facing == CameraFacing.Back)
            {
                result = 360 - degrees + info.Orientation;
            }
            else if (info.Facing == CameraFacing.Front)
            {
                result = 360 - degrees - info.Orientation;
                result += 360;
            }
            result = result % 360;
            _camera.SetDisplayOrientation(result);

            var parameters = _camera.GetParameters();
            parameters.PictureFormat = ImageFormat.Jpeg;
            parameters.JpegQuality = 90;

            if (parameters.SupportedFocusModes != null && parameters.SupportedFocusModes.Contains(Camera.Parameters.FocusModeContinuousVideo))
            {
                parameters.FocusMode = Camera.Parameters.FocusModeContinuousVideo;
            }
            if (parameters.SupportedFlashModes != null && parameters.SupportedFlashModes.Contains(Camera.Parameters.FlashModeOff))
            {
                parameters.FlashMode = Camera.Parameters.FlashModeOff;
            }
            if (parameters.SupportedSceneModes != null && parameters.SupportedSceneModes.Contains(Camera.Parameters.SceneModeAuto))
            {
                parameters.SceneMode = Camera.Parameters.SceneModeAuto;
            }
            if (parameters.SupportedWhiteBalance != null && parameters.SupportedWhiteBalance.Contains(Camera.Parameters.WhiteBalanceAuto))
            {
                parameters.WhiteBalance = Camera.Parameters.WhiteBalanceAuto;
            }
            _camera.SetParameters(parameters);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {

        }

        public void OnPictureTaken(byte[] data, Camera camera)
        {
            Task.Run(() =>
            {
                var directory = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim);
                var photoUri = $"{directory}/{Guid.NewGuid()}.jpeg";

                var stream = new Java.IO.FileOutputStream(photoUri);
                stream.Write(data);
                stream.Close();

                var exifInterface = new ExifInterface(photoUri);
                var orientation = exifInterface.GetAttributeInt(ExifInterface.TagOrientation, 0);

                if (orientation != 1 && orientation != 0)
                {
                    var bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);
                    bitmap = BitmapUtils.RotateImage(bitmap, _rotationOnShutter);
                    var rotationStream = new System.IO.FileStream(photoUri, System.IO.FileMode.Create);
                    bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, rotationStream);
                }

                //var i = new Intent(Context, typeof(PostDescriptionActivity));
                //i.PutExtra(PostDescriptionActivity.PhotoExtraPath, photoUri);
                //i.PutExtra(PostDescriptionActivity.IsNeedCompressExtraPath, false);

                Activity.RunOnUiThread(() =>
                {
                    //StartActivity(i);
                    Activity.Finish();
                    if (_progressBar != null)
                    {
                        _progressBar.Visibility = ViewStates.Gone;
                        _shotButton.Visibility = ViewStates.Visible;
                        _flashButton.Enabled = true;
                        _galleryButton.Enabled = true;
                        _revertButton.Enabled = true;
                        _closeButton.Enabled = true;
                    }
                });
            });
        }

        public void OnShutter()
        {
            _rotationOnShutter = _currentRotation;
            _progressBar.Visibility = ViewStates.Visible;
            _shotButton.Visibility = ViewStates.Gone;
            _flashButton.Enabled = false;
            _galleryButton.Enabled = false;
            _revertButton.Enabled = false;
            _closeButton.Enabled = false;
        }

        public void SwitchCamera(object sender, EventArgs e)
        {
            if (_camera != null)
            {
                _camera.StopPreview();
                _camera.Release();
                _camera = null;
            }

            var cameraToSwitch = _cameraId == 0 ? 1 : 0;
            EnableCamera(cameraToSwitch);
        }

        private void EnableCamera(int cameraToSwitch)
        {
            try
            {
                _camera = Camera.Open(cameraToSwitch);
                _cameraId = cameraToSwitch;
                SetPreviewSize(FullScreen);
                SetCameraDisplayOrientation(_cameraId);
                _camera.SetPreviewDisplay(_holder);
                _camera.StartPreview();
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Java.Lang.RuntimeException) && ex.Message == "Fail to connect to camera service")
                    Activity.Finish();
                else
                {
                    AppSettings.Reporter.SendCrash(ex);
                    Activity.ShowAlert(LocalizationKeys.CameraSettingError, ToastLength.Short);
                }
            }
        }

        private void GetGalleryIcon()
        {
            string[] projection = {
                MediaStore.Images.ImageColumns.Id,
                MediaStore.Images.ImageColumns.Data,
                MediaStore.Images.ImageColumns.BucketDisplayName,
                MediaStore.Images.ImageColumns.DateTaken,
                MediaStore.Images.ImageColumns.MimeType
            };
            var cursor = Context.ContentResolver
                .Query(MediaStore.Images.Media.ExternalContentUri, projection, null, null, MediaStore.Images.ImageColumns.DateTaken + " DESC");

            if (cursor.MoveToFirst())
            {
                var imageLocation = cursor.GetString(1);
                var imageFile = new Java.IO.File(imageLocation);
                if (imageFile.Exists())
                {
                    using (var fileDescriptor = Activity.ContentResolver.OpenFileDescriptor(imageFile.Path.ToUri(), "r").FileDescriptor)
                    {
                        var bitmap = BitmapUtils.DecodeSampledBitmapFromDescriptor(fileDescriptor, 300, 300);
                        bitmap = BitmapUtils.RotateImageIfRequired(bitmap, fileDescriptor, imageFile.Path);
                        _galleryIcon.SetImageBitmap(bitmap);
                    }
                }
            }
        }
    }
}
