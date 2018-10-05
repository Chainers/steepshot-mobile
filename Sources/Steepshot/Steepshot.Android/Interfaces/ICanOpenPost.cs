using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

namespace Steepshot.Interfaces
{
    public interface ICanOpenPost
    {
        void OpenPost(Post post);
        bool ClosePost();
    }
}