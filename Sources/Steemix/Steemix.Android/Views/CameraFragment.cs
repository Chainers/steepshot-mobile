using System;
using Android.Support.V4.App;
using Android.Views;
using Android.OS;
using Com.Lilarcor.Cheeseknife;
using Steemix.Android.Activity;
using Android.Widget;

namespace Steemix.Android
{
	public class CameraFragment: BaseFragment<CameraViewModel>
	{
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var v = inflater.Inflate(Resource.Layout.lyt_fragment_take_photo, null);
			Cheeseknife.Inject(this, v);
			return v;
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			Cheeseknife.Reset(this);
		}
	}
}

