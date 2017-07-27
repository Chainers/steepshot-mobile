namespace Steepshot
{
    public class SplashPresenter : BasePresenter
    {
        public SplashPresenter(SplashView view) : base(view)
        {
        }

        public bool IsGuest { get { return !User.IsAuthenticated; } }
    }
}
