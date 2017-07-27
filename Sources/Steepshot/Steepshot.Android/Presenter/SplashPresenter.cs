using Steepshot.Base;
using Steepshot.View;

namespace Steepshot.Presenter
{
    public class SplashPresenter : BasePresenter
    {
        public SplashPresenter(SplashView view) : base(view)
        {
        }

        public bool IsGuest { get { return !User.IsAuthenticated; } }
    }
}
