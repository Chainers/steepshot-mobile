using System.Threading.Tasks;
using Android.Graphics;
using Android.Provider;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

namespace Steepshot.Utils.Media
{
    public interface IMediaPerformer
    {
        MediaType MediaType { get; }
        MediaModel MediaSource { get; set; }
        void DrawBuffer();
        Task<bool> PrepareBufferAsync(Bitmap bitmap);
        void ReleaseBuffer();
    }
}