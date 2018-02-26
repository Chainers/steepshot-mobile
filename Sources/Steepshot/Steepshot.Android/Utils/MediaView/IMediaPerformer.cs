using Android.Graphics;
using Steepshot.Core.Models.Common;
using System.Threading.Tasks;

namespace Steepshot.Utils.MediaView
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