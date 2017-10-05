using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Square.Picasso;
using Steepshot.Core.Presenters;


namespace Steepshot.Adapter
{
    public class VotersAdapter : RecyclerView.Adapter
    {
        public Action<int> Click;
        private readonly Context _context;
        private readonly VotersPresenter _presenter;
        public override int ItemCount => _presenter.Count;

        public VotersAdapter(Context context, VotersPresenter presenter)
        {
            _context = context;
            _presenter = presenter;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var user = _presenter[position];
            if (user == null)
                return;

            if (!string.IsNullOrEmpty(user.Name))
            {
                ((UsersSearchViewHolder)holder).Name.Visibility = ViewStates.Visible;
                ((UsersSearchViewHolder)holder).Name.Text = user.Name;
            }
            else
                ((UsersSearchViewHolder)holder).Name.Visibility = ViewStates.Gone;

            ((UsersSearchViewHolder)holder).Username.Text = user.Username;

            if (Math.Abs(user.Percent) > 0.01)
            {
                ((UsersSearchViewHolder)holder).Percent.Visibility = ViewStates.Visible;
                ((UsersSearchViewHolder)holder).Percent.Text = $"{user.Percent}%";
            }
            else
                ((UsersSearchViewHolder)holder).Percent.Visibility = ViewStates.Gone;

            if (!string.IsNullOrEmpty(user.ProfileImage))
            {
                try
                {
                    Picasso.With(_context).Load(user.ProfileImage).NoFade().Resize(100, 0).Into(((UsersSearchViewHolder)holder).Avatar);
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
}
