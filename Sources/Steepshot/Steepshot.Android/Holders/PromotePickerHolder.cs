using System;
using System.Collections;
using System.Collections.Generic;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Com.Aigestudio.Wheelpicker;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Utils;
using Android.Content;

namespace Steepshot.Holders
{
    public class PromotePickerHolder : RecyclerView.ViewHolder
    {
        private readonly Context _context;
        private readonly IList _displayCoins;
        private readonly List<CurrencyType> _coins;

        public int selectedPosition;
        public Action<ActionType> PromoteAction;

        public PromotePickerHolder(View itemView, Context context, List<CurrencyType> data, int selected) : base(itemView)
        {
            _context = context;

            _displayCoins = new List<string>();
            data.ForEach(x => _displayCoins.Add(x.ToString().ToUpper()));
            _coins = data;
            selectedPosition = selected;

            InitializeView();
        }

        private void InitializeView()
        {
            var wheelPicker = ItemView.FindViewById<WheelPicker>(Resource.Id.coin_picker);
            wheelPicker.Typeface = Style.Light;
            wheelPicker.VisibleItemCount = _coins.Count;
            wheelPicker.SetAtmospheric(true);
            wheelPicker.SelectedItemTextColor = Style.R255G34B5;
            wheelPicker.ItemTextColor = Color.Black;
            wheelPicker.ItemTextSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, 27, _context.Resources.DisplayMetrics);
            wheelPicker.ItemSpace = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 20, _context.Resources.DisplayMetrics);
            wheelPicker.Data = _displayCoins;
            wheelPicker.SelectedItemPosition = selectedPosition;
            wheelPicker.ItemSelected += WheelPickerOnItemSelected;
        }

        private void WheelPickerOnItemSelected(object sender, WheelPicker.ItemSelectedEventArgs e)
        {
            selectedPosition = e.P2;
        }
    }
}
