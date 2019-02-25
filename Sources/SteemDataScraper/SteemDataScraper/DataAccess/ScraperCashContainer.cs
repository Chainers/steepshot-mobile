using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Steem.Operations;
using Npgsql;
using SteemDataScraper.Models;

namespace SteemDataScraper.DataAccess
{
    public class ScraperCashContainer
    {
        readonly List<TransferTable> _transfers = new List<TransferTable>();
        readonly List<AccountTable> _accounts = new List<AccountTable>();
        readonly List<AccountTable> _updateAccounts = new List<AccountTable>();
        readonly List<PostTable> _updatePosts = new List<PostTable>();
        readonly List<VoteTemp> _updateVotes = new List<VoteTemp>();

        public int Count;


        public void Add(TransferTable transfer)
        {
            _transfers.Add(transfer);
            Interlocked.Increment(ref Count);
        }

        public void Add(AccountTable account)
        {
            _accounts.Add(account);
            Interlocked.Increment(ref Count);
        }


        public void Update(VoteTemp value)
        {
            var prev = _updateVotes.SingleOrDefault(a => a.Login.Equals(value.Login));
            if (prev != null)
            {
                if (value.CompareTo(prev) > 0)
                {
                    _updateVotes.Remove(prev);
                    _updateVotes.Add(value);
                }
            }
            else
            {
                _updateVotes.Add(value);
                Interlocked.Increment(ref Count);
            }
        }

        public void Update(AccountTable value)
        {
            var prev = _updateAccounts.SingleOrDefault(a => a.Login.Equals(value.Login));
            if (prev != null)
            {
                if (value.CompareTo(prev) > 0)
                {
                    _updateAccounts.Remove(prev);
                    _updateAccounts.Add(value);
                }
            }
            else
            {
                _updateAccounts.Add(value);
                Interlocked.Increment(ref Count);
            }
        }

        public void Update(PostTable value)
        {
            var prev = _updatePosts.SingleOrDefault(p => p.Login.Equals(value.Login) && p.Permlink.Equals(value.Permlink));
            if (prev != null)
            {
                if (value.CompareTo(prev) > 0)
                {
                    prev.Title = value.Title;
                    prev.JsonData = value.JsonData;
                    prev.Body = value.Body;
                }
                else
                {
                    prev.Timestamp = value.Timestamp;
                }
            }
            else
            {
                _updatePosts.Add(value);
                Interlocked.Increment(ref Count);
            }
        }



        private void DoCopy<T>(NpgsqlConnection connection, long toBlockNum, List<T> set)
            where T : BaseTable
        {
            if (!set.Any())
                return;

            set.Sort();

            if (set[0].BlockNum > toBlockNum)
                return;

            var save = set.Where(i => i.BlockNum <= toBlockNum).ToArray();
            set.RemoveRange(0, save.Length);

            var cmd = save[0].CopyCommandText();
            using (var binaryImport = connection.BeginBinaryImport(cmd))
            {
                foreach (var itm in save)
                {
                    itm.Import(binaryImport);
                    Count--;
                }

                binaryImport.Complete();
            }
        }

        private async Task DoUpdate<T>(NpgsqlConnection connection, long toBlockNum, List<T> set, CancellationToken token)
            where T : BaseTable
        {
            if (!set.Any())
                return;

            set.Sort();

            if (set[0].BlockNum > toBlockNum)
                return;

            var save = set.Where(i => i.BlockNum <= toBlockNum).ToArray();
            set.RemoveRange(0, save.Length);


            var cmd = save[0].BulkUpdate();

            var command = new NpgsqlCommand
            {
                Connection = connection,
                CommandText = cmd
            };

            foreach (var itm in save)
            {
                itm.AddParameters(command.Parameters);
                Count--;
            }

            await command.ExecuteNonQueryAsync(token);
        }

        private async Task LoadPostsAndUpdateVotes<T>(NpgsqlConnection connection, long toBlockNum, List<T> set, CancellationToken token)
            where T : BaseTable
        {
            if (!set.Any())
                return;

            set.Sort();

            if (set[0].BlockNum > toBlockNum)
                return;

            var save = set.Where(i => i.BlockNum <= toBlockNum).ToArray();
            set.RemoveRange(0, save.Length);


            connection.SelectPosts();


            var cmd = save[0].BulkUpdate();

            var command = new NpgsqlCommand
            {
                Connection = connection,
                CommandText = cmd
            };

            foreach (var itm in save)
            {
                itm.AddParameters(command.Parameters);
                Count--;
            }

            await command.ExecuteNonQueryAsync(token);
        }


        private async Task DoPartitionCopy(NpgsqlConnection connection, long toBlockNum, List<TransferTable> set, CancellationToken token)
        {
            if (!set.Any())
                return;

            set.Sort();

            if (set[0].BlockNum > toBlockNum)
                return;

            var save = set.Where(i => i.BlockNum <= toBlockNum).ToArray();
            set.RemoveRange(0, save.Length);

            var parts = save.GroupBy(i => new DateTime(i.Timestamp.Year, i.Timestamp.Month, 1));

            foreach (var part in parts)
            {
                var cmd = part.First().CopyCommandText();

                await connection.CreateTransferPartitionIfNotExist(part.Key, token);

                using (var writer = connection.BeginBinaryImport(cmd))
                {
                    foreach (var itm in part)
                    {
                        itm.Import(writer);
                        Count--;
                    }

                    writer.Complete();
                }
            }
        }




        public async Task CommitAndDispose(NpgsqlConnection connection, long maxBlock, CancellationToken token)
        {
            OptimizeAccounts();
            OptimizeVotes();
            
            await LoadPostsAndUpdateVotes(connection, maxBlock, _updateAccounts, token);

            await DoPartitionCopy(connection, maxBlock, _transfers, token);
            DoCopy(connection, maxBlock, _accounts);

            await DoUpdate(connection, maxBlock, _updateAccounts, token);
            await DoUpdate(connection, maxBlock, _updatePosts, token);
        }

        private void OptimizeAccounts()
        {
            for (int i = _updateAccounts.Count - 1; i >= 0; i--)
            {
                var update = _updateAccounts[i];
                var acc = _accounts.SingleOrDefault(a => a.Login.Equals(update.Login));
                if (acc != null)
                {
                    acc.JsonData = update.JsonData;
                    TryUploadUserAvatar(acc.JsonData);
                    _updateAccounts.RemoveAt(i);
                }
            }
        }

        private void OptimizeVotes()
        {
            for (int i = _updateVotes.Count - 1; i >= 0; i--)
            {
                var vote = _updateVotes[i];
                var post = _updatePosts.SingleOrDefault(a => a.Login.Equals(vote.Login) && a.Permlink.Equals(vote.Permlink));
                if (post != null)
                {
                    post.VoteTemps.Add(vote);
                    _updateVotes.RemoveAt(i);
                }
            }
        }

        private void TryUploadUserAvatar(string json)
        {
            //throw new NotImplementedException();
        }

        internal void Clear()
        {
            _transfers.Clear();
            //_delayedTransactions.Clear();
            //_tokenActions.Clear();
            //_blocks.Clear();
            Count = 0;
        }
    }
}
