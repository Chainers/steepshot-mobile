using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
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