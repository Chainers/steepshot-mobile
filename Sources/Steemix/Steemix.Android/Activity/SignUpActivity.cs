using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Steemix.Library.Exceptions;
using Steemix.Library.Models.Requests;

namespace Steemix.Android.Activity
{
    [Activity(Label = "SteepShot")]
    public class SignUpActivity : BaseActivity
    {
        private AppCompatButton SignUpBtn;
        private AppCompatButton ForgotPassBtn;
        private AppCompatButton CreateAccBtn;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_sign_up);

            SignUpBtn = FindViewById<AppCompatButton>(Resource.Id.sign_up_btn);
            ForgotPassBtn = FindViewById<AppCompatButton>(Resource.Id.forgot_pass_btn);
            CreateAccBtn = FindViewById<AppCompatButton>(Resource.Id.create_acc_btn);

            SignUpBtn.Click += SignUpBtn_Click;
            ForgotPassBtn.Click += ForgotPassBtn_Click;
            CreateAccBtn.Click += CreateAccBtn_Click;
        }

        private void CreateAccBtn_Click(object sender, System.EventArgs e)
        {

        }

        private void ForgotPassBtn_Click(object sender, System.EventArgs e)
        {
        }

        private void SignUpBtn_Click(object sender, System.EventArgs e)
        {
            var username = FindViewById<AppCompatButton>(Resource.Id.input_username);
            var postingKey = FindViewById<AppCompatButton>(Resource.Id.input_posting_key);
            var password = FindViewById<AppCompatButton>(Resource.Id.input_password);
            var request = new RegisterRequest(postingKey.Text, username.Text, password.Text);

            if (!IsValid(request))
            {
                ShowAlert(Resource.String.msg_empty_user_login);
            }
            else
            {
                try
                {
                    var response = ApiClient.Register(request);
                    if (!string.IsNullOrEmpty(response.error))
                    {
                        ShowAlert(Resource.String.error_not_uniq_user);
                    }
                    else
                    {
                        UserName = response.username;
                        Token = response.Token;
                    }
                }
                catch (ApiGatewayException ex)
                {
                    ShowAlert(Resource.String.error_connect_to_server);
                }
            }
        }

        private bool IsValid(RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.username)
                || string.IsNullOrEmpty(request.password)
                || string.IsNullOrEmpty(request.posting_key))
                return false;
            return true;
        }
    }
}