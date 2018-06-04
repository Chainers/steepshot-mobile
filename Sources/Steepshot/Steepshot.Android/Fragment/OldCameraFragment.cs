using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Hardware;
using Android.Locations;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Refractored.Controls;
using Steepshot.Base;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Camera = Android.Hardware.Camera;
using Format = Android.Graphics.Format;

namespace Steepshot.Fragment
{
#pragma warning disable 0649, 4014, 0618
    public sealed class OldCameraFragment : BaseFragment, ISurfaceHolderCallback, Camera.IPictureCallback, Camera.IShutterCallback, ILocationListener
    {
        private Location _currentLocation;
        private LocationManager _locationManager;
        private bool isGpsEnable = false;

        private const bool FullScreen = true;
        private const int GalleryRequestCode = 228;

        private ISurfaceHolder _holder;
        private Camera _camera;
        private int _cameraId;
        private int _currentRotation;
        private int _rotationOnShutter;
        private float _dist;
        private CameraOrientationEventListener _orientationListner;

        [BindView(Resource.Id.surfaceView)] private SurfaceView _sv;
        [BindView(Resource.Id.flash_button)] private ImageButton _flashButton;
        [BindView(Resource.Id.gps_button)] private ImageButton _gpsButton;
        [BindView(Resource.Id.shot_button)] private ImageButton _shotButton;
        [BindView(Resource.Id.loading_spinner)] private ProgressBar _progressBar;
        [BindView(Resource.Id.revert_button)] private ImageButton _revertButton;
        [BindView(Resource.Id.close_button)] private ImageButton _closeButton;
        [BindView(Resource.Id.gallery_button)] private RelativeLayout _galleryButton;
        [BindView(Resource.Id.gallery_icon)] private CircleImageView _galleryIcon;

#pragma warning restore 0649

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.lyt_old_camera, null);
            Cheeseknife.Bind(this, v);
            return v;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((BaseActivity)Activity).RequestPermissions(BaseActivity.CommonPermissionsRequestCode, Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation);

            if (Camera.NumberOfCameras < 2)
                _revertButton.Visibility = ViewStates.Gone;

            _flashButton.Click += FlashClick;
            _shotButton.Click += TakePhotoClick;
            _closeButton.Click += GoBack;
            _galleryButton.Click += OpenGallery;
            _revertButton.Click += SwitchCamera;
            _sv.Touch += SvOnTouch;
            _gpsButton.Click += GpsButtonOnClick;

            _orientationListner = new CameraOrientationEventListener(Activity, SensorDelay.Normal);
            _orientationListner.OrientationChanged += OnOrientationChanged;
            GetGalleryIcon();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == GalleryRequestCode && !grantResults.Any(x => x != Permission.Granted))
            {
                _locationManager = (LocationManager)Context.GetSystemService(Context.LocationService);
                var criteriaForLocationService = new Criteria { Accuracy = Accuracy.NoRequirement };
                _locationManager.RequestLocationUpdates(0, 0, criteriaForLocationService, this, null);
                GpsButtonOnClick(null, null);
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void GpsButtonOnClick(object sender, EventArgs eventArgs)
        {
            _gpsButton.Enabled = false;

            if (!((BaseActivity)Activity).RequestPermissions(BaseActivity.CommonPermissionsRequestCode, Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation))
            {
                isGpsEnable = !isGpsEnable;
                _gpsButton.SetImageResource(isGpsEnable ? Resource.Drawable.ic_gps : Resource.Drawable.ic_gps_n);
            }

            _gpsButton.Enabled = true;
        }

        private void SvOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            _sv.Enabled = false;

            var cameraParams = _camera.GetParameters();
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

            _sv.Enabled = true;
        }

        private void HandleZoom(MotionEvent e, Camera.Parameters p)
        {
            var maxZoom = p.MaxZoom;
            var zoom = p.Zoom;
            var newDist = GetFingerSpacing(e);
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
            var x = e.GetX(0) - e.GetX(1);
            var y = e.GetY(0) - e.GetY(1);
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
            _flashButton.Enabled = false;

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

            _flashButton.Enabled = true;
        }

        private void TakePhotoClick(object sender, EventArgs e)
        {
            _shotButton.Enabled = false;

            try
            {
                var parameters = _camera.GetParameters();
                AddGps(parameters);
                parameters.SetRotation(_currentRotation);
                _camera.SetParameters(parameters);
                _camera?.TakePicture(this, null, this);
            }
            catch
            {
                _shotButton.Enabled = true;
            }
        }

        private void AddGps(Camera.Parameters parameters)
        {
            if (isGpsEnable && _currentLocation != null)
            {
                parameters.RemoveGpsData();
                parameters.SetGpsLatitude(_currentLocation.Latitude);
                parameters.SetGpsLongitude(_currentLocation.Longitude);
                parameters.SetGpsAltitude(_currentLocation.Altitude);
                parameters.SetGpsTimestamp(_currentLocation.Time);
                parameters.SetGpsProcessingMethod(_currentLocation.Provider);
            }
        }

        private void GoBack(object sender, EventArgs e)
        {
            _closeButton.Enabled = false;

            Activity.OnBackPressed();

            _closeButton.Enabled = true;
        }

        private void OpenGallery(object sender, EventArgs e)
        {
            _galleryButton.Enabled = false;

            ((BaseActivity)Activity).OpenNewContentFragment(new GalleryFragment());

            _galleryButton.Enabled = true;
        }

        private void OnOrientationChanged(int orientation)
        {
            if (_camera == null)
                return;

            var info = new Camera.CameraInfo();
            Camera.GetCameraInfo(_cameraId, info);

            orientation = (orientation + 45) / 90 * 90;
            var rotation = info.Facing == Camera.CameraInfo.CameraFacingFront
                ? (info.Orientation - orientation + 360) % 360
                : (info.Orientation + orientation) % 360;

            _currentRotation = rotation;
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

                var model = new GalleryMediaModel
                {
                    Path = photoUri
                };

                Activity.RunOnUiThread(() =>
                {
                    ((BaseActivity)Activity).OpenNewContentFragment(new PostCreateFragment(model));
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
            _revertButton.Enabled = false;

            if (_camera != null)
            {
                _camera.StopPreview();
                _camera.Release();
                _camera = null;
            }

            var cameraToSwitch = _cameraId == 0 ? 1 : 0;
            EnableCamera(cameraToSwitch);

            _revertButton.Enabled = true;
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
                MediaStore.Images.ImageColumns.Data
            };
            var cursor = Context.ContentResolver
                .Query(MediaStore.Images.Media.ExternalContentUri, projection, null, null, MediaStore.Images.ImageColumns.DateTaken + " DESC");

            if (cursor != null && cursor.Count > 0)
            {
                var idInd = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Id);
                var pathInd = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data);

                while (cursor.MoveToNext())
                {
                    var imageId = cursor.GetLong(idInd);
                    var imageLocation = cursor.GetString(pathInd);
                    var imageFile = new Java.IO.File(imageLocation);
                    if (imageFile.Exists())
                    {
                        using (var bitmap = MediaStore.Images.Thumbnails.GetThumbnail(Context.ContentResolver, imageId,
                            ThumbnailKind.MicroKind, null))
                        {
                            _galleryIcon.SetImageBitmap(bitmap);
                        }
                        break;
                    }
                }
            }
            else
            {
                _galleryIcon.SetImageDrawable(new ColorDrawable(Style.R245G245B245));
            }
        }



        public void OnLocationChanged(Location location)
        {
            if (_currentLocation == null)
            {
                _gpsButton.Visibility = ViewStates.Visible;
                _gpsButton.SetImageResource(Resource.Drawable.ic_gps);
            }
            if (_currentLocation == null || _currentLocation.Accuracy > location.Accuracy)
                _currentLocation = location;
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
        }
    }
}
