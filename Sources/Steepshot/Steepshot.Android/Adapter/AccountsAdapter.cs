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
            tHolder.UpdateData(account.Chain);

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
        private readonly TextView _cellText;
        private readonly ImageView _checkImage;
        private readonly ImageButton _deleteAccountButton;
        private readonly RelativeLayout _cellLayout;
        private readonly Action<int> _pickAccount;
        private readonly Action<int> _deleteAccount;

        public AccountViewHolder(View itemView, Action<int> pickAccount, Action<int> deleteAccount) : base(itemView)
        {
            _pickAccount = pickAccount;
            _deleteAccount = deleteAccount;
            _cellText = itemView.FindViewById<TextView>(Resource.Id.cell_text);
            _checkImage = itemView.FindViewById<ImageView>(Resource.Id.pick_image);
            _deleteAccountButton = itemView.FindViewById<ImageButton>(Resource.Id.delete_btn);
            _cellLayout = itemView.FindViewById<RelativeLayout>(Resource.Id.account_cell_layout);

            _cellText.Typeface = Style.Semibold;

            _deleteAccountButton.Click += OnDeleteAccountButtonOnClick;
            _cellLayout.Click += OnCellLayoutOnClick;
        }

        public void UpdateData(KnownChains chains)
        {
            _cellText.Text = $"{chains} {Localization.Messages.Account}";
            _checkImage.SetImageResource(BasePresenter.Chain == chains ? Resource.Drawable.@checked : Resource.Drawable.@unchecked);
        }

        private void OnCellLayoutOnClick(object sender, EventArgs e)
        {
            _pickAccount?.Invoke(AdapterPosition);
        }

        private void OnDeleteAccountButtonOnClick(object sender, EventArgs e)
        {
            _deleteAccount?.Invoke(AdapterPosition);
        }
    }
}
