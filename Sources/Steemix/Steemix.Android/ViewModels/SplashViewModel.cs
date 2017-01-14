namespace Steemix.Droid
{
	public class SplashViewModel: MvvmViewModelBase
	{
		public bool IsGuest { get { return !UserPrincipal.IsAuthenticated; } }
	}
}
