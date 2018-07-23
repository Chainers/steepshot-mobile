using System;
using System.Threading.Tasks;

namespace Steepshot.Core.Services
{
    public interface ILogService
    {
        Task Fatal(Exception ex);

        Task Error(Exception ex);

        Task Warning(Exception ex);

        Task Info(Exception ex);
    }
}