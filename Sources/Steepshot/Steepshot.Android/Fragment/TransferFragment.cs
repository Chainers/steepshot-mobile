using System;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Base;
using Steepshot.Core.Presenters;
using Steepshot.CustomViews;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class TransferFragment : BaseFragmentWithPresenter<TransferPresenter>
    {
#pragma warning disable 0649, 4014
        [BindView(Resource.Id.recipient_name)] private TextView _recipientTitle;
        [BindView(Resource.Id.recipient_search)] private EditText _recipientSearch;
        [BindView(Resource.Id.transfer_amount)] private TextView _transferAmountTitle;
        [BindView(Resource.Id.transfer_amount_edit)] private EditText _transferAmountEdit;
        [BindView(Resource.Id.transfer_comment)] private TextView _transferCommentTitle;
        [BindView(Resource.Id.transfer_comment_edit)] private EditText _transferCommentEdit;
        [BindView(Resource.Id.transfer_coin)] private LinearLayout _transferCoinType;
        [BindView(Resource.Id.transfercoin_name)] private TextView _transferCoinName;
#pragma warning restore 0649

        private CoinPickDialog _coinPickDialog;
        private List<string> _coins;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_wallet, null);
                Cheeseknife.Bind(this, InflatedView);
            }

            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            _coins = new List<string>();
            _coins.AddRange(new[] { "Steem", "Steem dollars" });

            _recipientTitle.Typeface = Style.Semibold;
            _recipientSearch.Typeface = Style.Light;
            _transferAmountTitle.Typeface = Style.Semibold;
            _transferAmountEdit.Typeface = Style.Light;
            _transferCommentTitle.Typeface = Style.Semibold;
            _transferCommentEdit.Typeface = Style.Light;
            _transferCoinName.Typeface = Style.Semibold;

            _recipientTitle.Text = "Recipient name";
            _recipientSearch.Hint = "Enter recipient name";
            _transferAmountTitle.Text = "Transfer amount";
            _transferAmountEdit.Hint = "Enter transfer amount";
            _transferCommentTitle.Text = "Accompanying commentary";
            _transferCommentEdit.Hint = "Enter accompanying commentary";
            _transferCoinName.Text = _coins[0];

            _coinPickDialog = new CoinPickDialog(Activity, _coins);
            _coinPickDialog.Window.RequestFeature(WindowFeatures.NoTitle);
            _coinPickDialog.CoinSelected += CoinSelected;

            _transferCoinType.Click += TransferCoinTypeOnClick;
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
            GC.Collect(0);
        }

        private void TransferCoinTypeOnClick(object sender, EventArgs e)
        {
            _coinPickDialog.Show(_coins.IndexOf(_transferCoinName.Text));
        }

        private void CoinSelected(string pickedCoin)
        {
            _transferCoinName.Text = pickedCoin;
        }
    }
}