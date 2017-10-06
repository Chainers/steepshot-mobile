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
using Refractored.Controls;
using Steepshot.Activity;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Camera = Android.Hardware.Camera;

namespace Steepshot.Fragment
{
#pragma warning disable 0649, 4014, 0618
    public class OldCameraFragment : BaseFragment, ISurfaceHolderCallback, Camera.IPictureCallback, Camera.IShutterCallback
    {
        private ISurfaceHolder _holder;
        private Camera _camera;
        private int _cameraId = 0;
        private const bool _fullScreen = true;
        private const int galleryRequestCode = 228;
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
            _orientationListner = new CameraOrientationEventListener(Activity, SensorDelay.Normal);
            _orientationListner.OrientationChanged += (orientation) =>
            {
                var parameters = _camera?.GetParameters();
                parameters?.SetRotation(_cameraId != 0 && orientation == 90 ? 270 : orientation);
                _camera?.SetParameters(parameters);
            };
            GetGalleryIcon();
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
            if (_camera != null)
                _camera.Release();
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

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == -1 && requestCode == galleryRequestCode)
            {
                var i = new Intent(Context, typeof(PostDescriptionActivity));
                i.PutExtra("FILEPATH", GetPathToImage(data.Data));
                StartActivity(i);
            }
        }

        [InjectOnClick(Resource.Id.flash_button)]
        private void FlashClick(object sender, EventArgs e)
        {
            var parameters = _camera.GetParameters();
            if (parameters.SupportedFlashModes != null && parameters.SupportedFlashModes.Contains(Camera.Parameters.FlashModeOff) && parameters.SupportedFlashModes.Contains(Camera.Parameters.FlashModeOn))
            {
                parameters.FlashMode = parameters.FlashMode != Camera.Parameters.FlashModeOff ? Camera.Parameters.FlashModeOff : Camera.Parameters.FlashModeOn;
                _camera.SetParameters(parameters);
            }
        }

        [InjectOnClick(Resource.Id.shot_button)]
        private void TakePhotoClick(object sender, EventArgs e)
        {
            _camera?.TakePicture(this, null, this);
        }

        [InjectOnClick(Resource.Id.close_button)]
        private void GoBack(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

        [InjectOnClick(Resource.Id.gallery_button)]
        private void OpenGallery(object sender, EventArgs e)
        {
            Intent intent = new Intent();
            intent.SetAction(Intent.ActionGetContent);
            intent.SetType("image/*");
            StartActivityForResult(intent, galleryRequestCode);
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
            }
        }

        private void SetPreviewSize(bool fullScreen)
        {
            Display display = Activity.WindowManager.DefaultDisplay;
            bool widthIsMax = display.Width > display.Height;

            var parameters = _camera.GetParameters();

            if (parameters.SupportedFlashModes == null || parameters.SupportedFlashModes.Count() == 1)
                _flashButton.Visibility = ViewStates.Gone;
            else
                _flashButton.Visibility = ViewStates.Visible;

            var cameraSizes = GetSizes(parameters.SupportedPreviewSizes, parameters.SupportedPictureSizes);
            parameters.SetPictureSize(cameraSizes.Item2.Width, cameraSizes.Item2.Height);
            parameters.SetPreviewSize(cameraSizes.Item1.Width, cameraSizes.Item1.Height);
            _camera.SetParameters(parameters);
            var size = _camera.GetParameters().PreviewSize;

            RectF rectDisplay = new RectF();
            RectF rectPreview = new RectF();

            rectDisplay.Set(0, 0, display.Width, display.Height);

            if (widthIsMax)
                rectPreview.Set(0, 0, size.Width, size.Height);
            else
                rectPreview.Set(0, 0, size.Height, size.Width);

            Matrix matrix = new Matrix();
            if (!fullScreen)
                matrix.SetRectToRect(rectPreview, rectDisplay, Matrix.ScaleToFit.Start);
            else
            {
                matrix.SetRectToRect(rectDisplay, rectPreview, Matrix.ScaleToFit.Start);
                matrix.Invert(matrix);
            }
            matrix.MapRect(rectPreview);
            _sv.LayoutParameters.Height = (int)(rectPreview.Bottom);
            _sv.LayoutParameters.Width = (int)(rectPreview.Right);
        }

        private Tuple<Camera.Size, Camera.Size> GetSizes(IList<Camera.Size> supportedPreviewSizes, IList<Camera.Size> supportedPictureSizes)
        {
            var previewSizes = supportedPreviewSizes.OrderByDescending((arg) => arg.Width).ToList();
            var pictureSizes = supportedPictureSizes.OrderByDescending((arg) => arg.Width).ToList();

            Tuple<Camera.Size, Camera.Size> rez = null;
            int difference = int.MaxValue;

            foreach (var previewSize in previewSizes)
            {
                var previewCoeff = (double)previewSize.Height / (double)previewSize.Width;

                foreach (var pictureSize in pictureSizes)
                {
                    var picCoeff = (double)pictureSize.Height / (double)pictureSize.Width;
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
            int degrees = 0;
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

            int result = 0;
            Camera.CameraInfo info = new Camera.CameraInfo();
            Camera.GetCameraInfo(cameraId, info);
            if (info.Facing == CameraFacing.Back)
            {
                result = ((360 - degrees) + info.Orientation);
            }
            else if (info.Facing == CameraFacing.Front)
            {
                result = ((360 - degrees) - info.Orientation);
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
            parameters?.SetRotation(_cameraId != 0 && _orientationListner.Orientation == 90 ? 270 : _orientationListner.Orientation);
            _camera.SetParameters(parameters);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {

        }

        public void OnPictureTaken(byte[] data, Camera camera)
        {
            Task.Run(() =>
            {
                var directoryPictures = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
                var directory = new Java.IO.File(directoryPictures, Constants.Steepshot);
                if (!directory.Exists())
                    directory.Mkdirs();

                var _photoUri = $"{directory}/{Guid.NewGuid()}.jpeg";

                var stream = new Java.IO.FileOutputStream(_photoUri);
                stream.Write(data);
                stream.Close();

                var i = new Intent(Context, typeof(PostDescriptionActivity));
                i.PutExtra("FILEPATH", _photoUri);
                i.PutExtra("SHOULD_COMPRESS", false);

                Activity.RunOnUiThread(() =>
                {
                    StartActivity(i);
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

        private string GetPathToImage(Android.Net.Uri uri)
        {
            string doc_id = "";
            using (var c1 = Activity.ContentResolver.Query(uri, null, null, null, null))
            {
                c1.MoveToFirst();
                String document_id = c1.GetString(0);
                doc_id = document_id.Substring(document_id.LastIndexOf(":") + 1);
            }
            string path = null;
            string selection = MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
            using (var cursor = Activity.ManagedQuery(MediaStore.Images.Media.ExternalContentUri, null, selection, new string[] { doc_id }, null))
            {
                if (cursor == null) return path;
                var columnIndex = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.Data);
                cursor.MoveToFirst();
                path = cursor.GetString(columnIndex);
            }
            return path;
        }

        public void OnShutter()
        {
            _progressBar.Visibility = ViewStates.Visible;
            _shotButton.Visibility = ViewStates.Gone;
            _flashButton.Enabled = false;
            _galleryButton.Enabled = false;
            _revertButton.Enabled = false;
            _closeButton.Enabled = false;
        }

        [InjectOnClick(Resource.Id.revert_button)]
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
            _camera = Camera.Open(cameraToSwitch);
            _cameraId = cameraToSwitch;
            SetPreviewSize(_fullScreen);
            SetCameraDisplayOrientation(_cameraId);
            try
            {
                _camera.SetPreviewDisplay(_holder);
                _camera.StartPreview();
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
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
            var cursor = Context.ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, projection, null,
                                                       null, MediaStore.Images.ImageColumns.DateTaken + " DESC");

            if (cursor.MoveToFirst())
            {
                String imageLocation = cursor.GetString(1);
                var imageFile = new Java.IO.File(imageLocation);
                if (imageFile.Exists())
                {
                    var bitmap = BitmapUtils.DecodeSampledBitmapFromResource(imageFile.Path, 300, 300);
                    bitmap = BitmapUtils.RotateImageIfRequired(bitmap, imageFile.Path);
                    _galleryIcon.SetImageBitmap(bitmap);
                }
            }
        }
    }
}
