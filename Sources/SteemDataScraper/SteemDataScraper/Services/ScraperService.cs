using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Steem.Operations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using SteemDataScraper.DataAccess;
using SteemDataScraper.Extensions;
using SteemDataScraper.Models;

namespace SteemDataScraper.Services
{
    public sealed class ScraperService : BaseDbService
    {
        private readonly BlockMiningService _blockMiningService;
        private readonly DateTime _start;
        private readonly ScraperCashContainer _container = new ScraperCashContainer();
        private readonly List<long> _blockIds = new List<long>();
        private readonly List<MonitorInfo> _stateLoad = new List<MonitorInfo>();
        private readonly List<Tuple<long, int>> _stateSave = new List<Tuple<long, int>>();
        private ScraperState _scraperState;

        public const int ServiceId = 1;

        public byte ThreadsCount
        {
            get => _blockMiningService.ThreadsCount;
            set => _blockMiningService.ThreadsCount = value;
        }

        public uint BlockRange { get; set; } = 5000;

        public ScraperService(ILogger<ScraperService> logger, IConfiguration configuration)
        : base(logger, configuration)
        {
            _blockMiningService = new BlockMiningService { Callback = AddResult };
            _start = DateTime.Now;
        }

        public uint MoveToMaxСonsistentBlockNum(uint blockId)
        {
            _blockIds.Sort();
            int count = 0;
            foreach (var b in _blockIds)
            {
                if (blockId + 1 == b)
                {
                    blockId++;
                    count++;
                }
                else
                {
                    break;
                }
            }

            if (count > 0)
                _blockIds.RemoveRange(0, count);
            return blockId;
        }

        protected override async Task DoSomethingAsync(NpgsqlConnection connection, CancellationToken token)
        {
            uint lastBlockNum = 0;
            _blockIds.Clear();
            _container.Clear();
            bool isDelay;
            var st = DateTime.Now;

            try
            {
                _scraperState = await connection.GetServiceStateAsync<ScraperState>(ServiceId, token);
                await _blockMiningService.InitCashAsync(connection, token);

                var count = BlockRange;
                await _blockMiningService.StartAsync(_scraperState.BlockId + 1, _scraperState.BlockId + count, token);
                isDelay = count > _blockIds.Count;
                _stateLoad.Add(new MonitorInfo(st, _scraperState.BlockId, (long)(DateTime.Now - st).TotalMilliseconds, _blockIds.Count));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _stateLoad.Add(new MonitorInfo(st, _scraperState.BlockId));
                throw new Exception($"Last block num: {lastBlockNum}", e);
            }
            finally
            {
                if (!token.IsCancellationRequested && _scraperState != null)
                {
                    await BulkSaveAsync(connection, _container, _scraperState, token);
                }
            }

            if (isDelay)
            {
                //await connection.AddPrimaryKeyAndRelations(token);
                //await connection.AddPartitionsPrimaryKeyAndRelations(token);
                //await connection.UpdateDelayedTransferAsync(token);
                //await connection.UpdateDelayedTokenAsync(token);
                //await connection.UpdateTransferInfo(token);
                await Task.Delay(TimeSpan.FromMinutes(5), token);
            }
        }

        private void AddResult(BlockResult result)
        {
            lock (_container)
            {
                Parse(_container, result);
                _blockIds.Add(result.BlockNum);
            }
        }

        private string JsonBeautify(string json)
        {
            if (string.IsNullOrEmpty(json))
                return json;
            var obj = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        private HashSet<string> skip = new HashSet<string>
        {
            "pow_operation",
            "account_witness_vote_operation",
            "account_witness_proxy_operation",
            "feed_publish_operation"
        };

        private void Parse(ScraperCashContainer container, BlockResult result)
        {
            var block = result.Value.Result.Block;

            if (block.Transactions.Any())
            {
                var j = JsonBeautify(result.Value.RawResponse);

                for (short trxNum = 0; trxNum < block.Transactions.Length; trxNum++)
                {
                    var transaction = block.Transactions[trxNum];
                    for (short opNum = 0; opNum < transaction.Operations.Length; opNum++)
                    {
                        BaseOperation operation = transaction.Operations[opNum];

                        switch (operation)
                        {
                            case TransferOperation typed:
                                {
                                    var transfer = new TransferTable(result.BlockNum, trxNum, opNum, block.Timestamp.Value, typed);
                                    container.Add(transfer);
                                    break;
                                }
                            case TransferToVestingOperation typed:
                                {
                                    var transfer = new TransferTable(result.BlockNum, trxNum, opNum, block.Timestamp.Value, typed);
                                    container.Add(transfer);
                                    break;
                                }
                            case AccountCreateOperation typed:
                                {
                                    var account = new AccountTable(result.BlockNum, trxNum, opNum, block.Timestamp.Value, typed);
                                    container.Add(account);
                                    break;
                                }
                            case AccountUpdateOperation typed:
                                {
                                    var account = new AccountTable(result.BlockNum, trxNum, opNum, block.Timestamp.Value, typed);
                                    container.Update(account);
                                    break;
                                }
                            case CommentOperation typed:
                                {
                                    if (typed.Author.Equals("joseph.kalu"))
                                    {

                                    }
                                    // if ((string.IsNullOrEmpty(operation.JsonMetadata) || !operation.JsonMetadata.Contains("")) && !operation.Body.Equals("*deleted*"))
                                    if ((string.IsNullOrEmpty(typed.JsonMetadata) || typed.JsonMetadata.Equals("{}")) && !typed.Body.Equals("*deleted*"))
                                    {
                                        break;
                                    }

                                    if (string.IsNullOrEmpty(typed.ParentAuthor))
                                    {
                                        var post = new PostTable(result.BlockNum, trxNum, opNum, block.Timestamp.Value, typed);
                                        container.Update(post);
                                    }
                                    else
                                    {
                                        throw new NotImplementedException("reply");
                                    }
                                    break;
                                }
                            case VoteOperation typed:
                                {
                                    var vote = new VoteTemp(result.BlockNum, trxNum, opNum, typed);
                                    container.Update(vote);
                                    break;
                                }
                            case UnsupportedOperation typed:
                                {
                                    var name = typed.TypeName;

                                    if (skip.Contains(name))
                                        break;

                                    try
                                    {
                                        if (name.Equals("custom_operation"))
                                        {
                                            var opName = ((Newtonsoft.Json.Linq.JProperty)typed.Value.First).Name;
                                            if (opName.Equals("required_auths"))
                                            {
                                                break;
                                            }
                                            else
                                            {

                                            }
                                        }
                                    }
                                    catch
                                    {

                                    }


                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                }
            }
        }

        private async Task BulkSaveAsync(NpgsqlConnection connection, ScraperCashContainer container, ScraperState scraperState, CancellationToken token)
        {
            NpgsqlTransaction transaction = null;

            try
            {
                var st = DateTime.Now;
                transaction = connection.BeginTransaction();
                scraperState.BlockId = MoveToMaxСonsistentBlockNum(scraperState.BlockId);
                var count = container.Count;
                await container.CommitAndDispose(connection, scraperState.BlockId, token);
                await connection.UpdateServiceStateAsync(ServiceId, JsonConvert.SerializeObject(scraperState), token);
                await _blockMiningService.UpdateNodeInfoAsync(connection, token);
                transaction.Commit();
                _stateSave.Add(new Tuple<long, int>((long)(DateTime.Now - st).TotalMilliseconds, count));
            }
            catch (Exception e)
            {
                transaction.RollbackAndDispose();
                _stateSave.Add(new Tuple<long, int>(-1, -1));
                Logger.LogCritical(e, "BulkSaveAsync");
                throw;
            }
        }

        public void PrintStatus(StringBuilder sb)
        {
            if (_scraperState == null)
                return;

            sb.AppendLine("{");
            sb.AppendLine($"  \"start_time\"={_start},");
            sb.AppendLine($"  \"block_range\"={BlockRange},");
            sb.AppendLine($"  \"container_count\"={_container.Count},");
            sb.AppendLine($"  \"start_block\"={_scraperState.BlockId},");
            sb.AppendLine("  \"stats\"=[");
            sb.AppendLine("      |_____start_time____||______from/to______||______total_read______||__________insert_________|");
            for (int i = _stateSave.Count - 1; i >= 0; i--)
            {
                var l = _stateLoad[i];
                var s = _stateSave[i];
                sb.AppendLine($"      [{l.Start:G}][{l.FromBlock} > {l.ToBlock}][{l.Count}/{l.ReadSeconds:N1} | {l.BlockPerSec:N1} b/s][{s.Item2}/{s.Item1} | {s.Item2 * 1000.0 / s.Item1:N1} i/s]");
            }
            sb.AppendLine("  ],");
            _blockMiningService.PrintStatus(sb);
            sb.AppendLine("}");
        }
    }

    internal class MonitorInfo
    {
        public MonitorInfo(DateTime start, long fromBlock, long readMilliseconds, int count)
        {
            Start = start;
            FromBlock = fromBlock;
            ReadSeconds = readMilliseconds / 1000.0;
            Count = count;
        }

        public MonitorInfo(DateTime start, long fromBlock)
        {
            Start = start;
            FromBlock = fromBlock;
            ReadSeconds = -1;
            Count = -1;
        }

        public DateTime Start { get; set; }

        public long FromBlock { get; set; }

        public long ToBlock => FromBlock + Count;

        public double ReadSeconds { get; set; }

        public long Count { get; set; }

        public double BlockPerSec => Count / ReadSeconds;
    }

    internal class ScraperState
    {
        public uint BlockId { get; set; }
    }
}