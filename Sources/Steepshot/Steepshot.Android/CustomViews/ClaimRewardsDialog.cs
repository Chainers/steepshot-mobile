using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Steepshot.Core;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.CustomViews
{
    public class ClaimRewardsDialog : BottomSheetDialog
    {
        public Func<BalanceModel, Task<Exception>> Claim;
        private readonly BalanceModel _balance;
        private ProgressBar _claimSpinner;
        private Button _claimBtn;

        public ClaimRewardsDialog(Context context, BalanceModel balance) : this(context)
        {
            _balance = balance;
        }

        private ClaimRewardsDialog(Context context) : base(context)
        {
        }

        public override void Show()
        {
            using (var dialogView = LayoutInflater.From(Context).Inflate(Resource.Layout.lyt_claim_rewards, null))
            {
                dialogView.SetMinimumWidth((int)(Context.Resources.DisplayMetrics.WidthPixels * 0.8));

                var title = dialogView.FindViewById<TextView>(Resource.Id.title);
                title.Typeface = Style.Light;
                title.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ClaimRewards);

                var tokenOne = dialogView.FindViewById<TextView>(Resource.Id.token_one);
                tokenOne.Typeface = Style.Semibold;

                var tokenTwo = dialogView.FindViewById<TextView>(Resource.Id.token_two);
                tokenTwo.Typeface = Style.Semibold;

                var tokenThree = dialogView.FindViewById<TextView>(Resource.Id.token_three);
                tokenThree.Typeface = Style.Semibold;

                var tokenOneValue = dialogView.FindViewById<TextView>(Resource.Id.token_one_value);
                tokenOneValue.Typeface = Style.Semibold;
                tokenOneValue.Text = _balance.RewardSteem.ToBalanceVaueString();

                var tokenTwoValue = dialogView.FindViewById<TextView>(Resource.Id.token_two_value);
                tokenTwoValue.Typeface = Style.Semibold;
                tokenTwoValue.Text = _balance.RewardSp.ToBalanceVaueString();

                var tokenThreeValue = dialogView.FindViewById<TextView>(Resource.Id.token_three_value);
                tokenThreeValue.Typeface = Style.Semibold;
                tokenThreeValue.Text = _balance.RewardSbd.ToBalanceVaueString();

                switch (_balance.UserInfo.Chain)
                {
                    case KnownChains.Steem:
                        tokenOne.Text = CurrencyType.Steem.ToString().ToUpper();
                        tokenTwo.Text = $"{CurrencyType.Steem.ToString()} Power".ToUpper();
                        tokenThree.Text = CurrencyType.Sbd.ToString().ToUpper();
                        break;
                    case KnownChains.Golos:
                        tokenOne.Text = CurrencyType.Golos.ToString().ToUpper();
                        tokenTwo.Text = $"{CurrencyType.Golos.ToString()} Power".ToUpper();
                        tokenThree.Text = CurrencyType.Gbg.ToString().ToUpper();
                        break;
                }

                _claimBtn = dialogView.FindViewById<Button>(Resource.Id.claimBtn);
                _claimBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ClaimRewards);
                _claimBtn.Click += ClaimBtnOnClick;

                var closeBtn = dialogView.FindViewById<Button>(Resource.Id.close);
                closeBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Close);
                closeBtn.Click += CloseBtnOnClick;

                _claimSpinner = dialogView.FindViewById<ProgressBar>(Resource.Id.claim_spinner);

                SetContentView(dialogView);
                Window.FindViewById(Resource.Id.design_bottom_sheet).SetBackgroundColor(Color.Transparent);
                var dialogPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics);
                Window.DecorView.SetPadding(dialogPadding, dialogPadding, dialogPadding, dialogPadding);
                base.Show();

                var bottomSheet = FindViewById<FrameLayout>(Resource.Id.design_bottom_sheet);
                BottomSheetBehavior.From(bottomSheet).State = BottomSheetBehavior.StateExpanded;
            }
        }

        private async void ClaimBtnOnClick(object sender, EventArgs e)
        {
            if (Claim == null)
                return;

            _claimBtn.Text = string.Empty;
            _claimSpinner.Visibility = ViewStates.Visible;
            var exception = await Claim.Invoke(_balance);
            if (exception == null)
            {
                Context.ShowAlert(LocalizationKeys.TransferSuccess);
                Dismiss();
            }
            else
            {
                _claimBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ClaimRewards);
                _claimSpinner.Visibility = ViewStates.Gone;
                Context.ShowAlert(exception);
            }
        }

        private void CloseBtnOnClick(object sender, EventArgs e)
        {
            Dismiss();
        }
    }
}