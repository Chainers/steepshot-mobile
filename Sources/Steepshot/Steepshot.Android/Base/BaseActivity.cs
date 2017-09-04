using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Support.V7.App;
using Square.Picasso;
using Steepshot.Core;
using Steepshot.Fragment;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace Steepshot.Base
{
    public abstract class BaseActivity : AppCompatActivity, IBaseView
    {
        public static LruCache Cache { get; set; }
        protected HostFragment CurrentHostFragment;

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
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle(Localization.Messages.Error);
            alert.SetMessage(message);
            alert.SetPositiveButton(Localization.Messages.Ok, (senderAlert, args) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        protected virtual void ShowAlert(string message)
        {
            var alert = new AlertDialog.Builder(this);
            alert.SetMessage(message);
            alert.SetPositiveButton(Localization.Messages.Ok, (senderAlert, args) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        protected virtual void ShowAlert(List<string> messages)
        {
            var alert = new AlertDialog.Builder(this);
            alert.SetMessage(string.Join(System.Environment.NewLine, messages));
            alert.SetPositiveButton(Localization.Messages.Ok, (senderAlert, args) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }
    }
}
