using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class TrxModel
    {
        public DateTime Date { get; set; }
    }

    public class TrxHistoryAdapter : RecyclerView.Adapter
    {
        public TrxHistoryAdapter()
        {
        }

        public override int ItemCount => 10;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_trx_history, null);
            return new TrxHistoryHolder(itemView);
        }
    }

    public class TrxHistoryHolder : RecyclerView.ViewHolder
    {
        public DateTime Date { get; private set; }

        public TrxHistoryHolder(View itemView) : base(itemView)
        {
        }

        public void UpdateData()
        {

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
                var middle = (child.Top + child.Bottom) / 2f;

                if (child.Top != 0)
                    c.DrawLine(left, child.Top - _itemSpacing, left, middle - _dashSpace, _paint);
                c.DrawCircle(left, middle, _dotRadius, _paint);
                if (child.Bottom != parent.Height - _itemSpacing)
                    c.DrawLine(left, middle + _dashSpace, left, child.Bottom + _itemSpacing, _paint);
            }
        }
    }
}