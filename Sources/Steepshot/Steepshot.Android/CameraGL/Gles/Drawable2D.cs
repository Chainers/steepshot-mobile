using Java.Lang;
using Java.Nio;

namespace Steepshot.CameraGL.Gles
{
    public class Drawable2D : object
    {
        private static int SIZEOF_FLOAT = 4;

        /**
         * Simple equilateral triangle (1.0 per side).  Centered on (0,0).
         */
        private static readonly float[] TriangleCoords = {
         0.0f,  0.577350269f,   // 0 top
        -0.5f, -0.288675135f,   // 1 bottom left
         0.5f, -0.288675135f    // 2 bottom right
    };
        private static readonly float[] TriangleTexCoords = {
        0.5f, 0.0f,     // 0 top center
        0.0f, 1.0f,     // 1 bottom left
        1.0f, 1.0f,     // 2 bottom right
    };
        private static readonly FloatBuffer TriangleBuf =
            GlUtil.CreateFloatBuffer(TriangleCoords);
        private static readonly FloatBuffer TriangleTexBuf =
                GlUtil.CreateFloatBuffer(TriangleTexCoords);

        /**
         * Simple square, specified as a triangle strip.  The square is centered on (0,0) and has
         * a size of 1x1.
         * <p>
         * Triangles are 0-1-2 and 2-1-3 (counter-clockwise winding).
         */
        private static readonly float[] RectangleCoords = {
        -0.5f, -0.5f,   // 0 bottom left
         0.5f, -0.5f,   // 1 bottom right
        -0.5f,  0.5f,   // 2 top left
         0.5f,  0.5f,   // 3 top right
    };
        private static readonly float[] RectangleTexCoords = {
        0.0f, 1.0f,     // 0 bottom left
        1.0f, 1.0f,     // 1 bottom right
        0.0f, 0.0f,     // 2 top left
        1.0f, 0.0f      // 3 top right
    };
        private static readonly FloatBuffer RectangleBuf =
            GlUtil.CreateFloatBuffer(RectangleCoords);
        private static readonly FloatBuffer RectangleTexBuf =
                GlUtil.CreateFloatBuffer(RectangleTexCoords);

        /**
         * A "full" square, extending from -1 to +1 in both dimensions.  When the model/view/projection
         * matrix is identity, this will exactly cover the viewport.
         * <p>
         * The texture coordinates are Y-inverted relative to RECTANGLE.  (This seems to work out
         * right with external textures from SurfaceTexture.)
         */
        private static readonly float[] FullRectangleCoords = {
        -1.0f, -1.0f,   // 0 bottom left
         1.0f, -1.0f,   // 1 bottom right
        -1.0f,  1.0f,   // 2 top left
         1.0f,  1.0f,   // 3 top right
    };
        private static readonly float[] FullRectangleTexCoords = {
        0.0f, 0.0f,     // 0 bottom left
        1.0f, 0.0f,     // 1 bottom right
        0.0f, 1.0f,     // 2 top left
        1.0f, 1.0f      // 3 top right
    };
        private static readonly FloatBuffer FullRectangleBuf =
            GlUtil.CreateFloatBuffer(FullRectangleCoords);
        private static readonly FloatBuffer FullRectangleTexBuf =
                GlUtil.CreateFloatBuffer(FullRectangleTexCoords);


        private readonly FloatBuffer _mVertexArray;
        private readonly FloatBuffer _mTexCoordArray;
        private readonly int _mVertexCount;
        private readonly int _mCoordsPerVertex;
        private readonly int _mVertexStride;
        private readonly int _mTexCoordStride;
        private readonly Prefab _mPrefab;

        /**
         * Enum values for constructor.
         */
        public enum Prefab
        {
            Triangle, Rectangle, FullRectangle
        }

        /**
         * Prepares a drawable from a "pre-fabricated" shape definition.
         * <p>
         * Does no EGL/GL operations, so this can be done at any time.
         */
        public Drawable2D(Prefab shape)
        {
            switch (shape)
            {
                case Prefab.Triangle:
                    _mVertexArray = TriangleBuf;
                    _mTexCoordArray = TriangleTexBuf;
                    _mCoordsPerVertex = 2;
                    _mVertexStride = _mCoordsPerVertex * SIZEOF_FLOAT;
                    _mVertexCount = TriangleCoords.Length / _mCoordsPerVertex;
                    break;
                case Prefab.Rectangle:
                    _mVertexArray = RectangleBuf;
                    _mTexCoordArray = RectangleTexBuf;
                    _mCoordsPerVertex = 2;
                    _mVertexStride = _mCoordsPerVertex * SIZEOF_FLOAT;
                    _mVertexCount = RectangleCoords.Length / _mCoordsPerVertex;
                    break;
                case Prefab.FullRectangle:
                    _mVertexArray = FullRectangleBuf;
                    _mTexCoordArray = FullRectangleTexBuf;
                    _mCoordsPerVertex = 2;
                    _mVertexStride = _mCoordsPerVertex * SIZEOF_FLOAT;
                    _mVertexCount = FullRectangleCoords.Length / _mCoordsPerVertex;
                    break;
                default:
                    throw new RuntimeException("Unknown shape " + shape);
            }
            _mTexCoordStride = 2 * SIZEOF_FLOAT;
            _mPrefab = shape;
        }

        /**
         * Returns the array of vertices.
         * <p>
         * To avoid allocations, this returns internal state.  The caller must not modify it.
         */
        public FloatBuffer GetVertexArray()
        {
            return _mVertexArray;
        }

        /**
         * Returns the array of texture coordinates.
         * <p>
         * To avoid allocations, this returns internal state.  The caller must not modify it.
         */
        public FloatBuffer GetTexCoordArray()
        {
            return _mTexCoordArray;
        }

        /**
         * Returns the number of vertices stored in the vertex array.
         */
        public int GetVertexCount()
        {
            return _mVertexCount;
        }

        /**
         * Returns the width, in bytes, of the data for each vertex.
         */
        public int GetVertexStride()
        {
            return _mVertexStride;
        }

        /**
         * Returns the width, in bytes, of the data for each texture coordinate.
         */
        public int GetTexCoordStride()
        {
            return _mTexCoordStride;
        }

        /**
         * Returns the number of position coordinates per vertex.  This will be 2 or 3.
         */
        public int GetCoordsPerVertex()
        {
            return _mCoordsPerVertex;
        }

        public override string ToString()
        {
            return "[Drawable2d: " + _mPrefab + "]";
        }

    }
}