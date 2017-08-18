namespace Steepshot.Core.Presenters
{
    public class SplashPresenter : BasePresenter
    {
        public bool IsGuest => !User.IsAuthenticated;
    }
}