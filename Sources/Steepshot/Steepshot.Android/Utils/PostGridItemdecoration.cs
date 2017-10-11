using Android.Support.V7.Widget;

namespace Steepshot.Utils
{
    public class PostGridItemdecoration : RecyclerView.ItemDecoration
    {
        private bool _skipFirst;

        public PostGridItemdecoration()
        {
            _skipFirst = false;
        }


        public PostGridItemdecoration(bool skipFirst)
        {
            _skipFirst = skipFirst;
        }

        public override void GetItemOffsets(Android.Graphics.Rect outRect, Android.Views.View view, RecyclerView parent, RecyclerView.State state)
        {
            var index = ((RecyclerView.LayoutParams)view.LayoutParameters).ViewAdapterPosition;
            if (_skipFirst)
            {
                if (index == 0)
                {
                    base.GetItemOffsets(outRect, view, parent, state);
                    return;
                }
                index--;
            }

            switch (index % 3)
            {
                case 0:
                    {
                        outRect.Set(0, 0, 2, 3);
                        break;
                    }
                case 1:
                    {
                        outRect.Set(1, 0, 1, 3);
                        break;
                    }
                case 2:
                    {
                        outRect.Set(2, 0, 0, 3);
                        break;
                    }
            }
        }
    }
}
