using System.Collections;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class SpinnerAdapter : ArrayAdapter
    {
        public SpinnerAdapter(Context context, int resource, IList objects) : base(context, resource, objects)
        {
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = (TextView)base.GetView(position, convertView, parent);
            view.Typeface = Style.Semibold;
            view.TextSize = TypedValue.ApplyDimension(ComplexUnitType.Sp, 6, parent.Context.Resources.DisplayMetrics);
            return view;
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            var view = (TextView)base.GetDropDownView(position, convertView, parent);
            view.Typeface = Style.Semibold;
            view.TextSize = TypedValue.ApplyDimension(ComplexUnitType.Sp, 6, parent.Context.Resources.DisplayMetrics);
            return view;
        }
    }
}