using Android.Graphics;
using Android.Opengl;
using Android.Util;
using Java.Lang;
using Java.Nio;
using Steepshot.Core.Models.Common;
using Steepshot.Utils;
using Matrix = Android.Opengl.Matrix;
using String = System.String;

namespace Steepshot.CameraGL.Gles
{
    public class Texture2DProgram
    {
        private static readonly String Tag = "Texture2DProgram";

        // Simple vertex shader, used for all programs.
        private static String VERTEX_SHADER =
            "uniform mat4 uMVPMatrix;\n" +
            "uniform mat4 uTexMatrix;\n" +
            "attribute vec4 aPosition;\n" +
            "attribute vec4 aTextureCoord;\n" +
            "varying vec2 vTextureCoord;\n" +
            "void main() {\n" +
            "    gl_Position = uMVPMatrix * aPosition;\n" +
            "    vTextureCoord = (uTexMatrix * aTextureCoord).xy;\n" +
            "}\n";

        // Simple fragment shader for use with external 2D textures (e.g. what we get from
        // SurfaceTexture).
        private static String FRAGMENT_SHADER_EXT =
                "#extension GL_OES_EGL_image_external : require\n" +
                "precision mediump float;\n" +
                "varying vec2 vTextureCoord;\n" +
                "uniform samplerExternalOES sTexture;\n" +
                "void main() {\n" +
                "    gl_FragColor = texture2D(sTexture, vTextureCoord);\n" +
                "}\n";

        // This is not optimized for performance.  Some things that might make this faster:
        // - Remove the conditionals.  They're used to present a half & half view with a red
        //   stripe across the middle, but that's only useful for a demo.
        // - Unroll the loop.  Ideally the compiler does this for you when it's beneficial.
        // - Bake the filter kernel into the shader, instead of passing it through a uniform
        //   array.  That, combined with loop unrolling, should reduce memory accesses.
        public static int KERNEL_SIZE = 9;
        public FrameSize InputSize { get; set; }
        public Rect ViewPort { get; set; }

        // Handles to the GL program and various components of it.
        private int _mProgramHandle;
        private readonly int _muMvpMatrixLoc;
        private readonly int _muTexMatrixLoc;
        private readonly int _muKernelLoc;
        private readonly int _muTexOffsetLoc;
        private readonly int _muColorAdjustLoc;
        private readonly int _maPositionLoc;
        private readonly int _maTextureCoordLoc;

        private readonly int _mTextureTarget;

        private readonly float[] _mKernel = new float[KERNEL_SIZE];
        private float[] _mTexOffset;
        private float _mColorAdjust;


        /**
         * Prepares the program in the current EGL context.
         */
        public Texture2DProgram()
        {
            _mTextureTarget = GLES11Ext.GlTextureExternalOes;
            _mProgramHandle = GlUtil.CreateProgram(VERTEX_SHADER, FRAGMENT_SHADER_EXT);

            if (_mProgramHandle == 0)
            {
                throw new RuntimeException("Unable to create program");
            }

            // get locations of attributes and uniforms

            _maPositionLoc = GLES20.GlGetAttribLocation(_mProgramHandle, "aPosition");
            GlUtil.CheckLocation(_maPositionLoc, "aPosition");
            _maTextureCoordLoc = GLES20.GlGetAttribLocation(_mProgramHandle, "aTextureCoord");
            GlUtil.CheckLocation(_maTextureCoordLoc, "aTextureCoord");
            _muMvpMatrixLoc = GLES20.GlGetUniformLocation(_mProgramHandle, "uMVPMatrix");
            GlUtil.CheckLocation(_muMvpMatrixLoc, "uMVPMatrix");
            _muTexMatrixLoc = GLES20.GlGetUniformLocation(_mProgramHandle, "uTexMatrix");
            GlUtil.CheckLocation(_muTexMatrixLoc, "uTexMatrix");
            _muKernelLoc = GLES20.GlGetUniformLocation(_mProgramHandle, "uKernel");
            if (_muKernelLoc < 0)
            {
                // no kernel in this one
                _muKernelLoc = -1;
                _muTexOffsetLoc = -1;
                _muColorAdjustLoc = -1;
            }
            else
            {
                // has kernel, must also have tex offset and color adj
                _muTexOffsetLoc = GLES20.GlGetUniformLocation(_mProgramHandle, "uTexOffset");
                GlUtil.CheckLocation(_muTexOffsetLoc, "uTexOffset");
                _muColorAdjustLoc = GLES20.GlGetUniformLocation(_mProgramHandle, "uColorAdjust");
                GlUtil.CheckLocation(_muColorAdjustLoc, "uColorAdjust");

                // initialize default values
                SetKernel(new[] { 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f }, 0f);
                SetTexSize(256, 256);
            }
        }

        /**
         * Releases the program.
         * <p>
         * The appropriate EGL context must be current (i.e. the one that was used to create
         * the program).
         */
        public void Release()
        {
            Log.Debug(Tag, "deleting program " + _mProgramHandle);
            GLES20.GlDeleteProgram(_mProgramHandle);
            _mProgramHandle = -1;
        }

        /**
         * Creates a texture object suitable for use with this program.
         * <p>
         * On exit, the texture will be bound.
         */
        public int CreateTextureObject()
        {
            int[] textures = new int[1];
            GLES20.GlGenTextures(1, textures, 0);
            GlUtil.CheckGlError("glGenTextures");

            int texId = textures[0];
            GLES20.GlBindTexture(_mTextureTarget, texId);
            GlUtil.CheckGlError("glBindTexture " + texId);

            GLES20.GlTexParameterf(GLES11Ext.GlTextureExternalOes, GLES20.GlTextureMinFilter,
                    GLES20.GlNearest);
            GLES20.GlTexParameterf(GLES11Ext.GlTextureExternalOes, GLES20.GlTextureMagFilter,
                    GLES20.GlLinear);
            GLES20.GlTexParameteri(GLES11Ext.GlTextureExternalOes, GLES20.GlTextureWrapS,
                    GLES20.GlClampToEdge);
            GLES20.GlTexParameteri(GLES11Ext.GlTextureExternalOes, GLES20.GlTextureWrapT,
                    GLES20.GlClampToEdge);
            GlUtil.CheckGlError("glTexParameter");

            return texId;
        }

        /**
         * Configures the convolution filter values.
         *
         * @param values Normalized filter values; must be KERNEL_SIZE elements.
         */
        public void SetKernel(float[] values, float colorAdj)
        {
            if (values.Length != KERNEL_SIZE)
            {
                throw new IllegalArgumentException("Kernel size is " + values.Length +
                        " vs. " + KERNEL_SIZE);
            }
            System.Array.Copy(values, 0, _mKernel, 0, KERNEL_SIZE);
            _mColorAdjust = colorAdj;
            //Log.d(TAG, "filt kernel: " + Arrays.toString(mKernel) + ", adj=" + colorAdj);
        }

        /**
         * Sets the size of the texture.  This is used to find adjacent texels when filtering.
         */
        public void SetTexSize(int width, int height)
        {
            float rw = 1.0f / width;
            float rh = 1.0f / height;

            // Don't need to create a new array here, but it's syntactically convenient.
            _mTexOffset = new[] {
            -rw, -rh,   0f, -rh,    rw, -rh,
            -rw, 0f,    0f, 0f,     rw, 0f,
            -rw, rh,    0f, rh,     rw, rh
        };
            //Log.d(TAG, "filt size: " + width + "x" + height + ": " + Arrays.toString(mTexOffset));
        }

        /**
         * Issues the draw call.  Does the full setup on every call.
         *
         * @param mvpMatrix The 4x4 projection matrix.
         * @param vertexBuffer Buffer with vertex position data.
         * @param firstVertex Index of first vertex to use in vertexBuffer.
         * @param vertexCount Number of vertices in vertexBuffer.
         * @param coordsPerVertex The number of coordinates per vertex (e.g. x,y is 2).
         * @param vertexStride Width, in bytes, of the position data for each vertex (often
         *        vertexCount * sizeof(float)).
         * @param texMatrix A 4x4 transformation matrix for texture coords.  (Primarily intended
         *        for use with SurfaceTexture.)
         * @param texBuffer Buffer with vertex texture data.
         * @param texStride Width, in bytes, of the texture data for each vertex.
         */
        public void Draw(float[] mvpMatrix, FloatBuffer vertexBuffer, int firstVertex,
                int vertexCount, int coordsPerVertex, int vertexStride,
                float[] texMatrix, FloatBuffer texBuffer, int textureId, int texStride)
        {
            GlUtil.CheckGlError("draw start");

            // Select the program.
            GLES20.GlUseProgram(_mProgramHandle);
            GlUtil.CheckGlError("glUseProgram");

            // Set the texture.
            GLES20.GlActiveTexture(GLES20.GlTexture0);
            GLES20.GlBindTexture(_mTextureTarget, textureId);

            if (InputSize != null)
            {
                if (InputSize.Width > InputSize.Height)
                {
                    Matrix.ScaleM(mvpMatrix, 0, InputSize.Width / (float)InputSize.Height, 1f, 1f);
                    if (ViewPort != null)
                    {
                        var multiplier = 1f - InputSize.Height / (float)InputSize.Width;
                        Matrix.TranslateM(mvpMatrix, 0, 2f * multiplier * ViewPort.Left / ViewPort.Width() + multiplier, 0f, 0f);
                    }
                }
                else
                {
                    if (ViewPort == null)
                        Matrix.TranslateM(mvpMatrix, 0, 0f, (Style.ScreenHeight - Style.ScreenWidth) / (2f * Style.ScreenHeight) - 1, 0f);
                    Matrix.ScaleM(mvpMatrix, 0, 1f, InputSize.Height / (float)InputSize.Width, 1f);
                    if (ViewPort != null)
                    {
                        var multiplier = 1f - InputSize.Width / (float)InputSize.Height;
                        Matrix.TranslateM(mvpMatrix, 0, 0f, -2f * multiplier * ViewPort.Top / ViewPort.Height() - multiplier, 0f);
                    }
                }
            }

            GLES20.GlUniformMatrix4fv(_muMvpMatrixLoc, 1, false, mvpMatrix, 0);

            // Copy the texture transformation matrix over.
            GLES20.GlUniformMatrix4fv(_muTexMatrixLoc, 1, false, texMatrix, 0);
            GlUtil.CheckGlError("glUniformMatrix4fv");

            // Enable the "aPosition" vertex attribute.
            GLES20.GlEnableVertexAttribArray(_maPositionLoc);
            GlUtil.CheckGlError("glEnableVertexAttribArray");

            // Connect vertexBuffer to "aPosition".
            GLES20.GlVertexAttribPointer(_maPositionLoc, coordsPerVertex,
                GLES20.GlFloat, false, vertexStride, vertexBuffer);
            GlUtil.CheckGlError("glVertexAttribPointer");

            // Enable the "aTextureCoord" vertex attribute.
            GLES20.GlEnableVertexAttribArray(_maTextureCoordLoc);
            GlUtil.CheckGlError("glEnableVertexAttribArray");

            // Connect texBuffer to "aTextureCoord".
            GLES20.GlVertexAttribPointer(_maTextureCoordLoc, 2,
                    GLES20.GlFloat, false, texStride, texBuffer);
            GlUtil.CheckGlError("glVertexAttribPointer");

            // Populate the convolution kernel, if present.
            if (_muKernelLoc >= 0)
            {
                GLES20.GlUniform1fv(_muKernelLoc, KERNEL_SIZE, _mKernel, 0);
                GLES20.GlUniform2fv(_muTexOffsetLoc, KERNEL_SIZE, _mTexOffset, 0);
                GLES20.GlUniform1f(_muColorAdjustLoc, _mColorAdjust);
            }

            // Draw the rect.
            GLES20.GlDrawArrays(GLES20.GlTriangleStrip, firstVertex, vertexCount);
            GlUtil.CheckGlError("glDrawArrays");

            // Done -- disable vertex array, texture, and program.
            GLES20.GlDisableVertexAttribArray(_maPositionLoc);
            GLES20.GlDisableVertexAttribArray(_maTextureCoordLoc);
            GLES20.GlBindTexture(_mTextureTarget, 0);
            GLES20.GlUseProgram(0);
        }
    }
}