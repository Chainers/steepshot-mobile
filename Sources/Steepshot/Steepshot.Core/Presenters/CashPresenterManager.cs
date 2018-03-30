using System.Collections.Generic;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    internal static class CashPresenterManager
    {
        private static readonly Dictionary<string, Container<Post>> PostsCash = new Dictionary<string, Container<Post>>();
        private static readonly Dictionary<string, HashSet<IFollowable>> FollowableCash = new Dictionary<string, HashSet<IFollowable>>();

        private class Container<T>
        {
            public readonly T Item;
            public int RefCount;

            public Container(T item)
            {
                Item = item;
                RefCount = 1;
            }
        }


        #region Post

        public static Post Add(Post item)
        {
            if (item == null)
                return null;

            lock (PostsCash)
            {
                if (PostsCash.ContainsKey(item.Url))
                {
                    var container = PostsCash[item.Url];
                    container.RefCount++;
                    CopyPost(container.Item, item);
                    return container.Item;
                }
                PostsCash.Add(item.Url, new Container<Post>(item));
                return item;
            }
        }

        public static void RemoveAll(IEnumerable<Post> items)
        {
            lock (PostsCash)
            {
                foreach (var item in items)
                    Remove(item);
            }
        }

        public static void RemoveRef(Post item)
        {
            if (item == null)
                return;

            lock (PostsCash)
            {
                if (PostsCash.ContainsKey(item.Url))
                    PostsCash.Remove(item.Url);
            }
        }

        private static void Remove(Post item)
        {
            if (item == null)
                return;

            if (PostsCash.ContainsKey(item.Url))
            {
                var container = PostsCash[item.Url];
                container.RefCount--;
                if (container.RefCount < 1)
                    PostsCash.Remove(item.Url);
            }
        }

        private static void CopyPost(Post item1, Post item2)
        {
            item1.Body = item2.Body;
            item1.Media = item2.Media;
            item1.Description = item2.Description;
            item1.Title = item2.Title;
            item1.Avatar = item2.Avatar;
            item1.CoverImage = item2.CoverImage;
            item1.AuthorRewards = item2.AuthorRewards;
            item1.AuthorReputation = item2.AuthorReputation;
            item1.NetVotes = item2.NetVotes;
            item1.NetLikes = item2.NetLikes;
            item1.NetFlags = item2.NetFlags;
            item1.Children = item2.Children;
            item1.CuratorPayoutValue = item2.CuratorPayoutValue;
            item1.TotalPayoutValue = item2.TotalPayoutValue;
            item1.PendingPayoutValue = item2.PendingPayoutValue;
            item1.MaxAcceptedPayout = item2.MaxAcceptedPayout;
            item1.TotalPayoutReward = item2.TotalPayoutReward;
            item1.Vote = item2.Vote;
            item1.Flag = item2.Flag;
            item1.Tags = item2.Tags;
            item1.Depth = item2.Depth;
            item1.Resteemed = item2.Resteemed;
            item1.TopLikersAvatars = item2.TopLikersAvatars;
            item1.IsLowRated = item2.IsLowRated;
            item1.IsNsfw = item2.IsNsfw;
            item1.CashoutTime = item2.CashoutTime;
        }

        #endregion


        #region FollowableCash

        public static void Add(IEnumerable<IFollowable> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public static void Add(IFollowable item)
        {
            if (item == null)
                return;
            lock (FollowableCash)
            {
                if (FollowableCash.ContainsKey(item.Key))
                {
                    var container = FollowableCash[item.Key];
                    if (!container.Contains(item))
                    {
                        Update(item);
                        container.Add(item);
                    }
                }
                else
                {
                    var hs = new HashSet<IFollowable> { item };
                    FollowableCash.Add(item.Key, hs);
                }
            }
        }

        public static void RemoveAll(IEnumerable<IFollowable> items)
        {
            foreach (var item in items)
                Remove(item);
        }

        public static void Update(IFollowable item)
        {
            if (item == null)
                return;

            lock (FollowableCash)
            {
                if (FollowableCash.ContainsKey(item.Key))
                {
                    var container = FollowableCash[item.Key];
                    foreach (var followable in container)
                    {
                        CopyIFollowable(followable, item);
                    }
                }
            }
        }

        public static void Remove(IFollowable item)
        {
            if (item == null)
                return;

            lock (FollowableCash)
            {
                if (FollowableCash.ContainsKey(item.Key))
                {
                    var container = FollowableCash[item.Key];
                    if (container.Contains(item))
                        container.Remove(item);
                }
            }
        }

        private static void CopyIFollowable(IFollowable item1, IFollowable item2)
        {
            item1.HasFollowed = item2.HasFollowed;
        }

        #endregion


    }
}
