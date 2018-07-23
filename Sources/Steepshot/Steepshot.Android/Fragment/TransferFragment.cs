using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
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
using Steepshot.Core.Facades;
using Steepshot.Core.Localization;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;
using Steepshot.CustomViews;
using Steepshot.Utils;
using ViewUtils = Steepshot.Utils.ViewUtils;

namespace Steepshot.Fragment
{
    public class TransferFragment : BaseFragment
    {
        private enum FragmentState
        {
            Search,
            TransferPrepare,
            Transfer,
            Comment,
            Cancel
        }
#pragma warning disable 0649, 4014        
        [BindView(Resource.Id.transfer_details)] private ScrollView _transferDetailsScroll;
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
        [BindView(Resource.Id.transfer_details_container)] private LinearLayout _transferDetailsContainer;
        [BindView(Resource.Id.recipient_search_list)] private LinearLayout _recipientSearchList;
        [BindView(Resource.Id.users)] private RecyclerView _recipientsList;
        [BindView(Resource.Id.transfer_btn)] private Button _transferBtn;
        [BindView(Resource.Id.search_spinner)] private ProgressBar _recipientSearchLoader;
        [BindView(Resource.Id.balance_spinner)] private ProgressBar _balanceLoader;
        [BindView(Resource.Id.transfer_spinner)] private ProgressBar _transferLoader;
        [BindView(Resource.Id.empty_query_label)] private TextView _emptyQueryLabel;
#pragma warning restore 0649

        private const int TransferFragmentRequestCode = 229;
        private readonly TransferFacade _transferFacade;
        private ViewGroup _activityRoot;
        private GradientDrawable _commentShape;
        private CoinPickDialog _coinPickDialog;
        private List<CurrencyType> _coins;
        private CurrencyType _pickedCoin;
        private string _prevQuery = string.Empty;

        private RecipientsAdapter _recipientsAdapter;
        private RecipientsAdapter RecipientsAdapter => _recipientsAdapter ?? (_recipientsAdapter = new RecipientsAdapter(Activity, _transferFacade.UserFriendPresenter));

        private FragmentState _state;
        private FragmentState State
        {
            get => _state;
            set
            {
                _state = value;
                OnFragmentStateChanged();
            }
        }

        private bool EditEnabled
        {
            set
            {
                _recipientSearch.Enabled = value;
                _transferAmountEdit.Enabled = value;
                _transferCommentEdit.Enabled = value;
                _transferBtn.Enabled = value;
            }
        }

        private int LytHeightDiff
        {
            get
            {
                var r = new Rect();
                _activityRoot.GetWindowVisibleDisplayFrame(r);
                var heightDiff = _activityRoot.RootView.Height - r.Height();
                return heightDiff;
            }
        }
        private bool IsKeyboardOpening => LytHeightDiff > ViewUtils.KeyboardVisibilityThreshold;

        public TransferFragment()
        {
            var client = App.MainChain == KnownChains.Steem ? App.SteemClient : App.GolosClient;
            _transferFacade = new TransferFacade();
            _transferFacade.SetClient(client);
            _transferFacade.OnRecipientChanged += OnRecipientChanged;
            _transferFacade.OnUserBalanceChanged += OnUserBalanceChanged;
        }

        public TransferFragment(UserProfileResponse user) : this()
        {
            _transferFacade.Recipient = new UserFriend()
            {
                Author = user.Username,
                Avatar = user.ProfileImage
            };
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

        public async override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);
            ToggleTabBar(true);
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

            _recipientSearch.SetFilters(new IInputFilter[] { new TextInputFilter(TextInputFilter.TagFilter) });
            _commentShape = new GradientDrawable();
            _commentShape.SetCornerRadius(BitmapUtils.DpToPixel(20, Resources));
            _commentShape.SetColor(Style.R244G244B246);
            _commentShape.SetStroke((int)BitmapUtils.DpToPixel(1, Resources), Style.R244G244B246);
            _transferCommentEdit.Background = _commentShape;
            _transferCommentEdit.TextChanged += TransferCommentEditOnTextChanged;

            _coinPickDialog = new CoinPickDialog(Activity, _coins);
            _coinPickDialog.Window.RequestFeature(WindowFeatures.NoTitle);
            _coinPickDialog.CoinSelected += CoinSelected;
            CoinSelected(_coins[0]);

            RecipientsAdapter.RecipientSelected += RecipientSelected;

            _recipientsList.SetLayoutManager(new LinearLayoutManager(Activity));
            var scrollListener = new ScrollListener();
            scrollListener.ScrolledToBottom += ScrollListenerOnScrolledToBottom;
            _recipientsList.AddOnScrollListener(scrollListener);
            _recipientsList.SetAdapter(RecipientsAdapter);

            _recipientSearch.TextChanged += RecipientSearchOnTextChanged;
            _recipientSearch.FocusChange += RecipientSearchOnFocusChange;
            _recipientSearch.KeyPress += RecipientSearchOnKeyPress;
            _recipientSearchClear.Click += RecipientSearchClearOnClick;
            _transferAmountEdit.TextChanged += TransferAmountEditOnTextChanged;
            _transferAmountClear.Click += TransferAmountClearOnClick;
            _transferCoinType.Click += TransferCoinTypeOnClick;
            _transferCommentTitle.Click += TransferCommentTitleOnClick;
            _transferCommentEdit.FocusChange += TransferCommentEditOnFocusChange;
            _transferBtn.Click += TransferBtnOnClick;
            _transferBtn.FocusChange += TransferBtnOnFocusChange;
            _backBtn.Click += BackBtnOnClick;
            _activityRoot.ViewTreeObserver.GlobalLayout += OnKeyboardOpening;

            State = FragmentState.TransferPrepare;
            _transferFacade.UserFriendPresenter.SourceChanged += PresenterOnSourceChanged;

            await UpdateAccountInfo();
            if (_transferFacade.Recipient != null)
            {
                _recipientSearch.Text = _transferFacade.Recipient.Author;
                OnRecipientChanged();
            }
        }

        public override void OnDetach()
        {
            _activityRoot.ViewTreeObserver.GlobalLayout -= OnKeyboardClosing;
            _activityRoot.ViewTreeObserver.GlobalLayout -= OnKeyboardOpening;
            _transferFacade.TasksCancel();
            base.OnDetach();
            Cheeseknife.Reset(this);
            GC.Collect(0);
        }

        private void OnKeyboardOpening(object sender, EventArgs e)
        {
            if (IsKeyboardOpening)
            {
                if (_transferCommentEdit.HasFocus)
                    State = FragmentState.Comment;
                _activityRoot.ViewTreeObserver.GlobalLayout -= OnKeyboardOpening;
                _activityRoot.ViewTreeObserver.GlobalLayout += OnKeyboardClosing;
            }
        }

        private void OnKeyboardClosing(object sender, EventArgs e)
        {
            if (!IsKeyboardOpening)
            {
                _activityRoot.ViewTreeObserver.GlobalLayout -= OnKeyboardClosing;
                _activityRoot.ViewTreeObserver.GlobalLayout += OnKeyboardOpening;
            }
        }

        private void ScrollListenerOnScrolledToBottom()
        {
            _transferFacade.TryLoadNextSearchUser(_prevQuery);
        }

        private void PresenterOnSourceChanged(Status obj)
        {
            RecipientsAdapter.NotifyDataSetChanged();
            _recipientSearchLoader.Visibility = ViewStates.Gone;
            _emptyQueryLabel.Visibility = _transferFacade.UserFriendPresenter.Count == 0 ? ViewStates.Visible : ViewStates.Gone;
            if (State == FragmentState.TransferPrepare)
                _transferFacade.Recipient = _transferFacade.Recipient ?? _transferFacade.UserFriendPresenter.FirstOrDefault(recipient => recipient.Author.Equals(_recipientSearch.Text));
        }

        private void OnFragmentStateChanged()
        {
            _transferBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Transfer);
            _emptyQueryLabel.Visibility = ViewStates.Gone;
            _transferLoader.Visibility = ViewStates.Gone;
            EditEnabled = true;
            switch (_state)
            {
                case FragmentState.Search:
                    _recipientSearchList.Visibility = ViewStates.Visible;
                    _transferDetailsContainer.Visibility = ViewStates.Gone;
                    _transferFacade.Recipient = null;
                    _recipientAvatar.Visibility = ViewStates.Gone;
                    break;
                case FragmentState.TransferPrepare:
                    _recipientSearchList.Visibility = ViewStates.Gone;
                    _transferDetailsContainer.Visibility = ViewStates.Visible;
                    _transferFacade.Recipient = _transferFacade.Recipient ?? _transferFacade.UserFriendPresenter.FirstOrDefault(recipient => recipient.Author.Equals(_recipientSearch.Text, StringComparison.OrdinalIgnoreCase));
                    break;
                case FragmentState.Comment:
                    _recipientSearchList.Visibility = ViewStates.Gone;
                    _transferDetailsScroll.FullScroll(FocusSearchDirection.Down);
                    break;
                case FragmentState.Transfer:
                    _transferBtn.Text = string.Empty;
                    _transferLoader.Visibility = ViewStates.Visible;
                    _recipientSearchList.Visibility = ViewStates.Gone;
                    _transferDetailsContainer.Visibility = ViewStates.Visible;
                    EditEnabled = false;
                    break;
                case FragmentState.Cancel:
                    if (IsKeyboardOpening)
                        ((BaseActivity)Activity).HideKeyboard();
                    break;
            }
        }

        private void OnUserBalanceChanged()
        {
            if (_transferFacade.UserBalance != null)
                _balance.Text = $"{_transferFacade.UserBalance.Value} {_pickedCoin.ToString()}";
        }

        private void OnRecipientChanged()
        {
            if (!IsInitialized)
                return;

            if (_transferFacade.Recipient != null)
            {
                if (!string.IsNullOrEmpty(_transferFacade.Recipient.Avatar))
                    Picasso.With(Activity)
                        .Load(_transferFacade.Recipient.Avatar.GetProxy(_recipientAvatar.LayoutParameters.Width, _recipientAvatar.LayoutParameters.Height))
                        .Placeholder(Resource.Drawable.ic_holder)
                        .NoFade()
                        .Priority(Picasso.Priority.Normal)
                        .Into(_recipientAvatar, null, () =>
                        {
                            Picasso.With(Activity)
                                .Load(_transferFacade.Recipient.Avatar.GetProxy(_recipientAvatar.LayoutParameters.Width, _recipientAvatar.LayoutParameters.Height))
                                .Placeholder(Resource.Drawable.ic_holder)
                                .NoFade()
                                .Priority(Picasso.Priority.Normal)
                                .Into(_recipientAvatar);
                        });
                else
                    Picasso.With(Activity).Load(Resource.Drawable.ic_holder).Into(_recipientAvatar);
                _recipientAvatar.Visibility = ViewStates.Visible;

                Activity.RunOnUiThread(() =>
                {
                    _recipientSearch.Text = _transferFacade.Recipient.Author;
                });
            }
            else
            {
                _recipientAvatar.Visibility = ViewStates.Gone;
            }
        }

        private void RecipientSearchOnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
                State = FragmentState.Search;
        }

        private async void RecipientSearchOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_recipientSearch.Text == _transferFacade?.Recipient?.Author)
                return;

            var isEmpty = string.IsNullOrEmpty(_recipientSearch.Text);
            _recipientSearchClear.SetImageResource(isEmpty ? Resource.Drawable.ic_search_small : Resource.Drawable.ic_close_tag_active);
            if (!isEmpty && _recipientSearch.Text.Length > 2 && !_prevQuery.Equals(_recipientSearch.Text))
            {
                _transferFacade.UserFriendPresenter.Clear();
                _recipientSearchLoader.Visibility = ViewStates.Visible;
                _emptyQueryLabel.Visibility = ViewStates.Gone;
                var searchResult = await _transferFacade.TryLoadNextSearchUser(_recipientSearch.Text);
                if (searchResult == null)
                {
                    _prevQuery = _recipientSearch.Text;
                }
            }
        }

        private void RecipientSearchOnKeyPress(object sender, View.KeyEventArgs e)
        {
            if (e.Event != null && (e.KeyCode == Keycode.Enter))
            {
                _transferBtn.RequestFocus();
                e.Handled = true;
                return;
            }
            e.Handled = false;
        }

        private void RecipientSearchClearOnClick(object sender, EventArgs e)
        {
            _prevQuery = string.Empty;
            _recipientSearch.Text = string.Empty;
            _transferFacade.Recipient = null;
            _transferFacade.UserFriendPresenter.Clear();
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

        private async Task UpdateAccountInfo()
        {
            _balance.Visibility = ViewStates.Gone;
            _balanceLoader.Visibility = ViewStates.Visible;
            var response = await _transferFacade.TryGetAccountInfo(AppSettings.User.Login);
            if (response.IsSuccess)
            {
                AppSettings.User.AccountInfo = response.Result;
                _transferFacade.UserBalance = AppSettings.User.AccountInfo?.Balances?.Find(x => x.CurrencyType == _pickedCoin);
            }
            _balance.Visibility = ViewStates.Visible;
            _balanceLoader.Visibility = ViewStates.Gone;
        }

        private void CoinSelected(CurrencyType pickedCoin)
        {
            _pickedCoin = pickedCoin;
            _transferCoinName.Text = _pickedCoin.ToString();
            _transferFacade.UserBalance = AppSettings.User.AccountInfo?.Balances?.Find(x => x.CurrencyType == _pickedCoin);
            switch (pickedCoin)
            {
                case CurrencyType.Steem:
                case CurrencyType.Golos:
                    _transferAmountEdit.SetFilters(new IInputFilter[] { new TransferAmountFilter(20, 3) });
                    break;
                case CurrencyType.Sbd:
                case CurrencyType.Gbg:
                    _transferAmountEdit.SetFilters(new IInputFilter[] { new TransferAmountFilter(20, 6) });
                    break;
            }
        }

        private void RecipientSelected(UserFriend recipient)
        {
            _transferFacade.Recipient = recipient;
            _transferBtn.RequestFocus();
        }

        private void TransferCommentTitleOnClick(object sender, EventArgs e)
        {
            _transferCommentEdit.Visibility = _transferCommentEdit.Visibility == ViewStates.Visible
                ? ViewStates.Gone
                : ViewStates.Visible;
            _transferBtn.RequestFocus();
            _transferCommentTitle.SetTextColor(Color.Black);
            _transferCommentTitle.Click -= TransferCommentTitleOnClick;
        }

        private void TransferCommentEditOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_transferCommentEdit.LineCount <= 2)
            {
                _commentShape.SetCornerRadius(BitmapUtils.DpToPixel(20, Resources) / _transferCommentEdit.LineCount);
                _transferCommentEdit.Background = _commentShape;
            }
        }

        private void TransferCommentEditOnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                State = FragmentState.Comment;
                _commentShape.SetColor(Color.White);
                _commentShape.SetStroke((int)BitmapUtils.DpToPixel(1, Resources), Style.R255G34B5);
            }
            else
            {
                _commentShape.SetColor(Style.R244G244B246);
                _commentShape.SetStroke((int)BitmapUtils.DpToPixel(1, Resources), Style.R244G244B246);
            }

            _transferCommentEdit.Background = _commentShape;
        }

        private void TransferBtnOnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                State = FragmentState.TransferPrepare;
                ((BaseActivity)Activity).HideKeyboard();
            }

        }

        private async Task<bool> Validate()
        {
            if (_transferFacade.Recipient == null)
            {
                Toast.MakeText(Activity, AppSettings.LocalizationManager.GetText(LocalizationKeys.WrongRecipientName), ToastLength.Short).Show();
                return false;
            }

            if (_transferFacade.UserBalance == null)
            {
                await _transferFacade.TryGetAccountInfo(AppSettings.User.Login);
                return await Validate();
            }

            var transferAmount = double.Parse(_transferAmountEdit.Text, CultureInfo.InvariantCulture);

            if (Math.Abs(transferAmount) < 0.0000001 || transferAmount > double.Parse(_transferFacade.UserBalance.Value, CultureInfo.InvariantCulture))
            {
                Toast.MakeText(Activity, AppSettings.LocalizationManager.GetText(LocalizationKeys.WrongTransferAmount), ToastLength.Short).Show();
                return false;
            }

            if (string.IsNullOrEmpty(AppSettings.User.ActiveKey))
            {
                var intent = new Intent(Activity, typeof(ActiveSignInActivity));
                StartActivityForResult(intent, TransferFragmentRequestCode);
                return false;
            }

            return true;
        }

        private async void TransferBtnOnClick(object sender, EventArgs e)
        {
            State = FragmentState.Transfer;

            if (!await Validate())
            {
                State = FragmentState.TransferPrepare;
                return;
            }

            Transfer();
        }

        private async void Transfer()
        {
            if (_transferFacade.UserBalance == null)
                return;

            var transferResponse = await _transferFacade.TransferPresenter.TryTransfer(_transferFacade.Recipient.Author, _transferAmountEdit.Text, _pickedCoin, _transferCommentEdit.Text);
            if (transferResponse.IsSuccess)
            {
                var succes = new SuccessfullTrxDialog(Activity, _transferFacade.Recipient.Author, $"{_transferAmountEdit.Text} {_pickedCoin.ToString().ToUpper()}");
                succes.Show();
                ClearEdits();
            }
            else
            {
                Toast.MakeText(Activity, transferResponse.Error.Message, ToastLength.Short).Show();
            }
            State = FragmentState.TransferPrepare;
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode == TransferFragmentRequestCode && resultCode == (int)Result.Ok)
            {
                State = FragmentState.Transfer;
                Transfer();
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void ClearEdits()
        {
            _recipientSearch.Text = _transferAmountEdit.Text = _transferCommentEdit.Text = string.Empty;
            _transferFacade.Recipient = null;
        }

        private void BackBtnOnClick(object sender, EventArgs e)
        {
            State = FragmentState.Cancel;
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