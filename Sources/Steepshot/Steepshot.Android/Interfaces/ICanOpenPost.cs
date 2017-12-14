using Steepshot.Core.Models.Common;

namespace Steepshot.Interfaces
{
    public interface ICanOpenPost
    {
        void OpenPost(Post post);
        bool ClosePost();
    }
}