using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Interfaces
{
    public interface IPageCloser
    {
        void OpenPost(Post post);
        bool ClosePost();
    }
}
