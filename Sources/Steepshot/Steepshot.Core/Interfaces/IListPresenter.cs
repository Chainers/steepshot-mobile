using System.Collections.Generic;

namespace Steepshot.Core.Interfaces
{
    public interface IListPresenter
    {
        bool IsLastReaded { get; }

        int Count { get; }
    }
}
