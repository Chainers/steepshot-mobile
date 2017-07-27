using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Square.Picasso;
using Steepshot.Core.Models.Responses;


namespace Steepshot.Adapter
{
	public class VotersAdapter : RecyclerView.Adapter
	{
		public List<VotersResult> Items = new List<VotersResult>();
		public Action<int> Click;
		private Context _context;
		public override int ItemCount => Items.Count;

		public VotersAdapter(Context context, List<VotersResult> items)
		{
			_context = context;
			Items = items;
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

			if (user.Percent != 0)
			{
				((UsersSearchViewHolder)holder).Percent.Visibility = ViewStates.Visible;
				((UsersSearchViewHolder)holder).Percent.Text = $"{user.Percent.ToString()}%";
			}
			else
				((UsersSearchViewHolder)holder).Percent.Visibility = ViewStates.Gone;

			if (!string.IsNullOrEmpty(user.ProfileImage))
			{
				try
				{
					Picasso.With(_context).Load(user.ProfileImage).NoFade().Resize(100, 0).Into(((UsersSearchViewHolder)holder).Avatar);
				}
				catch (Exception e)
				{
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
}
