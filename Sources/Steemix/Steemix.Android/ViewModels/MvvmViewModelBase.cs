using GalaSoft.MvvmLight;
using Sweetshot.Library.HttpClient;

namespace Steemix.Droid.ViewModels
{
	    public abstract class MvvmViewModelBase : ViewModelBase
    {

		protected SteepshotApiClient Api { get { return new SteepshotApiClient(""); }}

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
