using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Models.Common
{
    public class BalanceModel : INotifyPropertyChanged
    {
        public double Value { get; private set; }

        public byte MaxDecimals { get; private set; }

        public CurrencyType CurrencyType { get; private set; }

        public double EffectiveSp { get; set; }

        public double RewardSteem { get; set; }

        public double RewardSp { get; set; }

        public double RewardSbd { get; set; }

        public double DelegatedToMe { get; set; }

        public double DelegatedByMe { get; set; }

        public double ToWithdraw { get; set; }


        public BalanceModel(double value, byte maxDecimals, CurrencyType currencyType)
        {
            Value = value;
            MaxDecimals = maxDecimals;
            CurrencyType = currencyType;
        }
        
        public void Update(BalanceModel model)
        {
            Value = model.Value;
            EffectiveSp = model.EffectiveSp;
            RewardSteem = model.RewardSteem;
            RewardSp = model.RewardSp;
            RewardSbd = model.RewardSbd;
            DelegatedToMe = model.DelegatedToMe;
            DelegatedByMe = model.DelegatedByMe;
            ToWithdraw = model.ToWithdraw;

            NotifyPropertyChanged(nameof(BalanceModel));
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
