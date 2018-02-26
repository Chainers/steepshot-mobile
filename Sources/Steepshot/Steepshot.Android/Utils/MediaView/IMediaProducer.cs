using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.MediaView
{

    public interface IMediaProducer
    {
        void Init(MediaModel media);
        void Prepare();
        void Play();
        void Pause();
        void Release();
    }
}