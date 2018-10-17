using System.IO;
using Android.Graphics;
using Android.Opengl;
using Android.Util;
using Java.Lang;
using Java.Nio;
using Matrix = Android.Graphics.Matrix;
using String = System.String;

namespace Steepshot.CameraGL.Gles
{
    public class EglSurfaceBase
    {
        private static readonly String Tag = "EglSurfaceBase";

        // EglCore object we're associated with.  It may be associated with multiple surfaces.
        protected EglCore MEglCore;

        private EGLSurface _mEglSurface = EGL14.EglNoSurface;
        private int _mWidth = -1;
        private int _mHeight = -1;

        protected EglSurfaceBase(EglCore eglCore)
        {
            MEglCore = eglCore;
        }

        /**
         * Creates a window surface.
         * <p>
         * @param surface May be a Surface or SurfaceTexture.
         */
        public void CreateWindowSurface(Object surface)
        {
            if (_mEglSurface != EGL14.EglNoSurface)
            {
                throw new IllegalStateException("surface already created");
            }
            _mEglSurface = MEglCore.CreateWindowSurface(surface);

            // Don't cache width/height here, because the size of the underlying surface can change
            // out from under us (see e.g. HardwareScalerActivity).
            //mWidth = mEglCore.querySurface(mEGLSurface, EGL14.EGL_WIDTH);
            //mHeight = mEglCore.querySurface(mEGLSurface, EGL14.EGL_HEIGHT);
        }

        /**
         * Creates an off-screen surface.
         */
        public void CreateOffscreenSurface(int width, int height)
        {
            if (_mEglSurface != EGL14.EglNoSurface)
            {
                throw new IllegalStateException("surface already created");
            }
            _mEglSurface = MEglCore.CreateOffscreenSurface(width, height);
            _mWidth = width;
            _mHeight = height;
        }

        /**
         * Returns the surface's width, in pixels.
         * <p>
         * If this is called on a window surface, and the underlying surface is in the process
         * of changing size, we may not see the new size right away (e.g. in the "surfaceChanged"
         * callback).  The size should match after the next buffer swap.
         */
        public int GetWidth()
        {
            if (_mWidth < 0)
            {
                return MEglCore.QuerySurface(_mEglSurface, EGL14.EglWidth);
            }

            return _mWidth;
        }

        /**
         * Returns the surface's height, in pixels.
         */
        public int GetHeight()
        {
            if (_mHeight < 0)
            {
                return MEglCore.QuerySurface(_mEglSurface, EGL14.EglHeight);
            }

            return _mHeight;
        }

        /**
         * Release the EGL surface.
         */
        public void ReleaseEglSurface()
        {
            MEglCore.ReleaseSurface(_mEglSurface);
            _mEglSurface = EGL14.EglNoSurface;
            _mWidth = _mHeight = -1;
        }

        /**
         * Makes our EGL context and surface current.
         */
        public void MakeCurrent()
        {
            MEglCore.MakeCurrent(_mEglSurface);
        }

        /**
         * Makes our EGL context and surface current for drawing, using the supplied surface
         * for reading.
         */
        public void MakeCurrentReadFrom(EglSurfaceBase readSurface)
        {
            MEglCore.MakeCurrent(_mEglSurface, readSurface._mEglSurface);
        }

        /**
         * Calls eglSwapBuffers.  Use this to "publish" the current frame.
         *
         * @return false on failure
         */
        public bool SwapBuffers()
        {
            bool result = MEglCore.SwapBuffers(_mEglSurface);
            if (!result)
            {
                Log.Debug(Tag, "WARNING: swapBuffers() failed");
            }
            return result;
        }

        /**
         * Sends the presentation time stamp to EGL.
         *
         * @param nsecs Timestamp, in nanoseconds.
         */
        public void SetPresentationTime(long nsecs)
        {
            MEglCore.SetPresentationTime(_mEglSurface, nsecs);
        }

        /**
         * Saves the EGL surface to a file.
         * <p>
         * Expects that this object's EGL surface is current.
         */
        public byte[] GetFrame()
        {
            if (!MEglCore.IsCurrent(_mEglSurface))
            {
                throw new RuntimeException("Expected EGL context/surface is not current");
            }

            // glReadPixels fills in a "direct" ByteBuffer with what is essentially big-endian RGBA
            // data (i.e. a byte of red, followed by a byte of green...).  While the Bitmap
            // constructor that takes an int[] wants little-endian ARGB (blue/red swapped), the
            // Bitmap "copy pixels" method wants the same format GL provides.
            //
            // Ideally we'd have some way to re-use the ByteBuffer, especially if we're calling
            // here often.
            //
            // Making this even more interesting is the upside-down nature of GL, which means
            // our output will look upside down relative to what appears on screen if the
            // typical GL conventions are used.            
            var width = GetWidth();
            var height = GetHeight();
            var bufferSize = width * height * 4;
            var buf = ByteBuffer.AllocateDirect(bufferSize);
            buf.Order(ByteOrder.LittleEndian);
            GLES20.GlReadPixels(0, 0, width, height, GLES20.GlRgba, GLES20.GlUnsignedByte, buf);
            GlUtil.CheckGlError("glReadPixels");
            buf.Rewind();

            byte[] data;
            using (var ms = new MemoryStream())
            {
                using (var tmpBmp = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888))
                {
                    tmpBmp.CopyPixelsFromBuffer(buf);
                    var transform = new Matrix();
                    transform.PreScale(1f, -1f);
                    using (var resultBmp = Bitmap.CreateBitmap(tmpBmp, 0, 0, width, height, transform, true))
                    {
                        resultBmp.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                        resultBmp.Recycle();
                    }
                    tmpBmp.Recycle();
                }

                data = ms.GetBuffer();
            }

            return data;
        }
    }
}