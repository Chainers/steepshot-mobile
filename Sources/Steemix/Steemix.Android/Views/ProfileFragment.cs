using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Com.Lilarcor.Cheeseknife;

namespace Steemix.Droid.Views
{
	public class ProfileFragment : Fragment
	{
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var v = inflater.Inflate(Resource.Layout.lyt_fragment_profile, null);
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
