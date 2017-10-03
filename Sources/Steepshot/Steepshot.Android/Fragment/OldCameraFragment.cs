using System;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Java.IO;
using Steepshot.Base;
using Steepshot.Core.Utils;
using Camera = Android.Hardware.Camera;

namespace Steepshot.Fragment
{
#pragma warning disable 0649, 4014, 0618
    public class OldCameraFragment : BaseFragment, ISurfaceHolderCallback
    {
        private ISurfaceHolder _holder;
        private Camera _camera;
        private int _cameraId = 0;
        private const bool _fullScreen = true;
        [InjectView(Resource.Id.surfaceView)] private SurfaceView _sv;
        [InjectView(Resource.Id.close_button)] private ImageButton _closeButton;
        [InjectView(Resource.Id.flash_button)] private ImageButton _flashButton;
        [InjectView(Resource.Id.shot_button)] private ImageButton _shotButton;
        [InjectView(Resource.Id.revert_button)] private ImageButton _revertButton;
        [InjectView(Resource.Id.gallery_button)] private RelativeLayout _galleryButton;
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

            _holder = _sv.Holder;
            _holder.SetType(SurfaceType.PushBuffers);
            _holder.AddCallback(this);
        }

        public override void OnResume()
        {
            base.OnResume();
            _camera = Camera.Open(_cameraId);
            SetPreviewSize(_fullScreen);
        }

        public override void OnPause()
        {
            base.OnPause();
            if (_camera != null)
                _camera.Release();
            _camera = null;
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            _camera.StopPreview();
            SetCameraDisplayOrientation(_cameraId);
            try
            {
                _camera.SetPreviewDisplay(holder);
                _camera.StartPreview();
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            try
            {
                _camera.SetPreviewDisplay(holder);
                _camera.StartPreview();
            }
            catch (IOException ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
        }

        void SetPreviewSize(bool fullScreen)
        {
            Display display = Activity.WindowManager.DefaultDisplay;
            bool widthIsMax = display.Width > display.Height;
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

        void SetCameraDisplayOrientation(int cameraId)
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
            if (info.Facing == Android.Hardware.CameraFacing.Back)
            {
                result = ((360 - degrees) + info.Orientation);
            }
            else if (info.Facing == Android.Hardware.CameraFacing.Front)
            {
                result = ((360 - degrees) - info.Orientation);
                result += 360;
            }
            result = result % 360;
            _camera.SetDisplayOrientation(result);

            var parameters = _camera.GetParameters();
            if(parameters.SupportedFocusModes.Contains(Camera.Parameters.FocusModeContinuousVideo))
            {
                parameters.FocusMode = Camera.Parameters.FocusModeContinuousVideo;
                _camera.SetParameters(parameters);
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {

        }
    }
}
