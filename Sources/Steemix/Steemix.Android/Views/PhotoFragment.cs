using System;
using Android.Support.V4.App;
using Android.Views;
using Android.OS;
using Com.Lilarcor.Cheeseknife;
using Steemix.Android.Activity;

namespace Steemix.Android
{
	public class PhotoFragment : Fragment
	{
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var v = inflater.Inflate(Resource.Layout.lyt_fragment_photo, null);
			Cheeseknife.Inject(this, v);
			return v;
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			Cheeseknife.Reset(this);
		}
	}
}
