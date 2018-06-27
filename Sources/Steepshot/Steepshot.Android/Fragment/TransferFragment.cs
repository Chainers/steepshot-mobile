using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Refractored.Controls;
using Square.Picasso;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.CustomViews;
using Steepshot.Utils;
using ViewUtils = Steepshot.Utils.ViewUtils;

namespace Steepshot.Fragment
{
    public class TransferFragment : BaseFragmentWithPresenter<TransferPresenter>
    {
        private enum FragmentState
        {
            Search,
            Transfer,
            Comment
        }
#pragma warning disable 0649, 4014        
        [BindView(Resource.Id.arrow_back)] private ImageButton _backBtn;
        [BindView(Resource.Id.title)] private TextView _fragmentTitle;
        [BindView(Resource.Id.balance)] private TextView _balance;
        [BindView(Resource.Id.recipient_avatar)] private CircleImageView _recipientAvatar;
        [BindView(Resource.Id.recipient_name)] private TextView _recipientTitle;
        [BindView(Resource.Id.recipient_search)] private EditText _recipientSearch;
        [BindView(Resource.Id.search_clear)] private ImageView _recipientSearchClear;
        [BindView(Resource.Id.transfer_amount)] private TextView _transferAmountTitle;
        [BindView(Resource.Id.transfer_amount_edit)] private EditText _transferAmountEdit;
        [BindView(Resource.Id.transfer_comment)] private TextView _transferCommentTitle;
        [BindView(Resource.Id.amount_clear)] private ImageView _transferAmountClear;
        [BindView(Resource.Id.transfer_comment_edit)] private EditText _transferCommentEdit;
        [BindView(Resource.Id.transfer_coin)] private LinearLayout _transferCoinType;
        [BindView(Resource.Id.transfercoin_name)] private TextView _transferCoinName;
        [BindView(Resource.Id.transfer_recipient_container)] private LinearLayout _transferRecipientContainer;
        [BindView(Resource.Id.transfer_amount_container)] private LinearLayout _transferAmountContainer;
        [BindView(Resource.Id.transfer_comment_container)] private LinearLayout _transferCommentContainer;
        [BindView(Resource.Id.recipient_search_list)] private LinearLayout _recipientSearchList;
        [BindView(Resource.Id.users)] private RecyclerView _recipientsList;
        [BindView(Resource.Id.transfer_btn)] private Button _transferBtn;
        [BindView(Resource.Id.search_spinner)] private ProgressBar _recipientSearchLoader;
        [BindView(Resource.Id.empty_query_label)] private TextView _emptyQueryLabel;
#pragma warning restore 0649
        private ViewGroup _activityRoot;
        private CoinPickDialog _coinPickDialog;
        private List<CurrencyType> _coins;
        private CurrencyType _pickedCoin;
        private string _prevQuery = string.Empty;

        private RecipientsAdapter _recipientsAdapter;
        private RecipientsAdapter RecipientsAdapter =>
            _recipientsAdapter ?? (_recipientsAdapter = new RecipientsAdapter(Activity, Presenter));

        private FragmentState _state;
        private FragmentState State
        {
            get => _state;
            set
            {
                _state = value;
                switch (_state)
                {
                    case FragmentState.Search:
                        _recipientSearchList.Visibility = ViewStates.Visible;
                        _transferAmountContainer.Visibility = ViewStates.Gone;
                        _transferCommentContainer.Visibility = ViewStates.Gone;
                        _recipient = null;
                        _recipientAvatar.Visibility = ViewStates.Gone;
                        break;
                    case FragmentState.Transfer:
                        _recipientSearchList.Visibility = ViewStates.Gone;
                        _transferRecipientContainer.Visibility = ViewStates.Visible;
                        _transferAmountContainer.Visibility = ViewStates.Visible;
                        _transferCommentContainer.Visibility = ViewStates.Visible;
                        Recipient = Recipient ?? Presenter.FirstOrDefault(recipient => recipient.Author.Equals(_recipientSearch.Text));
                        break;
                    case FragmentState.Comment:
                        _recipientSearchList.Visibility = ViewStates.Gone;
                        _transferRecipientContainer.Visibility = ViewStates.Gone;
                        _transferAmountContainer.Visibility = ViewStates.Gone;
                        _transferCommentContainer.Visibility = ViewStates.Visible;
                        break;
                }
            }
        }

        private UserFriend _recipient;
        private UserFriend Recipient
        {
            get => _recipient;
            set
            {
                _recipient = value;
                if (_recipient != null)
                {
                    if (!string.IsNullOrEmpty(_recipient.Avatar))
                        Picasso.With(Activity)
                            .Load(_recipient.Avatar.GetProxy(_recipientAvatar.LayoutParameters.Width, _recipientAvatar.LayoutParameters.Height))
                            .Placeholder(Resource.Drawable.ic_holder)
                            .NoFade()
                            .Priority(Picasso.Priority.Normal)
                            .Into(_recipientAvatar, null, () =>
                            {
                                Picasso.With(Activity)
                                    .Load(_recipient.Avatar.GetProxy(_recipientAvatar.LayoutParameters.Width, _recipientAvatar.LayoutParameters.Height))
                                    .Placeholder(Resource.Drawable.ic_holder)
                                    .NoFade()
                                    .Priority(Picasso.Priority.Normal)
                                    .Into(_recipientAvatar);
                            });
                    else
                        Picasso.With(Activity).Load(Resource.Drawable.ic_holder).Into(_recipientAvatar);
                    _recipientAvatar.Visibility = ViewStates.Visible;
                }
                else
                {
                    _recipientAvatar.Visibility = ViewStates.Gone;
                }
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_transfer, null);
                Cheeseknife.Bind(this, InflatedView);
            }

            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);
            _activityRoot = Activity.FindViewById<ViewGroup>(Android.Resource.Id.Content);

            _coins = new List<CurrencyType>();
            switch (AppSettings.User.Chain)
            {
                case KnownChains.Steem:
                    _coins.AddRange(new[] { CurrencyType.Steem, CurrencyType.Sbd });
                    break;
                case KnownChains.Golos:
                    _coins.AddRange(new[] { CurrencyType.Golos, CurrencyType.Gbg });
                    break;
            }

            _fragmentTitle.Typeface = Style.Semibold;
            _balance.Typeface = Style.Semibold;
            _recipientTitle.Typeface = Style.Semibold;
            _recipientSearch.Typeface = Style.Light;
            _transferAmountTitle.Typeface = Style.Semibold;
            _transferAmountEdit.Typeface = Style.Light;
            _transferCommentTitle.Typeface = Style.Semibold;
            _transferCommentEdit.Typeface = Style.Light;
            _transferCoinName.Typeface = Style.Semibold;
            _emptyQueryLabel.Typeface = Style.Light;

            _fragmentTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Transfer);
            _recipientTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.RecipientName);
            _recipientSearch.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.RecipientNameHint);
            _transferAmountTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferAmount);
            _transferAmountEdit.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferAmountHint);
            _transferCommentTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferComment);
            _transferCommentEdit.Hint = AppSettings.LocalizationManager.GetText(LocalizationKeys.TranferCommentHint);
            _emptyQueryLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.EmptyQuery);
            _transferBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Transfer);
            _transferCoinName.Text = _coins[0].ToString();
            _balance.Text = $"{BasePresenter.ToFormatedCurrencyString(100)}$";

            _coinPickDialog = new CoinPickDialog(Activity, _coins);
            _coinPickDialog.Window.RequestFeature(WindowFeatures.NoTitle);
            _coinPickDialog.CoinSelected += CoinSelected;

            RecipientsAdapter.RecipientSelected += RecipientSelected;

            _recipientsList.SetLayoutManager(new LinearLayoutManager(Activity));
            _recipientsList.SetAdapter(RecipientsAdapter);

            _recipientSearch.TextChanged += RecipientSearchOnTextChanged;
            _recipientSearch.FocusChange += RecipientSearchOnFocusChange;
            _recipientSearchClear.Click += RecipientSearchClearOnClick;
            _transferAmountEdit.TextChanged += TransferAmountEditOnTextChanged;
            _transferAmountClear.Click += TransferAmountClearOnClick;
            _transferCoinType.Click += TransferCoinTypeOnClick;
            _transferCommentEdit.FocusChange += TransferCommentEditOnFocusChange;
            _transferBtn.Click += TransferBtnOnClick;
            _backBtn.Click += BackBtnOnClick;
            _activityRoot.ViewTreeObserver.GlobalLayout += OnKeyboardOpen;

            State = FragmentState.Transfer;
            Presenter.SourceChanged += PresenterOnSourceChanged;
        }

        public override void OnDetach()
        {
            _activityRoot.ViewTreeObserver.GlobalLayout -= OnKeyboardClose;
            _activityRoot.ViewTreeObserver.GlobalLayout -= OnKeyboardOpen;
            base.OnDetach();
            Cheeseknife.Reset(this);
            GC.Collect(0);
        }

        private void OnKeyboardOpen(object sender, EventArgs e)
        {
            var r = new Rect();
            _activityRoot.GetWindowVisibleDisplayFrame(r);
            var heightDiff = _activityRoot.RootView.Height - r.Height();
            if (heightDiff > ViewUtils.KeyboardVisibilityThreshold)
            {
                _activityRoot.ViewTreeObserver.GlobalLayout -= OnKeyboardOpen;
                _activityRoot.ViewTreeObserver.GlobalLayout += OnKeyboardClose;
            }
        }

        private void OnKeyboardClose(object sender, EventArgs e)
        {
            var r = new Rect();
            _activityRoot.GetWindowVisibleDisplayFrame(r);
            var heightDiff = _activityRoot.RootView.Height - r.Height();
            if (heightDiff < ViewUtils.KeyboardVisibilityThreshold)
            {
                _transferBtn.RequestFocus();
                _activityRoot.ViewTreeObserver.GlobalLayout -= OnKeyboardClose;
                _activityRoot.ViewTreeObserver.GlobalLayout += OnKeyboardOpen;
            }
        }

        private void PresenterOnSourceChanged(Status obj)
        {
            RecipientsAdapter.NotifyDataSetChanged();
            _recipientSearchLoader.Visibility = ViewStates.Gone;
            _emptyQueryLabel.Visibility = Presenter.Count == 0 ? ViewStates.Visible : ViewStates.Gone;
        }

        private void RecipientSearchOnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            State = e.HasFocus ? FragmentState.Search : FragmentState.Transfer;
        }

        private async void RecipientSearchOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var isEmpty = string.IsNullOrEmpty(_recipientSearch.Text);
            _recipientSearchClear.SetImageResource(isEmpty ? Resource.Drawable.ic_search_small : Resource.Drawable.ic_close_tag_active);
            if (!isEmpty && !_prevQuery.Equals(_recipientSearch.Text))
            {
                Presenter.Clear();
                _recipientSearchLoader.Visibility = ViewStates.Visible;
                _emptyQueryLabel.Visibility = ViewStates.Gone;
                var searchResult = await Presenter.TryLoadNextSearchUser(_recipientSearch.Text);
                if (searchResult == null)
                {
                    _prevQuery = _recipientSearch.Text;
                }
            }
        }

        private void RecipientSearchClearOnClick(object sender, EventArgs e)
        {
            _recipientSearch.Text = string.Empty;
            Recipient = null;
        }

        private void TransferAmountEditOnTextChanged(object sender, TextChangedEventArgs e)
        {
            _transferAmountClear.Visibility =
                string.IsNullOrEmpty(_transferAmountEdit.Text) ? ViewStates.Gone : ViewStates.Visible;
        }

        private void TransferAmountClearOnClick(object sender, EventArgs e)
        {
            _transferAmountEdit.Text = string.Empty;
        }

        private void TransferCoinTypeOnClick(object sender, EventArgs e)
        {
            _coinPickDialog.Show(_coins.IndexOf(_pickedCoin));
        }

        private void CoinSelected(CurrencyType pickedCoin)
        {
            _pickedCoin = pickedCoin;
            _transferCoinName.Text = _pickedCoin.ToString();
        }

        private void RecipientSelected(UserFriend recipient)
        {
            Recipient = recipient;
            _transferBtn.RequestFocus();
        }

        private void TransferCommentEditOnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            State = e.HasFocus ? FragmentState.Comment : FragmentState.Transfer;
        }

        private void TransferBtnOnClick(object sender, EventArgs e)
        {
            if (Recipient == null)
                return;

            if (string.IsNullOrEmpty(AppSettings.User.ActiveKey))
            {
                Activity.StartActivity(typeof(ActiveSignInActivity));
            }

            Presenter.TryTransfer(Recipient.Author, 0.0, _pickedCoin, _transferCommentEdit.Text);
        }

        private void BackBtnOnClick(object sender, EventArgs e)
        {
            ((BaseActivity)Activity).OnBackPressed();
        }

        public override bool OnBackPressed()
        {
            if (State == FragmentState.Search)
            {
                _transferBtn.RequestFocus();
                return true;
            }
            return base.OnBackPressed();
        }
    }
}