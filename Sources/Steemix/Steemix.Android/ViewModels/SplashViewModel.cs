namespace Steemix.Droid.ViewModels
{
	public class SplashViewModel: MvvmViewModelBase
	{
		public bool IsGuest { get { return !UserPrincipal.IsAuthenticated; } }
	}
}
