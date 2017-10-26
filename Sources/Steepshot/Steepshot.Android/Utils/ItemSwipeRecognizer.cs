using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Android.Widget;

namespace Steepshot.Utils
{
    public class Suboption
    {
        public EventHandler Click;
        public View View => _view;
        private View _view;
        private View _containerView;
        private View _buttonView;

        public Suboption(Context context)
        {
            LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            _view = inflater.Inflate(Resource.Layout.lyt_comment_item_suboption, null);
            _containerView = _view.FindViewById(Resource.Id.Suboption);
            _buttonView = _containerView.FindViewById(Resource.Id.SuboptionButton);
        }

        public void SetImageResource(int imageResource)
        {
            ((ImageButton)_buttonView).SetImageResource(imageResource);
        }

        public void SetPadding(int l, int t, int r, int b)
        {
            _containerView.SetPadding(l, t, r, b);
        }

        public void InvokeClick(View.TouchEventArgs e)
        {
            var r = new Rect(_view.Left + _buttonView.Left + _containerView.PaddingLeft - _containerView.PaddingRight,
                             _view.Top + _buttonView.Top + _containerView.PaddingTop - _containerView.PaddingBottom,
                             _view.Right - _buttonView.Right + _buttonView.Width + _containerView.PaddingLeft - _containerView.PaddingRight,
                             _view.Bottom - _buttonView.Bottom + _buttonView.Height + _containerView.PaddingTop - _containerView.PaddingBottom);
            if (r.Contains((int)e.Event.GetX(), (int)e.Event.GetY()))
                Click?.Invoke(_buttonView, e);
        }
    }

    public class ItemSwipeRecognizer : ItemTouchHelper.Callback
    {
        private RecyclerView _recyclerView;
        private int? _previousTouchedItem;

        public ItemSwipeRecognizer(RecyclerView recyclerView)
        {
            _previousTouchedItem = null;
            _recyclerView = recyclerView;
            _recyclerView.Touch += _recyclerView_Touch;
            AttachToRecycler();
        }

        void _recyclerView_Touch(object sender, View.TouchEventArgs e)
        {
            if (_previousTouchedItem != null && e.Event.Action == MotionEventActions.Down)
            {
                var currentViewHolder = _recyclerView.FindViewHolderForAdapterPosition((int)_previousTouchedItem) as ItemSwipeViewHolder;
                if (currentViewHolder != null)
                {
                    currentViewHolder.SubOptions.ForEach(so => so.InvokeClick(e));
                    //e.Handled = true;
                }
            }
            e.Handled = false;
        }

        private void AttachToRecycler()
        {
            var itemTouchHelper = new ItemTouchHelper(this);
            itemTouchHelper.AttachToRecyclerView(_recyclerView);
        }

        public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
        {
            int dragFlags = ItemTouchHelper.Up | ItemTouchHelper.Down;
            int swipeFlags = ItemTouchHelper.Start | ItemTouchHelper.End;
            return MakeMovementFlags(dragFlags, swipeFlags);
        }

        public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
        {
            return false;
        }

        public override bool IsLongPressDragEnabled => false;

        public override bool IsItemViewSwipeEnabled => true;

        public override void OnChildDraw(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX, float dY, int actionState, bool isCurrentlyActive)
        {
            var commentsViewHolder = viewHolder as ItemSwipeViewHolder;
            var itemView = viewHolder.ItemView;
            int subOptionWidth = (int)(itemView.Width * 0.2);
            int swipeLimit = subOptionWidth * commentsViewHolder.SubOptions.Count;
            if (_previousTouchedItem != null && _previousTouchedItem != commentsViewHolder.AdapterPosition)
                base.ClearView(recyclerView, recyclerView.FindViewHolderForAdapterPosition((int)_previousTouchedItem));
            _previousTouchedItem = commentsViewHolder.AdapterPosition;
            if (dX < 0 && -dX == swipeLimit)
            {
                base.ClearView(recyclerView, viewHolder);
                return;
            }
            if (dX < 0 && Math.Abs(dX) > swipeLimit)
                dX = -swipeLimit;
            var itemToDrawIndex = (int)Math.Ceiling(Math.Abs(dX) / subOptionWidth) - 1;
            if (isCurrentlyActive == true && dX < 0 && Math.Abs(dX) < swipeLimit)
            {
                for (int i = 0; i < itemToDrawIndex; i++)
                {
                    commentsViewHolder.SubOptions[i].View.Measure(subOptionWidth, itemView.Height);
                    commentsViewHolder.SubOptions[i].View.Layout(itemView.Right - subOptionWidth * (i + 1), itemView.Top, itemView.Right - subOptionWidth * i, itemView.Bottom);
                    c.Save();
                    c.Translate(commentsViewHolder.SubOptions[i].View.Left, itemView.Top);
                    commentsViewHolder.SubOptions[i].View.Draw(c);
                    c.Restore();
                }
                var itemToDraw = commentsViewHolder.SubOptions[itemToDrawIndex];
                itemToDraw.View.Measure(subOptionWidth, itemView.Height);
                itemToDraw.View.Layout(itemView.Right + (int)dX, itemView.Top, itemView.Right - subOptionWidth * itemToDrawIndex, itemView.Bottom);
                c.Save();
                c.Translate(itemToDraw.View.Left, itemView.Top);
                itemToDraw.View.Draw(c);
                c.Restore();
                base.OnChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, false);
            }
            else if (dX < 0)
            {
                for (int i = 0; i <= itemToDrawIndex; i++)
                {

                    commentsViewHolder.SubOptions[i].View.Measure(subOptionWidth, itemView.Height);
                    commentsViewHolder.SubOptions[i].View.Layout(itemView.Right - subOptionWidth * (i + 1), itemView.Top, itemView.Right - subOptionWidth * i, itemView.Bottom);
                    c.Save();
                    c.Translate(commentsViewHolder.SubOptions[i].View.Left, itemView.Top);
                    commentsViewHolder.SubOptions[i].View.Draw(c);
                    c.Restore();
                }
                base.OnChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, false);
            }
        }

        public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction) { }
    }

    public class ItemSwipeViewHolder : RecyclerView.ViewHolder
    {
        public List<Suboption> SubOptions { get; }
        public ItemSwipeViewHolder(View itemView) : base(itemView)
        {
            SubOptions = new List<Suboption>();
        }
    }
}
