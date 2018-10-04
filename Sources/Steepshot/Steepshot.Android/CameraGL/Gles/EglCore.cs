using Android.Graphics;
using Android.Opengl;
using Android.Util;
using Android.Views;
using Java.Lang;
using Object = Java.Lang.Object;

namespace Steepshot.CameraGL.Gles
{
    public class EglCore
    {
        private static readonly string Tag = "EglCore";
        public static int FlagRecordable = 0x01;

        /**
         * Constructor flag: ask for GLES3, fall back to GLES2 if not available.  Without this
         * flag, GLES2 is used.
         */
        public static int FlagTryGles3 = 0x02;

        // Android-specific extension.
        private static int EGL_RECORDABLE_ANDROID = 0x3142;

        private EGLDisplay _mEglDisplay = EGL14.EglNoDisplay;
        private EGLContext _mEglContext = EGL14.EglNoContext;
        private EGLConfig _mEglConfig;
        private readonly int _mGlVersion = -1;

        /**
         * Prepares EGL display and context.
         * <p>
         * Equivalent to EglCore(null, 0).
         */
        public EglCore() : this(null, 0)
        {
        }


        // Prepares EGL display and context.
        public EglCore(EGLContext sharedContext, int flags)
        {
            if (_mEglDisplay != EGL14.EglNoDisplay)
            {
                throw new RuntimeException("EGL already set up");
            }

            if (sharedContext == null)
            {
                sharedContext = EGL14.EglNoContext;
            }

            _mEglDisplay = EGL14.EglGetDisplay(EGL14.EglDefaultDisplay);
            if (_mEglDisplay == EGL14.EglNoDisplay)
            {
                throw new RuntimeException("unable to get EGL14 display");
            }
            int[] version = new int[2];
            if (!EGL14.EglInitialize(_mEglDisplay, version, 0, version, 1))
            {
                _mEglDisplay = null;
                throw new RuntimeException("unable to initialize EGL14");
            }

            // Try to get a GLES3 context, if requested.
            if ((flags & FlagTryGles3) != 0)
            {
                //Log.d(TAG, "Trying GLES 3");
                EGLConfig config = GetConfig(flags, 3);
                if (config != null)
                {
                    int[] attrib3List = {
                        EGL14.EglContextClientVersion, 3,
                        EGL14.EglNone
                };
                    EGLContext context = EGL14.EglCreateContext(_mEglDisplay, config, sharedContext,
                            attrib3List, 0);

                    if (EGL14.EglGetError() == EGL14.EglSuccess)
                    {
                        //Log.d(TAG, "Got GLES 3 config");
                        _mEglConfig = config;
                        _mEglContext = context;
                        _mGlVersion = 3;
                    }
                }
            }
            if (_mEglContext == EGL14.EglNoContext)
            {  // GLES 2 only, or GLES 3 attempt failed
               //Log.d(TAG, "Trying GLES 2");
                EGLConfig config = GetConfig(flags, 2);
                if (config == null)
                {
                    throw new RuntimeException("Unable to find a suitable EGLConfig");
                }
                int[] attrib2List = {
                    EGL14.EglContextClientVersion, 2,
                    EGL14.EglNone
            };
                EGLContext context = EGL14.EglCreateContext(_mEglDisplay, config, sharedContext,
                        attrib2List, 0);
                checkEglError("eglCreateContext");
                _mEglConfig = config;
                _mEglContext = context;
                _mGlVersion = 2;
            }

            // Confirm with query.
            int[] values = new int[1];
            EGL14.EglQueryContext(_mEglDisplay, _mEglContext, EGL14.EglContextClientVersion,
                    values, 0);
            Log.Debug(Tag, "EGLContext created, client version " + values[0]);
        }

        /**
         * Finds a suitable EGLConfig.
         *
         * @param flags Bit flags from constructor.
         * @param version Must be 2 or 3.
         */
        private EGLConfig GetConfig(int flags, int version)
        {
            int renderableType = EGL14.EglOpenglEs2Bit;
            if (version >= 3)
            {
                renderableType |= EGLExt.EglOpenglEs3BitKhr;
            }

            // The actual surface is generally RGBA or RGBX, so situationally omitting alpha
            // doesn't really help.  It can also lead to a huge performance hit on glReadPixels()
            // when reading into a GL_RGBA buffer.
            int[] attribList = {
                EGL14.EglRedSize, 8,
                EGL14.EglGreenSize, 8,
                EGL14.EglBlueSize, 8,
                EGL14.EglAlphaSize, 8,
                //EGL14.EGL_DEPTH_SIZE, 16,
                //EGL14.EGL_STENCIL_SIZE, 8,
                EGL14.EglRenderableType, renderableType,
                EGL14.EglNone, 0,      // placeholder for recordable [@-3]
                EGL14.EglNone
        };
            if ((flags & FlagRecordable) != 0)
            {
                attribList[attribList.Length - 3] = EGL_RECORDABLE_ANDROID;
                attribList[attribList.Length - 2] = 1;
            }
            EGLConfig[] configs = new EGLConfig[1];
            int[] numConfigs = new int[1];
            if (!EGL14.EglChooseConfig(_mEglDisplay, attribList, 0, configs, 0, configs.Length,
                    numConfigs, 0))
            {
                Log.Warn(Tag, "unable to find RGB8888 / " + version + " EGLConfig");
                return null;
            }
            return configs[0];
        }

        /**
         * Discards all resources held by this class, notably the EGL context.  This must be
         * called from the thread where the context was created.
         * <p>
         * On completion, no context will be current.
         */
        public void Release()
        {
            if (_mEglDisplay != EGL14.EglNoDisplay)
            {
                // Android is unusual in that it uses a reference-counted EGLDisplay.  So for
                // every eglInitialize() we need an eglTerminate().
                EGL14.EglMakeCurrent(_mEglDisplay, EGL14.EglNoSurface, EGL14.EglNoSurface,
                        EGL14.EglNoContext);
                EGL14.EglDestroyContext(_mEglDisplay, _mEglContext);
                EGL14.EglReleaseThread();
                EGL14.EglTerminate(_mEglDisplay);
            }

            _mEglDisplay = EGL14.EglNoDisplay;
            _mEglContext = EGL14.EglNoContext;
            _mEglConfig = null;
        }

        ~EglCore()
        {
            if (_mEglDisplay != EGL14.EglNoDisplay)
            {
                // We're limited here -- finalizers don't run on the thread that holds
                // the EGL state, so if a surface or context is still current on another
                // thread we can't fully release it here.  Exceptions thrown from here
                // are quietly discarded.  Complain in the log file.
                Log.Warn(Tag, "WARNING: EglCore was not explicitly released -- state may be leaked");
                Release();
            }
        }


        /**
         * Destroys the specified surface.  Note the EGLSurface won't actually be destroyed if it's
         * still current in a context.
         */
        public void ReleaseSurface(EGLSurface eglSurface)
        {
            EGL14.EglDestroySurface(_mEglDisplay, eglSurface);
        }

        /**
         * Creates an EGL surface associated with a Surface.
         * <p>
         * If this is destined for MediaCodec, the EGLConfig should have the "recordable" attribute.
         */
        public EGLSurface CreateWindowSurface(Object surface)
        {
            if (!(surface is Surface) && !(surface is SurfaceTexture))
            {
                throw new RuntimeException("invalid surface: " + surface);
            }

            // Create a window surface, and attach it to the Surface we received.
            int[] surfaceAttribs = {
                EGL14.EglNone
        };
            EGLSurface eglSurface = EGL14.EglCreateWindowSurface(_mEglDisplay, _mEglConfig, surface,
                    surfaceAttribs, 0);
            checkEglError("eglCreateWindowSurface");
            if (eglSurface == null)
            {
                throw new RuntimeException("surface was null");
            }
            return eglSurface;
        }

        /**
         * Creates an EGL surface associated with an offscreen buffer.
         */
        public EGLSurface CreateOffscreenSurface(int width, int height)
        {
            int[] surfaceAttribs = {
                EGL14.EglWidth, width,
                EGL14.EglHeight, height,
                EGL14.EglNone
        };
            EGLSurface eglSurface = EGL14.EglCreatePbufferSurface(_mEglDisplay, _mEglConfig,
                    surfaceAttribs, 0);
            checkEglError("eglCreatePbufferSurface");
            if (eglSurface == null)
            {
                throw new RuntimeException("surface was null");
            }
            return eglSurface;
        }

        /**
         * Makes our EGL context current, using the supplied surface for both "draw" and "read".
         */
        public void MakeCurrent(EGLSurface eglSurface)
        {
            if (_mEglDisplay == EGL14.EglNoDisplay)
            {
                // called makeCurrent() before create?
                Log.Debug(Tag, "NOTE: makeCurrent w/o display");
            }
            if (!EGL14.EglMakeCurrent(_mEglDisplay, eglSurface, eglSurface, _mEglContext))
            {
                throw new RuntimeException("eglMakeCurrent failed");
            }
        }

        /**
         * Makes our EGL context current, using the supplied "draw" and "read" surfaces.
         */
        public void MakeCurrent(EGLSurface drawSurface, EGLSurface readSurface)
        {
            if (_mEglDisplay == EGL14.EglNoDisplay)
            {
                // called makeCurrent() before create?
                Log.Debug(Tag, "NOTE: makeCurrent w/o display");
            }
            if (!EGL14.EglMakeCurrent(_mEglDisplay, drawSurface, readSurface, _mEglContext))
            {
                throw new RuntimeException("eglMakeCurrent(draw,read) failed");
            }
        }

        /**
         * Makes no context current.
         */
        public void MakeNothingCurrent()
        {
            if (!EGL14.EglMakeCurrent(_mEglDisplay, EGL14.EglNoSurface, EGL14.EglNoSurface,
                    EGL14.EglNoContext))
            {
                throw new RuntimeException("eglMakeCurrent failed");
            }
        }

        /**
         * Calls eglSwapBuffers.  Use this to "publish" the current frame.
         *
         * @return false on failure
         */
        public bool SwapBuffers(EGLSurface eglSurface)
        {
            return EGL14.EglSwapBuffers(_mEglDisplay, eglSurface);
        }

        /**
         * Sends the presentation time stamp to EGL.  Time is expressed in nanoseconds.
         */
        public void SetPresentationTime(EGLSurface eglSurface, long nsecs)
        {
            EGLExt.EglPresentationTimeANDROID(_mEglDisplay, eglSurface, nsecs);
        }

        /**
         * Returns true if our context and the specified surface are current.
         */
        public bool IsCurrent(EGLSurface eglSurface)
        {
            return _mEglContext.Equals(EGL14.EglGetCurrentContext()) &&
                eglSurface.Equals(EGL14.EglGetCurrentSurface(EGL14.EglDraw));
        }

        /**
         * Performs a simple surface query.
         */
        public int QuerySurface(EGLSurface eglSurface, int what)
        {
            int[] value = new int[1];
            EGL14.EglQuerySurface(_mEglDisplay, eglSurface, what, value, 0);
            return value[0];
        }

        /**
         * Queries a string value.
         */
        public string QueryString(int what)
        {
            return EGL14.EglQueryString(_mEglDisplay, what);
        }

        /**
         * Returns the GLES version this context is configured for (currently 2 or 3).
         */
        public int GetGlVersion()
        {
            return _mGlVersion;
        }

        /**
         * Writes the current display, context, and surface to the log.
         */
        public static void LogCurrent(string msg)
        {
            var display = EGL14.EglGetCurrentDisplay();
            var context = EGL14.EglGetCurrentContext();
            var surface = EGL14.EglGetCurrentSurface(EGL14.EglDraw);
            Log.Info(Tag, "Current EGL (" + msg + "): display=" + display + ", context=" + context +
                    ", surface=" + surface);
        }

        /**
         * Checks for EGL errors.  Throws an exception if an error has been raised.
         */
        private void checkEglError(string msg)
        {
            int error;
            if ((error = EGL14.EglGetError()) != EGL14.EglSuccess)
            {
                throw new RuntimeException(msg + ": EGL error: 0x" + Integer.ToHexString(error));
            }
        }
    }
}