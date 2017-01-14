using System;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steemix.Droid.Activity;
using Steemix.Droid.ViewModels;

namespace Steemix.Droid.Views
{
    [Activity(NoHistory = true)]
    public class ChangePassword : BaseActivity<SettingsViewModel>
    {
        private EditText _oldPass;
        private EditText _newPass;
        private EditText _repeatPass;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_change_password);
            Cheeseknife.Inject(this);

            _oldPass = FindViewById<EditText>(Resource.Id.input_old_password);
            _newPass = FindViewById<EditText>(Resource.Id.input_new_password);
            _repeatPass = FindViewById<EditText>(Resource.Id.input_repeat_new_password);
            _oldPass.TextChanged += TextChanged;
            _newPass.TextChanged += TextChanged;
            _repeatPass.TextChanged += TextChanged;
        }

        private void TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            var isValid = true;
            var message = string.Empty;
            var typedsender = (EditText)sender;
            if (string.IsNullOrWhiteSpace(e.Text.ToString()))
            {
                isValid = false;
                message = GetString(Resource.String.error_empty_field);
            }
            else if (typedsender == _repeatPass && !_repeatPass.Text.EndsWith(_newPass.Text))
            {
                isValid = false;
                message = GetString(Resource.String.error_repeat_pass_fail);
            }

            typedsender.SetError(message, null);
            typedsender.SetBackgroundColor(isValid ? Color.White : Color.Red);
        }

        [InjectOnClick(Resource.Id.btn_change)]
        public void ChangeClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_oldPass.Error) || !string.IsNullOrEmpty(_newPass.Error) || !string.IsNullOrEmpty(_repeatPass.Error))
                return;


            Finish();
        }
    }
}