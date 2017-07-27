using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Steepshot.Fragment;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace Steepshot.Base
{
	public abstract class BaseActivity : AppCompatActivity, IBaseView
	{
		protected HostFragment CurrentHostFragment;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			CreatePresenter();
		}

		protected override void OnPostCreate(Bundle savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);
		}

		protected abstract void CreatePresenter();

		public Context GetContext()
		{
			return this;
		}

		public override void OnBackPressed()
		{
			if (CurrentHostFragment == null || !CurrentHostFragment.HandleBackPressed(SupportFragmentManager))
				base.OnBackPressed();
		}

		public virtual void OpenNewContentFragment(Android.Support.V4.App.Fragment frag)
		{
			CurrentHostFragment.ReplaceFragment(frag, true);
		}

		protected virtual void ShowAlert(int messageid)
		{
			var message = GetString(messageid);
			AlertDialog.Builder alert = new AlertDialog.Builder(this);
			alert.SetTitle("Error");
			alert.SetMessage(message);
			alert.SetPositiveButton("Ok", (senderAlert, args) => { });
			Dialog dialog = alert.Create();
			dialog.Show();
		}

		protected virtual void ShowAlert(string message)
		{
			AlertDialog.Builder alert = new AlertDialog.Builder(this);
			alert.SetMessage(message);
			alert.SetPositiveButton("Ok", (senderAlert, args) => { });
			Dialog dialog = alert.Create();
			dialog.Show();
		}

		protected virtual void ShowAlert(List<string> messages)
		{
			AlertDialog.Builder alert = new AlertDialog.Builder(this);
			alert.SetMessage(string.Join(System.Environment.NewLine, messages));
			alert.SetPositiveButton("Ok", (senderAlert, args) => { });
			Dialog dialog = alert.Create();
			dialog.Show();
		}
	}
}
