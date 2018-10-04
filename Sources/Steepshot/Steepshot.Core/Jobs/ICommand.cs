using System.Threading;
using System.Threading.Tasks;

namespace Steepshot.Core.Jobs
{
    public interface ICommand
    {
        int CommandId { get; }
        
        Task<JobState> Execute(int id, CancellationToken token);

        void CleanData(int jobDataId);

        object GetResult(int jobDataId);
    }
}
