using Steepshot.Base;

namespace Steepshot.Presenter
{
    public class SplashPresenter : BasePresenter
    {
        public SplashPresenter(IBaseView view) : base(view)
        {
        }

        public bool IsGuest { get { return !User.IsAuthenticated; } }
    }
}
