using GalaSoft.MvvmLight;

namespace Steemix.Droid.ViewModels
{
    public abstract class MvvmViewModelBase : ViewModelBase
    {
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
