using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Steemix.Library.HttpClient;

namespace Steemix.Android.Activity
{
    public class BaseActivity : AppCompatActivity
    {
        protected readonly SteemixApiClient ApiClient = new SteemixApiClient();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }
    }
}