using System;
using System.Threading.Tasks;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Ditch.Core.JsonRpc;
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
        public Func<BalanceModel, Task<OperationResult<VoidResponse>>> Claim;
        private readonly BalanceModel _balance;
        private TextView _title;
        private ProgressBar _claimSpinner;
        private RelativeLayout _claimBtnContainer;
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
            using (var dialogView = (LinearLayout)LayoutInflater.From(Context).Inflate(Resource.Layout.lyt_claim_rewards, null))
            {
                dialogView.SetMinimumWidth((int)(Style.ScreenWidth * 0.8));

                _title = dialogView.FindViewById<TextView>(Resource.Id.title);
                _title.Typeface = Style.Light;
                _title.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.TimeToClaimRewards);

                var tokenOne = dialogView.FindViewById<TextView>(Resource.Id.token_one);
                tokenOne.Typeface = Style.Semibold;

                var tokenTwo = dialogView.FindViewById<TextView>(Resource.Id.token_two);
                tokenTwo.Typeface = Style.Semibold;

                var tokenThree = dialogView.FindViewById<TextView>(Resource.Id.token_three);
                tokenThree.Typeface = Style.Semibold;

                var tokenOneValue = dialogView.FindViewById<TextView>(Resource.Id.token_one_value);
                tokenOneValue.Typeface = Style.Semibold;
                tokenOneValue.Text = _balance.RewardSteem.ToBalanceValueString();

                var tokenTwoValue = dialogView.FindViewById<TextView>(Resource.Id.token_two_value);
                tokenTwoValue.Typeface = Style.Semibold;
                tokenTwoValue.Text = _balance.RewardSp.ToBalanceValueString();

                var tokenThreeValue = dialogView.FindViewById<TextView>(Resource.Id.token_three_value);
                tokenThreeValue.Typeface = Style.Semibold;
                tokenThreeValue.Text = _balance.RewardSbd.ToBalanceValueString();

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

                _claimBtnContainer = dialogView.FindViewById<RelativeLayout>(Resource.Id.claimBtnContainer);

                _claimBtn = dialogView.FindViewById<Button>(Resource.Id.claimBtn);
                _claimBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ClaimRewards);
                _claimBtn.Click += ClaimBtnOnClick;

                var closeBtn = dialogView.FindViewById<Button>(Resource.Id.close);
                closeBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Close);
                closeBtn.Click += CloseBtnOnClick;

                _claimSpinner = dialogView.FindViewById<ProgressBar>(Resource.Id.claim_spinner);

                LayoutTransition transition = new LayoutTransition();
                transition.SetAnimateParentHierarchy(false);
                dialogView.LayoutTransition = transition;

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
            var result = await Claim.Invoke(_balance);
            if (result.IsSuccess)
            {
                _title.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.RewardsClaimed);
                _claimBtnContainer.Visibility = ViewStates.Gone;
            }
            else if (IsShowing)
            {
                _claimBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ClaimRewards);
                _claimSpinner.Visibility = ViewStates.Gone;
                Context.ShowAlert(result);
            }
        }

        private void CloseBtnOnClick(object sender, EventArgs e)
        {
            Dismiss();
        }
    }
}