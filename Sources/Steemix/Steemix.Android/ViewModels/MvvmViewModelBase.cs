using System;
using GalaSoft.MvvmLight;
using Steemstagram.Shared;

namespace Steemix.Android
{
	    public abstract class MvvmViewModelBase : ViewModelBase
    {

		protected Manager Manager { get { return SteemixApp.Manager;}}

        public virtual void ViewLoad() { }

        public virtual void ViewAppear() { }

        public virtual void ViewDisappear() { }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        protected object Parameter { get; set; }

        public void SetParameter(object parameter)
        {
            this.Parameter = parameter;
        }
    }
}
