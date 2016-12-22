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

namespace Steemix.Android.Activity
{
    [Activity(Label = "SteepShot",MainLauncher =true,Icon ="@mipmap/ic_launcher")]
    public class SignUpActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_sign_up);
        }
    }
}