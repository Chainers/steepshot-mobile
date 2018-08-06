using System.Globalization;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using IO.SuperCharge.ShimmerLayoutLib;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;
using Steepshot.CustomViews;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class TrxHistoryAdapter : RecyclerView.Adapter
    {
        private AccountHistoryResponse[] _accountHistory;

        public void SetAccountHistory(AccountHistoryResponse[] accountHistory)
        {
            _accountHistory = accountHistory;
            NotifyDataSetChanged();
        }

        public override int ItemCount => _accountHistory?.Length ?? 10;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is TrxHistoryHolder trxHolder)
                trxHolder.UpdateData(_accountHistory[position], position == 0 || _accountHistory[position].DateTime.Date != _accountHistory[position - 1].DateTime.Date);
            else if (holder is TrxHistoryShimmerHolder trxShimmerHolder)
                trxShimmerHolder.Animate();
        }

        public override int GetItemViewType(int position)
        {
            return _accountHistory == null ? -1 : 0;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (viewType >= 0)
            {
                var trxHistoryView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_trx_history, parent, false);
                return new TrxHistoryHolder(trxHistoryView);
            }

            var trxHistoryShimmerView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_trx_history_shimmer, parent, false);
            return new TrxHistoryShimmerHolder(trxHistoryShimmerView);
        }
    }

    public class TrxHistoryShimmerHolder : RecyclerView.ViewHolder
    {
        private readonly ShimmerLayout _shimmerContainer;
        private readonly ShimmerLayout _trxType;
        private readonly ShimmerLayout _recipient;
        private readonly ShimmerLayout _amount;

        public TrxHistoryShimmerHolder(View itemView) : base(itemView)
        {
            _shimmerContainer = itemView.FindViewById<ShimmerLayout>(Resource.Id.shimmer_container);
            _trxType = itemView.FindViewById<ShimmerLayout>(Resource.Id.shimmer_trx_type);
            _recipient = itemView.FindViewById<ShimmerLayout>(Resource.Id.shimmer_recipient);
            _amount = itemView.FindViewById<ShimmerLayout>(Resource.Id.shimmer_balance);

            _shimmerContainer.SetShimmerColor(Color.Argb(80, 255, 255, 255));
            _shimmerContainer.SetMaskWidth(0.8f);

            _trxType.SetShimmerColor(Color.White);
            _trxType.SetMaskWidth(0.8f);

            _recipient.SetShimmerColor(Color.White);
            _recipient.SetMaskWidth(0.8f);

            _amount.SetShimmerColor(Color.White);
            _amount.SetMaskWidth(0.8f);

        }

        public void Animate()
        {
            _shimmerContainer.StartShimmerAnimation();
            _trxType.StartShimmerAnimation();
            _recipient.StartShimmerAnimation();
            _amount.StartShimmerAnimation();
        }
    }

    public class TrxHistoryHolder : RecyclerView.ViewHolder
    {
        private readonly TextView _date;
        private readonly TextView _trxType;
        private readonly AutoLinkTextView _recipient;
        private readonly TextView _amount;
        private readonly LinearLayout _claimRewards;
        private readonly TextView _tokenOne;
        private readonly TextView _tokenTwo;
        private readonly TextView _tokenThree;
        private readonly TextView _tokenOneValue;
        private readonly TextView _tokenTwoValue;
        private readonly TextView _tokenThreeValue;

        public TrxHistoryHolder(View itemView) : base(itemView)
        {
            _date = itemView.FindViewById<TextView>(Resource.Id.date);
            _trxType = itemView.FindViewById<TextView>(Resource.Id.trx_type);
            _recipient = itemView.FindViewById<AutoLinkTextView>(Resource.Id.recipient);
            _amount = itemView.FindViewById<TextView>(Resource.Id.trx_amount);
            _tokenOne = itemView.FindViewById<TextView>(Resource.Id.token_one);
            _tokenTwo = itemView.FindViewById<TextView>(Resource.Id.token_two);
            _tokenThree = itemView.FindViewById<TextView>(Resource.Id.token_three);
            _tokenOneValue = itemView.FindViewById<TextView>(Resource.Id.token_one_value);
            _tokenTwoValue = itemView.FindViewById<TextView>(Resource.Id.token_two_value);
            _tokenThreeValue = itemView.FindViewById<TextView>(Resource.Id.token_three_value);
            _claimRewards = itemView.FindViewById<LinearLayout>(Resource.Id.claims);

            _date.Typeface = Style.Regular;
            _trxType.Typeface = Style.Semibold;
            _recipient.Typeface = Style.Regular;
            _amount.Typeface = Style.Semibold;
            _tokenOne.Typeface = Style.Semibold;
            _tokenTwo.Typeface = Style.Semibold;
            _tokenThree.Typeface = Style.Semibold;
            _tokenOneValue.Typeface = Style.Semibold;
            _tokenTwoValue.Typeface = Style.Semibold;
            _tokenThreeValue.Typeface = Style.Semibold;
        }

        public void UpdateData(AccountHistoryResponse transaction, bool headItem)
        {
            _date.Visibility = headItem ? ViewStates.Visible : ViewStates.Gone;
            _date.Text = transaction.DateTime.ToString("dd MMM yyyy", CultureInfo.GetCultureInfo("en-US"));
            _trxType.Text = transaction.Type.ToString();
            _recipient.AutoLinkText = $"{(transaction.From.Equals(AppSettings.User.Login) ? $"to @{transaction.To}" : $"from @{transaction.From}")}";
            if (transaction.Type == AccountHistoryResponse.OperationType.ClaimReward)
            {
                _tokenOne.Text = CurrencyType.Steem.ToString().ToUpper();
                _tokenOneValue.Text = transaction.RewardSteem;
                _tokenTwo.Text = $"{CurrencyType.Steem.ToString()} Power".ToUpper();
                _tokenTwoValue.Text = transaction.RewardSp;
                _tokenThree.Text = CurrencyType.Sbd.ToString().ToUpper();
                _tokenThreeValue.Text = transaction.RewardSbd;
                _amount.Visibility = ViewStates.Gone;
                _claimRewards.Visibility = ViewStates.Visible;
            }
            else
            {
                _amount.Text = $"{transaction.Amount}";
                _amount.Visibility = ViewStates.Visible;
                _claimRewards.Visibility = ViewStates.Gone;
            }
        }
    }

    public class DividerItemDecoration : RecyclerView.ItemDecoration
    {
        private readonly Paint _paint;
        private readonly int _itemSpacing;
        private readonly int _dashSpace;
        private readonly int _dotRadius;

        public DividerItemDecoration(Context context)
        {
            _paint = new Paint(PaintFlags.AntiAlias)
            {
                Color = Style.R230G230B230,
                StrokeCap = Paint.Cap.Round,
                StrokeWidth = TypedValue.ApplyDimension(ComplexUnitType.Dip, 2, context.Resources.DisplayMetrics)
            };
            _paint.SetStyle(Paint.Style.Fill);
            _dashSpace = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 16, context.Resources.DisplayMetrics);
            _dotRadius = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 3f, context.Resources.DisplayMetrics);
            _itemSpacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, context.Resources.DisplayMetrics);
        }

        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            outRect.Bottom = _itemSpacing;
        }

        public override void OnDrawOver(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            var left = (int)(parent.PaddingLeft / 2.5);

            for (int i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChildAt(i);
                var dateLabel = child.FindViewById<TextView>(Resource.Id.date);
                var dateLabelLytParams = (LinearLayout.LayoutParams)dateLabel?.LayoutParameters;
                var dateLabelFix = dateLabel?.Visibility == ViewStates.Visible
                    ? dateLabel.Height + dateLabelLytParams.TopMargin + dateLabelLytParams.BottomMargin
                    : 0;
                var middle = (child.Top + dateLabelFix + child.Bottom) / 2f;

                if (child.Top != 0)
                    c.DrawLine(left, child.Top - _itemSpacing, left, middle - _dashSpace, _paint);
                c.DrawCircle(left, middle, _dotRadius, _paint);
                if (child.Bottom != parent.Height - _itemSpacing)
                    c.DrawLine(left, middle + _dashSpace, left, child.Bottom + _itemSpacing, _paint);
            }
        }
    }
}