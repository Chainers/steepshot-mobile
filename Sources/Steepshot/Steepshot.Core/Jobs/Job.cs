using System;
using System.Threading;
using SQLite;

namespace Steepshot.Core.Jobs
{
    [Table(nameof(Job))]
    public class Job : SqlTableBase
    {
        public int CommandId { get; set; }

        public int DataId { get; set; }

        public JobState State { get; set; }

        [Ignore]
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public DateTime LastStartTime { get; set; }


        public Job() { }

        public Job(int commandId)
        {
            CommandId = commandId;
        }
    }
}