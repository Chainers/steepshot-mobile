using System;
using System.Collections.Generic;
using Android.Animation;
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
        public View ContainerView
        {
            get
            {
                return _containerView;
            }
        }
        private readonly View _containerView;
        private readonly View _buttonView;
        private readonly Context _context;

        public string Text
        {
            get
            {
                return ((Button)_buttonView).Text;
            }
            set
            {
                ((Button)_buttonView).Text = value;
            }
        }

        public Suboption(Context context)
        {
            _context = context;
            LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            _containerView = inflater.Inflate(Resource.Layout.lyt_comment_item_suboption, null);
            _buttonView = _containerView.FindViewById(Resource.Id.SuboptionButton);
        }

        public void SetPadding(int l, int t, int r, int b)
        {
            _containerView.SetPadding(l, t, r, b);
        }

        public bool InvokeClick(View.TouchEventArgs e)
        {
            var r = new Rect(_containerView.Left + _buttonView.Left + _containerView.PaddingLeft - _containerView.PaddingRight,
                             _containerView.Top + _buttonView.Top + _containerView.PaddingTop - _containerView.PaddingBottom,
                             _containerView.Right - _buttonView.Right + _buttonView.Width + _containerView.PaddingLeft - _containerView.PaddingRight,
                             _containerView.Bottom - _buttonView.Bottom + _buttonView.Height + _containerView.PaddingTop - _containerView.PaddingBottom);
            if (r.Contains((int)e.Event.GetX(), (int)e.Event.GetY()))
            {
                Click?.Invoke(_buttonView, e);
                return true;
            }
            return false;
        }
    }

    public class ItemSwipeRecognizer : ItemTouchHelper.Callback
    {
        private const double _subOptionWidthCof = 0.2;
        private readonly RecyclerView _recyclerView;
        private int? _previousTouchedItem;
        private bool _swiped, _wasSwiped;
        private float _xBegin, _xEnd, _deltaX, _yBegin;

        public ItemSwipeRecognizer(RecyclerView recyclerView)
        {
            _previousTouchedItem = null;
            _recyclerView = recyclerView;
            _recyclerView.Touch += RecyclerViewTouch;
            AttachToRecycler();
        }

        void RecyclerViewTouch(object sender, View.TouchEventArgs e)
        {
            ItemSwipeViewHolder viewHolder = null;
            View itemView = null;
            int subOptionWidth = 0, swipeLimit = 0;
            if (_previousTouchedItem != null)
            {
                viewHolder = _recyclerView.FindViewHolderForAdapterPosition(_previousTouchedItem.Value) as ItemSwipeViewHolder;
                if (viewHolder != null)
                {
                    itemView = viewHolder.ItemView;
                    subOptionWidth = (int)(itemView.Width * _subOptionWidthCof);
                    swipeLimit = -subOptionWidth * viewHolder.SubOptions.Count;
                    _swiped = viewHolder.ItemView.TranslationX <= swipeLimit;
                }
            }
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    _xBegin = e.Event.GetX();
                    _xEnd = _xBegin;
                    _yBegin = e.Event.GetY();
                    if (_swiped)
                    {
                        if (viewHolder.ItemView.Top > _yBegin || viewHolder.ItemView.Bottom < _yBegin)
                        {
                            ClearView(viewHolder);
                        }
                        else if (_xBegin > viewHolder.ItemView.TranslationX + viewHolder.ItemView.Width)
                        {
                            foreach (var subOption in viewHolder.SubOptions)
                                if (subOption.InvokeClick(e))
                                    break;
                        }
                    }
                    break;
                case MotionEventActions.Move:
                    _xEnd = e.Event.GetX();
                    _deltaX = _xEnd - _xBegin;
                    if (_wasSwiped)
                        _deltaX += swipeLimit;
                    break;
                case MotionEventActions.Up:
                    if (viewHolder != null)
                    {
                        if (_wasSwiped)
                            ClearView(viewHolder);
                        else if (_swiped)
                        {
                            _wasSwiped = true;
                            if (_xBegin == _xEnd || _deltaX > 0)
                                ClearView(viewHolder);
                        }
                        else if (itemView.Top < _yBegin && itemView.Bottom > _yBegin)
                            if (_deltaX < 0 && Math.Abs(_deltaX) >= 0.2 * Math.Abs(swipeLimit))
                            {
                                ValueAnimator animator = ValueAnimator.OfFloat(viewHolder.ItemView.TranslationX, swipeLimit);
                                animator.SetDuration(100);
                                animator.Update += (s, a) => _deltaX = (float)a.Animation.AnimatedValue;
                                animator.Start();
                            }
                            else if (_deltaX < 0)
                                ClearView(viewHolder);
                    }
                    break;
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
            if (_deltaX == 0) return;
            var itemSwipeViewHolder = viewHolder as ItemSwipeViewHolder;
            var itemView = viewHolder.ItemView;
            if (itemView.Top > _yBegin || itemView.Bottom < _yBegin) return;
            _previousTouchedItem = viewHolder.AdapterPosition;
            dX = _deltaX;
            int subOptionWidth = (int)(itemView.Width * _subOptionWidthCof);
            int swipeLimit = -subOptionWidth * itemSwipeViewHolder.SubOptions.Count;
            var itemToDrawIndex = (int)Math.Ceiling(Math.Abs(dX) / subOptionWidth) - 1;
            if (dX > swipeLimit && dX < 0)
            {
                for (int i = 0; i < itemToDrawIndex; i++)
                {
                    itemSwipeViewHolder.SubOptions[i].ContainerView.Measure(subOptionWidth, itemView.Height);
                    itemSwipeViewHolder.SubOptions[i].ContainerView.Layout(itemView.Right - subOptionWidth * (i + 1), itemView.Top, itemView.Right - subOptionWidth * i, itemView.Bottom);
                    c.Save();
                    c.Translate(itemSwipeViewHolder.SubOptions[i].ContainerView.Left, itemView.Top);
                    itemSwipeViewHolder.SubOptions[i].ContainerView.Draw(c);
                    c.Restore();
                }
                var itemToDraw = itemSwipeViewHolder.SubOptions[itemToDrawIndex];
                itemToDraw.ContainerView.Measure(subOptionWidth, itemView.Height);
                itemToDraw.ContainerView.Layout(itemView.Right + (int)dX, itemView.Top, itemView.Right - subOptionWidth * itemToDrawIndex, itemView.Bottom);
                c.Save();
                c.Translate(itemToDraw.ContainerView.Left, itemView.Top);
                itemToDraw.ContainerView.Draw(c);
                c.Restore();
                itemView.TranslationX = dX;
            }
            else if (dX < 0)
            {
                for (int i = 0; i < itemSwipeViewHolder.SubOptions.Count; i++)
                {
                    itemSwipeViewHolder.SubOptions[i].ContainerView.Measure(subOptionWidth, itemView.Height);
                    itemSwipeViewHolder.SubOptions[i].ContainerView.Layout(itemView.Right - subOptionWidth * (i + 1), itemView.Top, itemView.Right - subOptionWidth * i, itemView.Bottom);
                    c.Save();
                    c.Translate(itemSwipeViewHolder.SubOptions[i].ContainerView.Left, itemView.Top);
                    itemSwipeViewHolder.SubOptions[i].ContainerView.Draw(c);
                    c.Restore();
                }
                if (_deltaX != float.MinValue)
                    itemView.TranslationX = swipeLimit;
            }
        }

        public void ClearView(RecyclerView.ViewHolder viewHolder)
        {
            if (viewHolder != null)
            {
                _wasSwiped = false;
                _deltaX = float.MinValue;
                using (var animator = ValueAnimator.OfFloat(viewHolder.ItemView.TranslationX, 0))
                {
                    animator.SetDuration(500);
                    animator.Update += (sender, e) => viewHolder.ItemView.TranslationX = (float)e.Animation.AnimatedValue;
                    animator.AnimationEnd += (sender, e) =>
                    {
                        _deltaX = 0;
                        _recyclerView.GetAdapter().NotifyItemChanged(viewHolder.AdapterPosition);
                    };
                    animator.Start();
                }
            }
        }

        public override void ClearView(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
        {
            return;
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
