using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Models.Responses;


namespace Steepshot.Adapter
{
    public class UsersSearchAdapter : RecyclerView.Adapter
    {
        public List<UserSearchResult> Items;
        public Action<int> Click;
        private readonly Context _context;
        public override int ItemCount => Items.Count;

        public UsersSearchAdapter(Context context)
        {
            _context = context;
            Items = new List<UserSearchResult>();
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var user = Items[position];
            if (!string.IsNullOrEmpty(user.Name))
            {
                ((UsersSearchViewHolder)holder).Name.Visibility = ViewStates.Visible;
                ((UsersSearchViewHolder)holder).Name.Text = user.Name;
            }
            else
                ((UsersSearchViewHolder)holder).Name.Visibility = ViewStates.Gone;

            ((UsersSearchViewHolder)holder).Username.Text = user.Username;

            /*if (user.Percent != 0)
			{
				((UsersSearchViewHolder)holder).Percent.Visibility = ViewStates.Visible;
				((UsersSearchViewHolder)holder).Percent.Text = $"{user.Percent.ToString()}%";
			}
			else*/
            ((UsersSearchViewHolder)holder).Percent.Visibility = ViewStates.Gone;

            if (!string.IsNullOrEmpty(user.ProfileImage))
            {
                try
                {
                    Picasso.With(_context).Load(user.ProfileImage).NoFade().Resize(80, 0).Into(((UsersSearchViewHolder)holder).Avatar);
                }
                catch
                {
                    //TODO:KOA: Empty try{}catch
                }
            }
            else
            {
                ((UsersSearchViewHolder)holder).Avatar.SetImageResource(Resource.Drawable.ic_user_placeholder);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_users_search_item, parent, false);
            var vh = new UsersSearchViewHolder(itemView);
            itemView.Click += (sender, e) =>
            {
                Click?.Invoke(vh.AdapterPosition);
            };
            return vh;
        }
    }

    public class UsersSearchViewHolder : RecyclerView.ViewHolder
    {
        public ImageView Avatar { get; }
        public TextView Name { get; }
        public TextView Username { get; }
        public TextView Percent { get; }

        public UsersSearchViewHolder(View itemView) : base(itemView)
        {
            Name = itemView.FindViewById<TextView>(Resource.Id.name);
            Username = itemView.FindViewById<TextView>(Resource.Id.username);
            Percent = itemView.FindViewById<TextView>(Resource.Id.percent);
            Avatar = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.avatar);
            //Tag.Clickable = true;
            //Tag.Click += (sender, e) => Click?.Invoke(AdapterPosition);
        }
    }
}
