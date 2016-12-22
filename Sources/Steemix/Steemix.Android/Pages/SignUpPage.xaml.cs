using System;
using System.Linq;
using Steemix.Library.Exceptions;
using Steemix.Library.HttpClient;
using Steemix.Library.Models.Requests;
using Xamarin.Forms;

namespace Steemix.Android.Pages
{
    public partial class SignUpPage : ContentPage
    {
        readonly SteemixApiClient _api = new SteemixApiClient();

        public SignUpPage()
        {
            InitializeComponent();
        }

        async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            var request = new RegisterRequest(postingKeyEntry.Text, usernameEntry.Text, passwordEntry.Text);

            if (!IsValid(request))
            {
                messageLabel.Text = "Sign up failed";
                return;
            }

            try
            {
                var response = _api.Register(request);
                if (string.IsNullOrEmpty(response.error))
                {
                    var rootPage = Navigation.NavigationStack.FirstOrDefault();
                    if (rootPage != null)
                    {
                        //App.IsUserLoggedIn = true;
                        //Navigation.InsertPageBefore(new MainPage(), Navigation.NavigationStack.First());
                        //await Navigation.PopToRootAsync();
                    }
                }
                else
                {
                    messageLabel.Text = response.error;
                }
            }
            catch (ApiGatewayException ex)
            {
                messageLabel.Text = "Sign up failed";
            }
        }

        async void OnCancelButtonClicked(object sender, EventArgs e)
        {
        }

        bool IsValid(RegisterRequest request)
        {
            return !string.IsNullOrWhiteSpace(request.posting_key) && !string.IsNullOrWhiteSpace(request.username) && !string.IsNullOrWhiteSpace(request.password);
        }
    }
}