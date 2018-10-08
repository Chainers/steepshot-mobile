using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Interfaces
{
    public interface IPageCloser
    {
        void OpenPost(Post post);
        bool ClosePost();
    }
}
