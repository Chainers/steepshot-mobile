using Steepshot.Base;
using Steepshot.View;

namespace Steepshot.Presenter
{
    public class SplashPresenter : BasePresenter
    {
        public SplashPresenter(ISplashView view) : base(view)
        {
        }

        public bool IsGuest { get { return !User.IsAuthenticated; } }
    }
}
