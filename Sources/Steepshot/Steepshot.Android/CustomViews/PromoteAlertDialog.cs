using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Localization;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Steepshot.Adapter;
using Steepshot.Core.Models.Common;
using System.Threading.Tasks;
using System.Globalization;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Activity;
using Steepshot.Base;
using Steepshot.Core;

namespace Steepshot.CustomViews
{
    public sealed class PromoteAlertDialog : BottomSheetDialog
    {
        private enum Pages
        {
            CoinPick,
            Main,
            Promoter,
            Messages
        }

        private readonly Action<AutoLinkType, string> _autoLinkAction;
        private readonly PromotePresenter _presenter;
        private readonly Post _post;

        private PromotePagerAdapter _adapter;
        private PromoteRequest _promoteRequest;
        private PromoteResponse _promoterResult;

        private TextView _promoteTitle;
        private Button _actionBtn;
        private ProgressBar _actionSpinner;
        private ViewPager _pager;

        private string _actionButtonTitle;

        private PromoteAlertDialog(Context context) : base(context) { }

        public PromoteAlertDialog(Context context, Post post, Action<AutoLinkType, string> autoLinkAction) : this(context)
        {
            _presenter = new PromotePresenter();
            _presenter.SetClient(AppSettings.User.Chain == KnownChains.Steem ? App.SteemClient : App.GolosClient);
            _post = post;
            _autoLinkAction = autoLinkAction;
            ShowEvent += OnShowEvent;
        }

        private async void OnShowEvent(object sender, EventArgs e)
        {
            var response = await _presenter.TryGetAccountInfoAsync(AppSettings.User.Login);
            if (response.IsSuccess)
            {
                _adapter.MainHolder.AccountInfo = response.Result;
            }
        }

        public override void Show()
        {
            using (var dialogView = LayoutInflater.From(Context).Inflate(Resource.Layout.lyt_promote_popup, null))
            {
                dialogView.SetMinimumWidth((int)(Context.Resources.DisplayMetrics.WidthPixels * 0.8));

                _promoteTitle = dialogView.FindViewById<TextView>(Resource.Id.promote_title);
                _promoteTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromotePost);
                _promoteTitle.Typeface = Style.Semibold;

                _pager = dialogView.FindViewById<ViewPager>(Resource.Id.promote_container);
                _adapter = new PromotePagerAdapter(Context);
                _pager.OffscreenPageLimit = 2;
                _pager.Adapter = _adapter;
                _pager.SetCurrentItem((int)Pages.Main, false);
                _pager.PageSelected += PageSelected;

                _actionBtn = dialogView.FindViewById<Button>(Resource.Id.findpromote_btn);
                _actionBtn.Text = _actionButtonTitle = AppSettings.LocalizationManager.GetText(LocalizationKeys.FindPromoter);
                _actionBtn.Typeface = Style.Semibold;
                _actionBtn.Click += ActionButtonClick;

                _actionSpinner = dialogView.FindViewById<ProgressBar>(Resource.Id.promote_spinner);
                _actionSpinner.Visibility = ViewStates.Gone;

                var close = dialogView.FindViewById<Button>(Resource.Id.close);
                close.Typeface = Style.Semibold;
                close.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Close);
                close.Click += CloseOnClick;

                SetContentView(dialogView);
                Window.FindViewById(Resource.Id.design_bottom_sheet).SetBackgroundColor(Color.Transparent);
                var dialogPadding = (int)Android.Util.TypedValue.ApplyDimension(Android.Util.ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics);
                Window.DecorView.SetPadding(dialogPadding, dialogPadding, dialogPadding, dialogPadding);
                base.Show();

                var bottomSheet = FindViewById<FrameLayout>(Resource.Id.design_bottom_sheet);
                var behavior = BottomSheetBehavior.From(bottomSheet);
                behavior.State = BottomSheetBehavior.StateExpanded;
                behavior.SetBottomSheetCallback(new CustomBottomSheetCallback());
            }
        }

        private void CloseOnClick(object sender, EventArgs e)
        {
            Cancel();
        }

        private async void ActionButtonClick(object sender, EventArgs e)
        {
            switch ((Pages)_pager.CurrentItem)
            {
                case Pages.CoinPick:
                    _pager.SetCurrentItem((int)Pages.Main, true);
                    var currencyType = _adapter.Coins[_adapter.PickerHolder.SelectedPosition];
                    _adapter.MainHolder.UpdateTokenInfo(currencyType);
                    break;
                case Pages.Main:
                    EnableActionBtn(false);
                    await FindPromoter();
                    EnableActionBtn(true);
                    break;
                case Pages.Promoter:
                    if (!AppSettings.User.HasActivePermission)
                    {
                        var intent = new Intent(Context, typeof(ActiveSignInActivity));
                        intent.PutExtra(ActiveSignInActivity.ActiveSignInUserName, AppSettings.User.Login);
                        intent.PutExtra(ActiveSignInActivity.ActiveSignInChain, (int)AppSettings.User.Chain);
                        Context.StartActivity(intent);
                        return;
                    }
                    var promoteConfirmation = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromoteConfirmation, _promoteRequest.Amount, _promoteRequest.CurrencyType, _promoterResult.Bot.Author);
                    var actionAlert = new ActionAlertDialog(Context, promoteConfirmation,
                                                            AppSettings.LocalizationManager.GetText(string.Empty),
                                                            AppSettings.LocalizationManager.GetText(LocalizationKeys.Yes),
                                                            AppSettings.LocalizationManager.GetText(LocalizationKeys.No), _autoLinkAction, Orientation.Vertical);
                    actionAlert.AlertAction += async () =>
                    {
                        EnableActionBtn(false);
                        await LaunchPromoCampaign();
                        EnableActionBtn(true);
                    };
                    actionAlert.Show();
                    break;
                case Pages.Messages:
                    _pager.SetCurrentItem((int)Pages.Main, false);
                    break;
            }
        }

        private async Task FindPromoter()
        {
            var mainHolder = _adapter.MainHolder;

            if (string.IsNullOrEmpty(mainHolder.AmountEdit))
            {
                _adapter.MainHolder.ShowError($"{AppSettings.LocalizationManager.GetText(LocalizationKeys.MinBid)} {Constants.MinBid}");
                return;
            }

            if (!double.TryParse(mainHolder.AmountEdit, NumberStyles.Any, CultureInfo.InvariantCulture, out var amountEdit))
                return;

            if (amountEdit > mainHolder.Balances?.Find(x => x.CurrencyType == mainHolder.PickedCoin).Value)
            {
                mainHolder.ShowError(AppSettings.LocalizationManager.GetText(LocalizationKeys.NotEnoughBalance));
                return;
            }

            if (string.IsNullOrEmpty(mainHolder.AmountEdit) || !IsValidAmount(mainHolder.AmountEdit))
                return;

            _promoteRequest = new PromoteRequest
            {
                Amount = amountEdit,
                CurrencyType = mainHolder.PickedCoin,
                PostToPromote = _post
            };

            var promoter = await _presenter.FindPromoteBotAsync(_promoteRequest);

            if (promoter.IsSuccess)
            {
                _promoterResult = promoter.Result;
                _adapter.FoundHolder.UpdatePromoterInfo(_promoterResult);
                _pager.SetCurrentItem((int)Pages.Promoter, true);
            }
            else
            {
                _adapter.MessageHolder.SetupMessage(AppSettings.LocalizationManager.GetText(LocalizationKeys.PromoterNotFound), string.Empty);
                ShowResultMessage(AppSettings.LocalizationManager.GetText(LocalizationKeys.PromoterSearchResult), AppSettings.LocalizationManager.GetText(LocalizationKeys.SearchAgain), false);
            }
        }

        private async Task LaunchPromoCampaign()
        {
            var transferResponse = await _presenter.TryTransferAsync(AppSettings.User.UserInfo, _promoterResult.Bot.Author,
                                                                _promoteRequest.Amount.ToString(CultureInfo.InvariantCulture),
                                                                _promoteRequest.CurrencyType,
                                                                $"https://steemit.com{_post.Url}");

            _adapter.MessageHolder.SetupMessage(
                transferResponse.IsSuccess
                    ? AppSettings.LocalizationManager.GetText(LocalizationKeys.SuccessPromote)
                    : AppSettings.LocalizationManager.GetText(LocalizationKeys.TokenTransferError), string.Empty);

            ShowResultMessage(AppSettings.LocalizationManager.GetText(LocalizationKeys.PromoteComplete), AppSettings.LocalizationManager.GetText(LocalizationKeys.PromoteAgain), true);
        }

        private void ShowResultMessage(string viewTitle, string btnTitle, bool animate)
        {
            _actionButtonTitle = btnTitle;
            _promoteTitle.Text = viewTitle;
            _pager.SetCurrentItem((int)Pages.Messages, animate);
        }

        private bool IsValidAmount(string amount)
        {
            if (!double.TryParse(amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amountEdit))
                return false;

            return amountEdit >= Constants.MinBid && amountEdit <= Constants.MaxBid;
        }

        private void EnableActionBtn(bool enabled)
        {
            _actionBtn.Enabled = enabled;
            _actionBtn.Text = enabled ? _actionButtonTitle : string.Empty;
            _actionSpinner.Visibility = enabled ? ViewStates.Gone : ViewStates.Visible;
        }

        private void PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            switch ((Pages)e.Position)
            {
                case Pages.CoinPick:
                    _actionButtonTitle = AppSettings.LocalizationManager.GetText(LocalizationKeys.Select);
                    _promoteTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.SelectToken);
                    break;
                case Pages.Main:
                    _actionButtonTitle = AppSettings.LocalizationManager.GetText(LocalizationKeys.FindPromoter);
                    _promoteTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromotePost);
                    break;
                case Pages.Promoter:
                    _actionButtonTitle = AppSettings.LocalizationManager.GetText(LocalizationKeys.Promote);
                    _promoteTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromoterFound);
                    break;
            }

            _actionBtn.Text = _actionButtonTitle;
        }
    }
}
