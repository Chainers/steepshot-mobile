using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Database;

namespace Steepshot.Core.Interfaces
{
    public interface ICommand
    {
        string CommandId { get; }

        Task<JobState> Execute(int id, CancellationToken token);

        void CleanData(int jobDataId);

        object GetResult(int jobDataId);
    }
}
