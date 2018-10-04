using System;
using System.Threading.Tasks;

namespace Steepshot.Core.Interfaces
{
    public interface ILogService
    {
        Task FatalAsync(Exception ex);

        Task ErrorAsync(Exception ex);

        Task WarningAsync(Exception ex);

        Task InfoAsync(Exception ex);
    }
}