using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

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