using GalaSoft.MvvmLight;
using Sweetshot.Library.HttpClient;

namespace Steemix.Droid.ViewModels
{
    public abstract class MvvmViewModelBase : ViewModelBase
    {
        //TODO:KOA: move to some config
        //<add key="sweetshot_url" value="http://138.197.40.124/api/v1/" />
        protected SteepshotApiClient Api { get { return new SteepshotApiClient("http://138.197.40.124/api/v1/"); } }

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
