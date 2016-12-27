using System;
using Android.Support.V7.Widget;
using Android.Views;
using Steemix.Library.Models.Responses;
using Android.Widget;
using System.Collections.Generic;
using Square.Picasso;
using Android.Content;

namespace Steemix.Android.Adapter
{

    public class FeedAdapter : RecyclerView.Adapter
    {
        List<UserPost> Posts;
        private Context context;

        public FeedAdapter(Context context, List<UserPost> Posts=null)
        {
            this.context = context;
            if (Posts == null)
                this.Posts = new List<UserPost>();
            else
                this.Posts = Posts;
        }

        public void UpdatePosts(List<UserPost> Posts)
        {
            this.Posts = Posts;
            NotifyDataSetChanged();
        }

        public void AddPosts(List<UserPost> Posts)
        {
            this.Posts.AddRange(Posts);
            NotifyDataSetChanged();
        }

        public UserPost GetItem(int position)
        {
             return Posts[position];
        }
        public override int ItemCount
        {
            get
            {
                return Posts.Count;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            FeedViewHolder vh = holder as FeedViewHolder;
            vh.Photo.SetImageResource(0);
            vh.Author.Text = Posts[position].Author;
            Picasso.With(context).Load(Posts[position].Body).Into(vh.Photo);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).
                    Inflate(Resource.Layout.lyt_feed_item, parent, false);
            FeedViewHolder vh = new FeedViewHolder(itemView);
            return vh;
        }

        public class FeedViewHolder : RecyclerView.ViewHolder
        {
            public ImageView Photo { get; private set; }
            public ImageView Image { get; private set; }
            public TextView Author { get; private set; }

            public FeedViewHolder(View itemView) : base(itemView)
            {
                Image = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.profile_image);
                Author = itemView.FindViewById<TextView>(Resource.Id.author_name);
                Photo = itemView.FindViewById<ImageView>(Resource.Id.photo);
            }
        }
    }
}