using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Activity;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PowerUpDownFragment : BaseFragmentWithPresenter<TransferPresenter>
    {
        [BindView(Resource.Id.title)] private TextView _fragmentTitle;
        [BindView(Resource.Id.arrow_back)] private ImageButton _backBtn;
        [BindView(Resource.Id.token1)] private TextView _tokenOneTitle;
        [BindView(Resource.Id.token1_value)] private TextView _tokenOneValue;
        [BindView(Resource.Id.token2)] private TextView _tokenTwoTitle;
        [BindView(Resource.Id.token2_value)] private TextView _tokenTwoValue;
        [BindView(Resource.Id.amount_title)] private TextView _amountTitle;
        [BindView(Resource.Id.transfer_amount_edit)] private EditText _amountEdit;
        [BindView(Resource.Id.amount_limit)] private TextView _amountLimitMessage;
        [BindView(Resource.Id.transfercoin_name)] private TextView _tokenName;
        [BindView(Resource.Id.max)] private Button _maxBtn;
        [BindView(Resource.Id.powerBtn)] private Button _powerBtn;
        [BindView(Resource.Id.power_spinner)] private ProgressBar _powerBtnLoader;

        private readonly BalanceModel _balance;
        private readonly PowerAction _powerAction;
        private SpannableString _tokenValueOne;
        private SpannableString _tokenValueTwo;
        private double _powerAmount;

        public PowerUpDownFragment(BalanceModel balance, PowerAction action)
        {
            _balance = balance;
            _powerAction = action;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_power_up_down, null);
                Cheeseknife.Bind(this, InflatedView);
            }

            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);
            ToggleTabBar(true);

            Presenter.SetClient(_balance.UserInfo.Chain == KnownChains.Steem ? App.SteemClient : App.GolosClient);

            _fragmentTitle.Typeface = Style.Semibold;
            _tokenOneTitle.Typeface = Style.Semibold;
            _tokenOneValue.Typeface = Style.Semibold;
            _tokenTwoTitle.Typeface = Style.Semibold;
            _tokenTwoValue.Typeface = Style.Semibold;
            _amountTitle.Typeface = Style.Semibold;
            _maxBtn.Typeface = Style.Semibold;
            _powerBtn.Typeface = Style.Semibold;
            _tokenName.Typeface = Style.Semibold;
            _amountEdit.Typeface = Style.Semibold;
            _amountLimitMessage.Typeface = Style.Semibold;

            _fragmentTitle.Text = AppSettings.LocalizationManager.GetText(_powerAction == PowerAction.PowerUp ? LocalizationKeys.PowerUp : LocalizationKeys.PowerDown);
            _tokenOneTitle.Text = _balance.CurrencyType.ToString().ToUpper();
            _tokenName.Text = _balance.CurrencyType.ToString().ToUpper();
            _tokenTwoTitle.Text = $"{_balance.CurrencyType} power".ToUpper();
            _amountTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Amount);
            _amountEdit.Hint = "0";
            _maxBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Max);
            _powerBtn.Text = AppSettings.LocalizationManager.GetText(_powerAction == PowerAction.PowerUp ? LocalizationKeys.PowerUp : LocalizationKeys.PowerDown);
            _amountLimitMessage.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.AmountLimitFull);

            _amountEdit.SetFilters(new IInputFilter[] { new TransferAmountFilter(Int32.MaxValue, 3) });
            AmountEditOnTextChanged(null, null);

            _tokenOneValue.ViewTreeObserver.GlobalLayout += TokenValuesGlobalLayout;
            _tokenTwoValue.ViewTreeObserver.GlobalLayout += TokenValuesGlobalLayout;
            _tokenName.ViewTreeObserver.GlobalLayout += TokenLayedOut;
            _amountEdit.TextChanged += AmountEditOnTextChanged;
            _maxBtn.Click += MaxBtnOnClick;
            _powerBtn.Click += PowerBtnOnClick;
            _backBtn.Click += BackBtnOnClick;
        }

        private void TokenValuesGlobalLayout(object sender, EventArgs e)
        {
            if (!IsInitialized || IsDetached)
                return;

            var commonTextSize = (int)(Math.Min(_tokenOneValue.TextSize, _tokenTwoValue.TextSize) / Activity.Resources.DisplayMetrics.ScaledDensity);
            TextViewCompat.SetAutoSizeTextTypeUniformWithConfiguration(_tokenOneValue, 2, commonTextSize, 2, (int)AutoSizeTextType.Uniform);
            TextViewCompat.SetAutoSizeTextTypeUniformWithConfiguration(_tokenTwoValue, 2, commonTextSize, 2, (int)AutoSizeTextType.Uniform);
        }

        private void TokenLayedOut(object sender, EventArgs e)
        {
            _amountEdit.SetPadding(_amountEdit.PaddingLeft, _amountEdit.PaddingTop, ((View)_tokenName.Parent).Width, _amountEdit.PaddingBottom);
            _tokenName.ViewTreeObserver.GlobalLayout -= TokenLayedOut;
        }

        public override void OnDetach()
        {
            _tokenOneValue.ViewTreeObserver.GlobalLayout -= TokenValuesGlobalLayout;
            _tokenTwoValue.ViewTreeObserver.GlobalLayout -= TokenValuesGlobalLayout;
            base.OnDetach();
            Cheeseknife.Reset(this);
            GC.Collect(0);
        }

        private void UpdateTokenValues(string currTokenOne, string nextTokenOne, string currTokenTwo, string nextTokenTwo)
        {
            _tokenValueOne = new SpannableString($"{currTokenOne} > {nextTokenOne}");
            _tokenValueOne.SetSpan(new ForegroundColorSpan(Style.R151G155B158), 0, currTokenOne.Length + 2, SpanTypes.ExclusiveExclusive);
            _tokenValueOne.SetSpan(new ForegroundColorSpan(Style.R255G34B5), currTokenOne.Length + 2, _tokenValueOne.Length(), SpanTypes.ExclusiveExclusive);
            _tokenOneValue.SetText(_tokenValueOne, TextView.BufferType.Spannable);

            _tokenValueTwo = new SpannableString($"{currTokenTwo} > {nextTokenTwo}");
            _tokenValueTwo.SetSpan(new ForegroundColorSpan(Style.R151G155B158), 0, currTokenTwo.Length + 2, SpanTypes.ExclusiveExclusive);
            _tokenValueTwo.SetSpan(new ForegroundColorSpan(Style.R255G34B5), currTokenTwo.Length + 2, _tokenValueTwo.Length(), SpanTypes.ExclusiveExclusive);
            _tokenTwoValue.SetText(_tokenValueTwo, TextView.BufferType.Spannable);
        }


        private void AmountEditOnTextChanged(object sender, TextChangedEventArgs e)
        {
            _amountLimitMessage.Visibility = ViewStates.Gone;
            var amountAvailable = _balance.Value;
            var spAvailiable = _balance.EffectiveSp - (_powerAction == PowerAction.PowerDown ? _balance.DelegatedToMe + 5 : 0);

            if (string.IsNullOrEmpty(_amountEdit.Text))
            {
                UpdateTokenValues(amountAvailable.ToBalanceValueString(), amountAvailable.ToBalanceValueString(), spAvailiable.ToBalanceValueString(), spAvailiable.ToBalanceValueString());
                _powerAmount = -1;
                return;
            }

            if (double.TryParse(_amountEdit.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var amountEdit))
            {
                _powerAmount = amountEdit;
                if (_powerAction == PowerAction.PowerUp && amountEdit <= amountAvailable)
                {
                    UpdateTokenValues(_balance.Value.ToBalanceValueString(),
                        (amountAvailable - amountEdit).ToBalanceValueString(),
                        spAvailiable.ToBalanceValueString(),
                        (spAvailiable + amountEdit).ToBalanceValueString());
                }
                else if (_powerAction == PowerAction.PowerDown && amountEdit <= spAvailiable)
                {
                    UpdateTokenValues(_balance.Value.ToBalanceValueString(),
                        (amountAvailable + amountEdit).ToBalanceValueString(),
                        spAvailiable.ToBalanceValueString(),
                        (spAvailiable - amountEdit).ToBalanceValueString());
                }
                else
                {
                    UpdateTokenValues(_balance.Value.ToBalanceValueString(),
                        AppSettings.LocalizationManager.GetText(LocalizationKeys.AmountLimit),
                        spAvailiable.ToBalanceValueString(),
                        AppSettings.LocalizationManager.GetText(LocalizationKeys.AmountLimit));
                    _powerAmount = -1;
                    _amountLimitMessage.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.AmountLimitFull);
                    _amountLimitMessage.Visibility = ViewStates.Visible;
                }
            }
        }

        private void MaxBtnOnClick(object sender, EventArgs e)
        {
            _amountEdit.Text = _powerAction == PowerAction.PowerUp ? _balance.Value.ToBalanceValueString() : (_balance.EffectiveSp - _balance.DelegatedToMe - 5).ToBalanceValueString();
            _amountEdit.SetSelection(_amountEdit.Text.Length);
        }

        private void PowerBtnOnClick(object sender, EventArgs e)
        {
            if (_powerAmount <= 0)
                return;

            if (string.IsNullOrEmpty(_balance.UserInfo.ActiveKey))
            {
                var intent = new Intent(Activity, typeof(ActiveSignInActivity));
                intent.PutExtra(ActiveSignInActivity.ActiveSignInUserName, _balance.UserInfo.Login);
                intent.PutExtra(ActiveSignInActivity.ActiveSignInChain, (int)_balance.UserInfo.Chain);
                StartActivityForResult(intent, ActiveSignInActivity.ActiveKeyRequestCode);
                return;
            }

            DoPowerAction();
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode == ActiveSignInActivity.ActiveKeyRequestCode && resultCode == (int)Result.Ok)
            {
                DoPowerAction();
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        private async void DoPowerAction()
        {
            _powerBtnLoader.Visibility = ViewStates.Visible;
            _powerBtn.Text = string.Empty;

            var model = new BalanceModel(_powerAmount, _balance.MaxDecimals, _balance.CurrencyType)
            {
                UserInfo = _balance.UserInfo
            };

            var response = await Presenter.TryPowerUpOrDown(model, _powerAction);

            if (!IsInitialized || IsDetached)
                return;

            _powerBtnLoader.Visibility = ViewStates.Gone;
            _powerBtn.Text = AppSettings.LocalizationManager.GetText(_powerAction == PowerAction.PowerUp ? LocalizationKeys.PowerUp : LocalizationKeys.PowerDown);

            if (response.IsSuccess)
            {
                TargetFragment.OnActivityResult(WalletFragment.WalletFragmentPowerUpOrDownRequestCode, (int)Result.Ok, null);
                Activity.ShowAlert(LocalizationKeys.TransferSuccess, ToastLength.Short);
                BackBtnOnClick(null, null);
            }
            else
            {
                Activity.ShowAlert(response.Exception, ToastLength.Short);
            }
        }

        private void BackBtnOnClick(object sender, EventArgs e)
        {
            ((BaseActivity)Activity).HideKeyboard();
            ((BaseActivity)Activity).OnBackPressed();
        }
    }
}