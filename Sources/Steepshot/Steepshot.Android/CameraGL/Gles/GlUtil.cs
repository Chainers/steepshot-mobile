using Android.Opengl;
using Java.Lang;
using Java.Nio;

namespace Steepshot.CameraGL.Gles
{
    public static class GlUtil
    {
        /** Identity matrix for general use.  Don't modify or life will get weird. */
        private static float[] _identityMatrix;

        public static float[] IdentityMatrix
        {
            get
            {
                _identityMatrix = new float[16];
                Matrix.SetIdentityM(_identityMatrix, 0);
                return _identityMatrix;
            }
        }

        private static int SIZEOF_FLOAT = 4;


        // do not instantiate

        /**
         * Creates a new program from the supplied vertex and fragment shaders.
         *
         * @return A handle to the program, or 0 on failure.
         */
        public static int CreateProgram(string vertexSource, string fragmentSource)
        {
            int vertexShader = LoadShader(GLES20.GlVertexShader, vertexSource);
            if (vertexShader == 0)
            {
                return 0;
            }
            int pixelShader = LoadShader(GLES20.GlFragmentShader, fragmentSource);
            if (pixelShader == 0)
            {
                return 0;
            }

            int program = GLES20.GlCreateProgram();
            CheckGlError("glCreateProgram");
            GLES20.GlAttachShader(program, vertexShader);
            CheckGlError("glAttachShader");
            GLES20.GlAttachShader(program, pixelShader);
            CheckGlError("glAttachShader");
            GLES20.GlLinkProgram(program);
            int[] linkStatus = new int[1];
            GLES20.GlGetProgramiv(program, GLES20.GlLinkStatus, linkStatus, 0);
            if (linkStatus[0] != GLES20.GlTrue)
            {
                GLES20.GlDeleteProgram(program);
                program = 0;
            }
            return program;
        }

        /**
         * Compiles the provided shader source.
         *
         * @return A handle to the shader, or 0 on failure.
         */
        public static int LoadShader(int shaderType, string source)
        {
            int shader = GLES20.GlCreateShader(shaderType);
            CheckGlError("glCreateShader type=" + shaderType);
            GLES20.GlShaderSource(shader, source);
            GLES20.GlCompileShader(shader);
            int[] compiled = new int[1];
            GLES20.GlGetShaderiv(shader, GLES20.GlCompileStatus, compiled, 0);
            if (compiled[0] == 0)
            {
                GLES20.GlDeleteShader(shader);
                shader = 0;
            }
            return shader;
        }

        /**
         * Checks to see if a GLES error has been raised.
         */
        public static void CheckGlError(string op)
        {
            int error = GLES20.GlGetError();
            if (error != GLES20.GlNoError)
            {
                var msg = op + ": glError 0x" + Integer.ToHexString(error);
                throw new RuntimeException(msg);
            }
        }

        /**
         * Checks to see if the location we obtained is valid.  GLES returns -1 if a label
         * could not be found, but does not set the GL error.
         * <p>
         * Throws a RuntimeException if the location is invalid.
         */
        public static void CheckLocation(int location, string label)
        {
            if (location < 0)
            {
                throw new RuntimeException("Unable to locate '" + label + "' in program");
            }
        }

        /**
         * Creates a texture from raw data.
         *
         * @param data Image data, in a "direct" ByteBuffer.
         * @param width Texture width, in pixels (not bytes).
         * @param height Texture height, in pixels.
         * @param format Image data format (use constant appropriate for glTexImage2D(), e.g. GL_RGBA).
         * @return Handle to texture.
         */
        public static int CreateImageTexture(ByteBuffer data, int width, int height, int format)
        {
            int[] textureHandles = new int[1];
            int textureHandle;

            GLES20.GlGenTextures(1, textureHandles, 0);
            textureHandle = textureHandles[0];
            CheckGlError("glGenTextures");

            // Bind the texture handle to the 2D texture target.
            GLES20.GlBindTexture(GLES20.GlTexture2d, textureHandle);

            // Configure min/mag filtering, i.e. what scaling method do we use if what we're rendering
            // is smaller or larger than the source image.
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMinFilter,
                    GLES20.GlLinear);
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMagFilter,
                    GLES20.GlLinear);
            CheckGlError("loadImageTexture");

            // Load the data from the buffer into the texture handle.
            GLES20.GlTexImage2D(GLES20.GlTexture2d, /*level*/ 0, format,
                    width, height, /*border*/ 0, format, GLES20.GlUnsignedByte, data);
            CheckGlError("loadImageTexture");

            return textureHandle;
        }

        /**
         * Allocates a direct float buffer, and populates it with the float array data.
         */
        public static FloatBuffer CreateFloatBuffer(float[] coords)
        {
            // Allocate a direct ByteBuffer, using 4 bytes per float, and copy coords into it.
            ByteBuffer bb = ByteBuffer.AllocateDirect(coords.Length * SIZEOF_FLOAT);
            bb.Order(ByteOrder.NativeOrder());
            FloatBuffer fb = bb.AsFloatBuffer();
            fb.Put(coords);
            fb.Position(0);
            return fb;
        }
    }
}