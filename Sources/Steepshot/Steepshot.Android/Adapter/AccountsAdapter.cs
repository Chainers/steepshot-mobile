using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class AccountsAdapter : RecyclerView.Adapter
    {
        public List<UserInfo> AccountsList;
        public Action<int> DeleteAccount;
        public Action<int> PickAccount;

        public override int ItemCount => AccountsList.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var account = AccountsList[position];
            if (account == null)
                return;
            var tHolder = (AccountViewHolder)holder;
            tHolder.CellText.Text = $"{account.Chain} {Localization.Messages.Account}";
            tHolder.CheckImage.SetImageResource(BasePresenter.Chain == account.Chain ? Resource.Drawable.@checked : Resource.Drawable.@unchecked);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_account_item, parent, false);
            var vh = new AccountViewHolder(itemView, PickAccount, DeleteAccount);
            return vh;
        }
    }

    public class AccountViewHolder : RecyclerView.ViewHolder
    {
        public TextView CellText { get; }
        public ImageView CheckImage { get; }
        private ImageButton DeleteAccountButton { get; }
        private RelativeLayout CellLayout { get; }

        public AccountViewHolder(View itemView, Action<int> pickAccount, Action<int> deleteAccount) : base(itemView)
        {
            CellText = itemView.FindViewById<TextView>(Resource.Id.cell_text);
            CheckImage = itemView.FindViewById<ImageView>(Resource.Id.pick_image);
            DeleteAccountButton = itemView.FindViewById<ImageButton>(Resource.Id.delete_btn);
            CellLayout = itemView.FindViewById<RelativeLayout>(Resource.Id.account_cell_layout);

            CellText.Typeface = Style.Semibold;

            DeleteAccountButton.Click += (sender, e) => deleteAccount?.Invoke(AdapterPosition);
            CellLayout.Click += (sender, e) => pickAccount?.Invoke(AdapterPosition);
        }
    }
}
