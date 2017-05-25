using System;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;

namespace Steepshot
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ChangePasswordActivity : BaseActivity, ChangePasswordView
    {
        private EditText _oldPass;
        private EditText _newPass;
        private EditText _repeatPass;

		private ChangePasswordPresenter presenter;

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
            var typedsender = (EditText)sender;
            if (string.IsNullOrWhiteSpace(e.Text.ToString()))
            {
                var message = GetString(Resource.String.error_empty_field);
                var d = GetDrawable(Resource.Drawable.ic_error);
                d.SetBounds(0, 0, d.IntrinsicWidth, d.IntrinsicHeight);
                typedsender.SetError(message, d);
            }
            else if (typedsender == _repeatPass && !_repeatPass.Text.EndsWith(_newPass.Text))
            {
                var message = GetString(Resource.String.error_repeat_pass_fail);
                var d = GetDrawable(Resource.Drawable.ic_error);
                d.SetBounds(0, 0, d.IntrinsicWidth, d.IntrinsicHeight);
                typedsender.SetError(message, d);
            }
        }

        [InjectOnClick(Resource.Id.btn_change)]
        public async void ChangeClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_oldPass.Error) || !string.IsNullOrEmpty(_newPass.Error) || !string.IsNullOrEmpty(_repeatPass.Error))
                return;

			var response = await presenter.ChangePassword(_oldPass.Text, _newPass.Text);

            if (response.Success)
            {
                Finish();
            }
            else
            {
                ShowAlert(response.Errors);
            }
        }

        [InjectOnClick(Resource.Id.go_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            Finish();
        }

		protected override void CreatePresenter()
		{
			presenter = new ChangePasswordPresenter(this);
		}
	}
}