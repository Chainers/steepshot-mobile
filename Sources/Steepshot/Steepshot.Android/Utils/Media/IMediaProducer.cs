using Android.Graphics;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
{
    public interface IMediaProducer
    {
        void Init(MediaModel media);
        void Prepare(SurfaceTexture surfaceTextureace, int width, int height);
        void Play();
        void Pause();
        void Release();
    }
}