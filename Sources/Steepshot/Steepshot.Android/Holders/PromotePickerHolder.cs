using System.Collections;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Steepshot.Core.Models.Requests;
using Steepshot.CustomViews;

namespace Steepshot.Holders
{
    public class PromotePickerHolder : RecyclerView.ViewHolder
    {
        private readonly IList _displayCoins;

        public int SelectedPosition;

        public PromotePickerHolder(View itemView, List<CurrencyType> data, int selected) : base(itemView)
        {
            _displayCoins = data;
            SelectedPosition = selected;

            InitializeView();
        }

        private void InitializeView()
        {
            var wheelPicker = ItemView.FindViewById<WheelPicker>(Resource.Id.coin_picker);
            wheelPicker.Items = _displayCoins;
            wheelPicker.ItemSelected += WheelPickerOnItemSelected;
            wheelPicker.Select(SelectedPosition);
        }

        private void WheelPickerOnItemSelected(int pos)
        {
            SelectedPosition = pos;
        }
    }
}
