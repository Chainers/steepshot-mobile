using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Base;
using Steepshot.Core.Authorization;
using Steepshot.Core.Localization;
using Steepshot.Utils;
using Steepshot.Core.Utils;

namespace Steepshot.Adapter
{
    public sealed class AccountsAdapter : RecyclerView.Adapter
    {
        public List<UserInfo> AccountsList;
        public Action<UserInfo> DeleteAccount;
        public Action<UserInfo> PickAccount;

        public override int ItemCount => AccountsList.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var account = AccountsList[position];
            if (account == null)
                return;

            var tHolder = (AccountViewHolder)holder;
            tHolder.UpdateData(account);

        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_account_item, parent, false);
            var vh = new AccountViewHolder(itemView, PickAccount, DeleteAccount);
            return vh;
        }
    }

    public sealed class AccountViewHolder : RecyclerView.ViewHolder
    {
        private readonly TextView _cellText;
        private readonly ImageView _checkImage;
        private readonly ImageButton _deleteAccountButton;
        private readonly RelativeLayout _cellLayout;
        private readonly Action<UserInfo> _pickAccount;
        private readonly Action<UserInfo> _deleteAccount;
        private UserInfo _userInfo;

        public AccountViewHolder(View itemView, Action<UserInfo> pickAccount, Action<UserInfo> deleteAccount) : base(itemView)
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

        public void UpdateData(UserInfo userInfo)
        {
            _userInfo = userInfo;
            _cellText.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Account, userInfo.Chain);
            _checkImage.SetImageResource(AppSettings.MainChain == userInfo.Chain ? Resource.Drawable.ic_checked : Resource.Drawable.ic_unchecked);
        }

        private void OnCellLayoutOnClick(object sender, EventArgs e)
        {
            _pickAccount?.Invoke(_userInfo);
        }

        private void OnDeleteAccountButtonOnClick(object sender, EventArgs e)
        {
            _deleteAccount?.Invoke(_userInfo);
        }
    }
}
