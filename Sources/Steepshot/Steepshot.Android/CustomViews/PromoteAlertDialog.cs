using System;
using System.Collections.Generic;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Extensions;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Android.Support.Design.Widget;
using Com.Aigestudio.Wheelpicker;
using Steepshot.Adapter;

namespace Steepshot.CustomViews
{
    public sealed class PromoteAlertDialog : BottomSheetDialog
    {
        private readonly Context context;

        private LinearLayout _mainPanel;
        private LinearLayout _notFoundPanel;
        private LinearLayout _counterPanel;

        private TextView _completePanel;
        private TextView _errorMessage;
        private TextView _balanceLabel;
        private ProgressBar _balanceLoader;
        private EditText _amountTextField;
        private Button _actionBtn;
        private Button _maxBtn;
        private LinearLayout _promoteCoin;
        private WheelPicker _coinPicker;

        private BasePostPresenter _presenter;

        private bool EditEnabled
        {
            set
            {
                _amountTextField.Enabled = value;
                _actionBtn.Enabled = value;
                _maxBtn.Enabled = value;
                _promoteCoin.Enabled = value;
            }
        }

        private PromoteAlertDialog(Context context) : base(context) { }

        public PromoteAlertDialog(Context context, BasePostPresenter presenter) : this(context)
        {
            this.context = context;
            _presenter = presenter;
        }

        public override void Show()
        {
            using (var dialogView = LayoutInflater.From(context).Inflate(Resource.Layout.lyt_promote_popup, null))
            {
                dialogView.SetMinimumWidth((int)(context.Resources.DisplayMetrics.WidthPixels * 0.8));

                var promoteTitle = dialogView.FindViewById<TextView>(Resource.Id.promote_title);
                promoteTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromotePost);
                promoteTitle.Typeface = Style.Semibold;

                var container = dialogView.FindViewById<Android.Support.V4.View.ViewPager>(Resource.Id.promote_container);
                container.Adapter = new PromotePagerAdapter(context, _presenter);
                container.SetCurrentItem(1, false);

                _actionBtn = dialogView.FindViewById<Button>(Resource.Id.findpromote_btn);
                _actionBtn.Typeface = Style.Semibold;
                _actionBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.FindPromoter);
                _actionBtn.Typeface = Style.Semibold;
                _actionBtn.Click += ActionButtonClick;

                var close = dialogView.FindViewById<Button>(Resource.Id.close);
                close.Typeface = Style.Semibold;
                close.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Close);
                close.Click += (sender, e) => { Cancel(); };

                //_mainPanel = dialogView.FindViewById<LinearLayout>(Resource.Id.mainpromote_lyt);
                //_notFoundPanel = dialogView.FindViewById<LinearLayout>(Resource.Id.notfound_lyt);
                //_counterPanel = dialogView.FindViewById<LinearLayout>(Resource.Id.counter_lyt);
                //_coinPicker = dialogView.FindViewById<WheelPicker>(Resource.Id.coin_picker);
                //_completePanel = dialogView.FindViewById<TextView>(Resource.Id.complete_promote);

                SetContentView(dialogView);
                Window.FindViewById(Resource.Id.design_bottom_sheet).SetBackgroundColor(Color.Transparent);
                var dialogPadding = (int)Android.Util.TypedValue.ApplyDimension(Android.Util.ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics);
                Window.DecorView.SetPadding(dialogPadding, dialogPadding, dialogPadding, dialogPadding);
                base.Show();

                var bottomSheet = FindViewById<FrameLayout>(Resource.Id.design_bottom_sheet);
                BottomSheetBehavior.From(bottomSheet).State = BottomSheetBehavior.StateExpanded;
            }
        }

        private void ActionButtonClick(object sender, EventArgs e)
        {
        }

        private void IsEnoughBalance(object sender, TextChangedEventArgs e)
        {
            _errorMessage.Visibility = ViewStates.Gone;

            if (string.IsNullOrEmpty(_amountTextField.Text))
                return;

            if (!double.TryParse(_amountTextField.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var amountEdit))
                return;

            if (amountEdit == 0)
                return;

            if (amountEdit < Core.Constants.MinBid)
            {
                _errorMessage.Visibility = ViewStates.Visible;
                _errorMessage.Text = $"{AppSettings.LocalizationManager.GetText(LocalizationKeys.MinBid)} {Core.Constants.MinBid}";
            }
            else if (amountEdit > Core.Constants.MaxBid)
            {
                _errorMessage.Visibility = ViewStates.Visible;
                _errorMessage.Text = $"{AppSettings.LocalizationManager.GetText(LocalizationKeys.MaxBid)} {Core.Constants.MaxBid}";
            }
        }
    }
}
