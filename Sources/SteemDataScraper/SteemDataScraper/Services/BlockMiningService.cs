using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core;
using Ditch.Core.JsonRpc;
using Ditch.Steem;
using Ditch.Steem.Models;
using Newtonsoft.Json;
using Npgsql;
using SteemDataScraper.DataAccess;
using SteemDataScraper.Models;

namespace SteemDataScraper.Services
{
    public sealed class BlockMiningService
    {
        private readonly DownloadWorker[] _workers = new DownloadWorker[DownloadWorkerSetLet];
        private const int DownloadWorkerSetLet = 10;
        private List<NodeInfo> _nodes;

        private uint _lastIrreversibleBlockNum;
        private int _workerCount;
        private int _taskCount;
        private byte _threadsCount = 100;
        public byte ThreadsCount
        {
            get => _threadsCount;
            set
            {
                if (value > 0)
                    _threadsCount = value;
            }
        }

        public delegate void CallbackDelegate(BlockResult result);
        public CallbackDelegate Callback;

        private void AddResult(BlockResult result, CancellationToken token)
        {
            if (result.Value == null || result.Value.IsError || result.Value?.Result == null)
            {
                AddTaskAsync(result.BlockNum, token);
                Console.WriteLine($"{result.BlockNum} | {result.Value?.RawResponse}");
            }
            else
            {
                Callback.Invoke(result);
                Interlocked.Decrement(ref _workerCount);
                Interlocked.Decrement(ref _taskCount);
            }
        }

        public async Task InitCashAsync(NpgsqlConnection connection, CancellationToken token)
        {
            _nodes?.Clear();
            _nodes = await connection.GetAllNodeInfo(token)
                .ConfigureAwait(false);
            _workerCount = 0;

            var nodeInfos = _nodes
                .OrderByDescending(n => n.SuccessCount / (double)(n.SuccessCount + n.FailCount))
                .ThenBy(n => n.ElapsedMilliseconds / (n.SuccessCount + 1.0))
                .Take(DownloadWorkerSetLet);

            var i = 0;
            foreach (var nodeInfo in nodeInfos)
            {
                var w = new DownloadWorker(nodeInfo, this);
                var connected = await w.TryToConnectAsync(token);
                if (connected)
                    _workers[i++] = w;
            }
        }

        private async Task AddTaskAsync(uint blockNum, CancellationToken token)
        {
            var w = await GetDownloadWorkerAsync(token)
                .ConfigureAwait(false);
            await w.AddBlockAsync(blockNum, token);
        }

        public async Task StartAsync(uint from, uint to, CancellationToken token)
        {
            if (_lastIrreversibleBlockNum < to)
            {
                var w = await GetDownloadWorkerAsync(token)
                    .ConfigureAwait(false);

                _lastIrreversibleBlockNum = await w.GetLastIrreversibleBlockNum(token)
                    .ConfigureAwait(false);

                if (_lastIrreversibleBlockNum != -1)
                    to = Math.Min(_lastIrreversibleBlockNum, to);
            }

            _taskCount = (int)(to - from);
            for (var blockNum = from; blockNum <= to; blockNum++)
            {
                token.ThrowIfCancellationRequested();

                while (_workerCount > ThreadsCount)
                {
                    await Task.Delay(100, token);
                }

                var w = await GetDownloadWorkerAsync(token)
                    .ConfigureAwait(false);

                w.AddBlockAsync(blockNum, token);
                Interlocked.Increment(ref _workerCount);
            }

            while (_workerCount > 0)
            {
                await Task.Delay(100, token);
            }
        }

        public async Task ExecuteForRange(List<uint> set, CancellationToken token)
        {
            foreach (var blockNum in set)
            {
                token.ThrowIfCancellationRequested();

                while (_workerCount > ThreadsCount)
                {
                    await Task.Delay(100, token);
                }

                var w = await GetDownloadWorkerAsync(token)
                    .ConfigureAwait(false);

                w.AddBlockAsync(blockNum, token);
                Interlocked.Increment(ref _workerCount);
            }

            while (_workerCount > 0)
            {
                await Task.Delay(100, token);
            }
        }


        private readonly SemaphoreSlim _downloadSemaphore = new SemaphoreSlim(1);
        private async Task<DownloadWorker> GetDownloadWorkerAsync(CancellationToken token)
        {
            const int maxThreadPerWorker = 5;
            await _downloadSemaphore.WaitAsync(token);

            try
            {
                do
                {
                    DownloadWorker worker;
                    lock (_workers)
                    {
                        worker = _workers
                            .Where(w => w != null && w.TaskCount < maxThreadPerWorker && (w.Workload < 500 || w.TaskCount == 0))
                            .OrderBy(w => w.Workload)
                            .FirstOrDefault();
                    }

                    if (worker == null)
                    {
                        await Task.Delay(100, token)
                            .ConfigureAwait(false);
                        continue;
                    }

                    return worker;
                } while (true);
            }
            finally
            {
                _downloadSemaphore.Release(1);
            }
        }

        public async Task UpdateNodeInfoAsync(NpgsqlConnection connection, CancellationToken token)
        {
            foreach (var node in _nodes)
            {
                await connection.UpdateAsync(node, token);
            }
        }

        public string PrintStatus(StringBuilder sb)
        {
            DownloadWorker[] workers;
            lock (_workers)
            {
                workers = _workers.ToArray();
            }

            sb.AppendLine("{");
            sb.AppendLine($"  \"last_irreversible_block_num\"=\"{_lastIrreversibleBlockNum}\",");
            sb.AppendLine($"  \"task_in_queue\"=\"{_taskCount}\",");
            sb.AppendLine($"  \"worker_count\"=\"{_workerCount}\",");
            sb.AppendLine($"  \"workers\"={JsonConvert.SerializeObject(workers, Formatting.Indented)}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        [JsonObject(MemberSerialization.OptIn)]
        private class DownloadWorker
        {
            private readonly OperationManager _operationManager;
            private readonly BlockMiningService _miningService;
            private int _taskCount;

            [JsonProperty("node_info")]
            public readonly NodeInfo Node;


            [JsonProperty("task_count")]
            public int TaskCount
            {
                get => _taskCount;
                private set => _taskCount = value;
            }

            public double Workload
            {
                get
                {
                    lock (Node)
                    {
                        return Node.Velocity * (TaskCount + 1);
                    }
                }
            }

            public DownloadWorker(NodeInfo node, BlockMiningService miningService)
            {
                _miningService = miningService;
                Node = node;

                var httpClient = new RepeatHttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(3);
                var httpManager = new HttpManager(httpClient);
                _operationManager = new OperationManager(httpManager);
            }

            public async Task<bool> TryToConnectAsync(CancellationToken token)
            {
                try
                {
                    return await _operationManager
                        .ConnectToAsync(Node.Url, token)
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return false;
            }

            public async Task AddBlockAsync(uint blockNum, CancellationToken token)
            {
                Interlocked.Increment(ref _taskCount);

                var start = DateTime.Now;
                var args = new GetBlockArgs { BlockNum = blockNum };
                var response = await _operationManager.GetBlockAsync(args, token)
                    .ConfigureAwait(false);
                var end = DateTime.Now;

                var result = new BlockResult
                {
                    BlockNum = blockNum,
                    Value = response
                };

                lock (Node)
                {
                    Node.Update(end - start, !response.IsError && result.Value != null);
                }

                _miningService.AddResult(result, token);
                Interlocked.Decrement(ref _taskCount);
            }

            public async Task<uint> GetLastIrreversibleBlockNum(CancellationToken token)
            {
                var result = await _operationManager.GetDynamicGlobalPropertiesAsync(token)
                    .ConfigureAwait(false);
                if (result.IsError)
                    return 0;

                return result.Result.LastIrreversibleBlockNum;
            }
        }
    }

    public class BlockResult
    {
        public uint BlockNum { get; set; }

        public JsonRpcResponse<GetBlockReturn> Value { get; set; }
    }
}
