namespace Steepshot.CameraGL.Gles
{
    public class FullFrameRect
    {
        private readonly Drawable2D _mRectDrawable = new Drawable2D(Drawable2D.Prefab.FullRectangle);
        private Texture2DProgram _mProgram;

        /**
         * Prepares the object.
         *
         * @param program The program to use.  FullFrameRect takes ownership, and will release
         *     the program when no longer needed.
         */
        public FullFrameRect(Texture2DProgram program)
        {
            _mProgram = program;
        }

        /**
         * Releases resources.
         * <p>
         * This must be called with the appropriate EGL context current (i.e. the one that was
         * current when the constructor was called).  If we're about to destroy the EGL context,
         * there's no value in having the caller make it current just to do this cleanup, so you
         * can pass a flag that will tell this function to skip any EGL-context-specific cleanup.
         */
        public void Release(bool doEglCleanup)
        {
            if (_mProgram != null)
            {
                if (doEglCleanup)
                {
                    _mProgram.Release();
                }
                _mProgram = null;
            }
        }

        /**
         * Returns the program currently in use.
         */
        public Texture2DProgram GetProgram()
        {
            return _mProgram;
        }

        /**
         * Creates a texture object suitable for use with drawFrame().
         */
        public int CreateTextureObject()
        {
            return _mProgram.CreateTextureObject();
        }

        /**
         * Draws a viewport-filling rect, texturing it with the specified texture object.
         */
        public void DrawFrame(int textureId, float[] texMatrix)
        {
            // Use the identity matrix for MVP so our 2x2 FULL_RECTANGLE covers the viewport.
            _mProgram.Draw(GlUtil.IdentityMatrix, _mRectDrawable.GetVertexArray(), 0,
                    _mRectDrawable.GetVertexCount(), _mRectDrawable.GetCoordsPerVertex(),
                    _mRectDrawable.GetVertexStride(),
                    texMatrix, _mRectDrawable.GetTexCoordArray(), textureId,
                    _mRectDrawable.GetTexCoordStride());
        }
    }
}