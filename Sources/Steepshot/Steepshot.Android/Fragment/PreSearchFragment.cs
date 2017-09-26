using System;
using Android.OS;
using Android.Views;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Base;

namespace Steepshot.Fragment
{
    public class PreSearchFragment : BaseFragment
    {

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                V = inflater.Inflate(Resource.Layout.lyt_feed, null);
                Cheeseknife.Inject(this, V);
            }
            return V;
        }

    }
}
