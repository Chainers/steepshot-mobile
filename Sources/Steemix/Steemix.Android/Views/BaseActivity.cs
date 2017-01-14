using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace Steemix.Droid.Activity 
{
    public class  BaseActivity<T> : AppCompatActivity where T : MvvmViewModelBase
    {
		protected T ViewModel { get { return SteemixApp.ViewModelLocator.GetViewModel<T>(); } }

		protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
			ViewModel.ViewLoad();
        }

		protected override void OnResume()
		{
			base.OnResume();
			ViewModel.ViewAppear();
		}

		protected override void OnPause()
		{
			ViewModel.ViewDisappear();
			base.OnPause();
		}

        protected virtual void ShowAlert(int messageid)
        {
            var message = GetString(messageid);
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Error");
            alert.SetMessage(message);
            alert.SetPositiveButton("Ok", ((senderAlert, args) => { }));
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        protected virtual void ShowAlert(string message)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetMessage(message);
            alert.SetPositiveButton("Ok", ((senderAlert, args) => { }));
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        protected virtual void ShowAlert(List<string> messages)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetMessage(string.Join(System.Environment.NewLine, messages));
            alert.SetPositiveButton("Ok", ((senderAlert, args) => { }));
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        public void OnCreatePresenter()
		{
			
		}
	}
}