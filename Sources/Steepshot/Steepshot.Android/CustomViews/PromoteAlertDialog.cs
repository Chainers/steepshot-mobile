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
using Steepshot.Adapter;
using Steepshot.Core.Models.Enums;
using Steepshot.Holders;
using Steepshot.Core.Models.Common;
using System.Threading.Tasks;
using System.Globalization;
using Steepshot.Core.Models.Requests;
using System.Threading;

namespace Steepshot.CustomViews
{
    public sealed class PromoteAlertDialog : BottomSheetDialog
    {
        private const int PickerPage = 0;
        private const int MainPage = 1;
        private const int PromoterFoundPage = 2;
        private const int MessagesPage = 3;

        private readonly Context context;
        private readonly Post post;

        private PromotePagerAdapter _adapter;
        private BasePostPresenter _presenter;
        private PromotePickerHolder _pickerHolder;
        private PromoteMainHolder _mainHolder;
        private PromoterFoundHolder _foundHolder;

        private TextView _promoteTitle;
        private Button _actionBtn;
        private ProgressBar _actionSpinner;
        private Android.Support.V4.View.ViewPager _container;

        private Action<ActionType> _promoteAction;
        private string _actionButtonTitle;
        private int _currentPage = MainPage;

        private PromoteAlertDialog(Context context) : base(context) { }

        public PromoteAlertDialog(Context context, BasePostPresenter presenter, Post post) : this(context)
        {
            this.context = context;
            _presenter = presenter;
            this.post = post;
        }

        public override void Show()
        {
            using (var dialogView = LayoutInflater.From(context).Inflate(Resource.Layout.lyt_promote_popup, null))
            {
                dialogView.SetMinimumWidth((int)(context.Resources.DisplayMetrics.WidthPixels * 0.8));

                _promoteAction += PromoteAlertDialogAction;

                _promoteTitle = dialogView.FindViewById<TextView>(Resource.Id.promote_title);
                _promoteTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromotePost);
                _promoteTitle.Typeface = Style.Semibold;

                _container = dialogView.FindViewById<Android.Support.V4.View.ViewPager>(Resource.Id.promote_container);
                _adapter = new PromotePagerAdapter(context, _presenter, _promoteAction);
                _container.Adapter = _adapter;
                _container.SetCurrentItem(MainPage, false);
                _container.PageSelected += PageSelected;

                _actionBtn = dialogView.FindViewById<Button>(Resource.Id.findpromote_btn);
                _actionBtn.Typeface = Style.Semibold;
                _actionBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.FindPromoter);
                _actionBtn.Typeface = Style.Semibold;
                _actionBtn.Click += ActionButtonClick;

                _actionSpinner = dialogView.FindViewById<ProgressBar>(Resource.Id.promote_spinner);
                _actionSpinner.Visibility = ViewStates.Gone;

                var close = dialogView.FindViewById<Button>(Resource.Id.close);
                close.Typeface = Style.Semibold;
                close.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Close);
                close.Click += (sender, e) => { Cancel(); };

                SetContentView(dialogView);
                Window.FindViewById(Resource.Id.design_bottom_sheet).SetBackgroundColor(Color.Transparent);
                var dialogPadding = (int)Android.Util.TypedValue.ApplyDimension(Android.Util.ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics);
                Window.DecorView.SetPadding(dialogPadding, dialogPadding, dialogPadding, dialogPadding);
                base.Show();

                var bottomSheet = FindViewById<FrameLayout>(Resource.Id.design_bottom_sheet);
                BottomSheetBehavior.From(bottomSheet).State = BottomSheetBehavior.StateExpanded;
            }
        }

        private async void ActionButtonClick(object sender, EventArgs e)
        {
            _pickerHolder = _adapter.pickerHolder;
            _mainHolder = _adapter.mainHolder;
            _foundHolder = _adapter.foundHolder;

            switch (_currentPage)
            {
                case PickerPage:
                    _container.SetCurrentItem(_container.CurrentItem + 1, true);
                    var currencyType = _adapter.coins[_pickerHolder.selectedPosition];
                    _mainHolder.UpdateTokenInfo(currencyType);
                    break;
                case MainPage:
                    EnableActionBtn(false);
                    await FindPromoter();
                    EnableActionBtn(true);
                    break;
                case PromoterFoundPage:
                    _container.SetCurrentItem(_container.CurrentItem - 1, true);
                    break;
            }
        }

        private async Task FindPromoter()
        {
            if (!double.TryParse(_mainHolder.AmountEdit, NumberStyles.Any, CultureInfo.InvariantCulture, out var amountEdit))
                return;

            if (amountEdit > _mainHolder.balances?.Find(x => x.CurrencyType == _mainHolder.pickedCoin).Value)
            {
                _mainHolder.ShowError(AppSettings.LocalizationManager.GetText(LocalizationKeys.NotEnoughBalance));
                return;
            }

            if (string.IsNullOrEmpty(_mainHolder.AmountEdit) || !IsValidAmount(_mainHolder.AmountEdit))
            {
                return;
            }

            var pr = new PromoteRequest
            {
                Amount = amountEdit,
                CurrencyType = _mainHolder.pickedCoin,
                PostToPromote = post
            };

            var promoter = await _presenter.FindPromoteBot(pr);

            if (promoter.IsSuccess)
            {
                _foundHolder.UpdatePromoterInfo(promoter.Result);
                _container.SetCurrentItem(PromoterFoundPage, true);
            }
        }

        private bool IsValidAmount(string amount)
        {
            if (!double.TryParse(amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amountEdit))
                return false;

            return amountEdit >= Core.Constants.MinBid && amountEdit <= Core.Constants.MaxBid;
        }

        private void EnableActionBtn(bool enabled)
        {
            _actionBtn.Enabled = enabled;
            _actionBtn.Text = enabled ? _actionButtonTitle : string.Empty;
            _actionSpinner.Visibility = enabled ? ViewStates.Gone : ViewStates.Visible;
        }

        private void PageSelected(object sender, Android.Support.V4.View.ViewPager.PageSelectedEventArgs e)
        {
            switch (e.Position)
            { 
                case PickerPage:
                    _actionButtonTitle = AppSettings.LocalizationManager.GetText(LocalizationKeys.Select);
                    _promoteTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.SelectToken);
                    break;
                case MainPage:
                    _actionButtonTitle = AppSettings.LocalizationManager.GetText(LocalizationKeys.FindPromoter);
                    _promoteTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromotePost);
                    break;
                case PromoterFoundPage:
                    _actionButtonTitle = AppSettings.LocalizationManager.GetText(LocalizationKeys.Promote);
                    _promoteTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromoterFound);
                    break;
            }

            _actionBtn.Text = _actionButtonTitle;
            _currentPage = e.Position;
        }

        private void PromoteAlertDialogAction(ActionType type)
        {
            switch (type)
            {
                case ActionType.PickCoin:
                    _container.SetCurrentItem(PickerPage, true);
                    break;
            }
        }
    }
}
