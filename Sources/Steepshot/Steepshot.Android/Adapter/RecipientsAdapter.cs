using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Refractored.Controls;
using Square.Picasso;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class RecipientsAdapter : FollowersAdapter
    {
        public Action<UserFriend> RecipientSelected;

        public RecipientsAdapter(Context context, ListPresenter<UserFriend> presenter) : base(context, presenter)
        {
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var recipientHolder = holder as RecipientViewHolder;

            if (recipientHolder == null || _presenter[position] == null)
                return;

            recipientHolder.UpdateData(_presenter[position]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch ((ViewType)viewType)
            {
                case ViewType.Loader:
                    var loaderView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.loading_item, parent, false);
                    var loaderVh = new LoaderViewHolder(loaderView);
                    return loaderVh;
                default:
                    var cellView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_recipients_item, parent, false);
                    var cellVh = new RecipientViewHolder(cellView, RecipientSelected);
                    return cellVh;
            }
        }
    }

    public class RecipientViewHolder : RecyclerView.ViewHolder
    {
        private readonly Action<UserFriend> _recipientSelected;
        private readonly FrameLayout _recipientRoot;
        private readonly CircleImageView _recipientAvatar;
        private readonly TextView _recipientLogin;
        private readonly TextView _recipientName;

        private UserFriend _profile;

        public RecipientViewHolder(View itemView, Action<UserFriend> recipientSelected) : base(itemView)
        {
            _recipientSelected = recipientSelected;
            _recipientRoot = itemView.FindViewById<FrameLayout>(Resource.Id.root_item);
            _recipientAvatar = itemView.FindViewById<CircleImageView>(Resource.Id.friend_avatar);
            _recipientLogin = itemView.FindViewById<TextView>(Resource.Id.username);
            _recipientName = itemView.FindViewById<TextView>(Resource.Id.name);

            _recipientLogin.Typeface = Style.Semibold;
            _recipientName.Typeface = Style.Light;

            _recipientRoot.Clickable = true;
            _recipientRoot.Click += ItemViewOnClick;
        }

        private void ItemViewOnClick(object sender, EventArgs e)
        {
            _recipientSelected?.Invoke(_profile);
        }

        public void UpdateData(UserFriend profile)
        {
            _profile = profile;
            _recipientAvatar.SetImageResource(Resource.Drawable.ic_holder);

            if (!string.IsNullOrEmpty(profile.Avatar))
            {
                Picasso.With(ItemView.Context)
                    .LoadWithProxy(profile.Avatar, _recipientAvatar.LayoutParameters.Width, _recipientAvatar.LayoutParameters.Height)
                    .Placeholder(Resource.Drawable.ic_holder)
                    .NoFade()
                    .Priority(Picasso.Priority.Normal)
                    .Into(_recipientAvatar, null, () =>
                         {
                             Picasso.With(ItemView.Context)
                                 .Load(profile.Avatar)
                                 .Placeholder(Resource.Drawable.ic_holder)
                                 .NoFade()
                                 .Priority(Picasso.Priority.Normal)
                                 .Into(_recipientAvatar);
                         });
            }

            _recipientLogin.Text = profile.Author;
            _recipientName.Text = profile.Name;
        }
    }
}