using System.Collections.Generic;
using Android.Content;
using Android.Support.V4.App;
using Steepshot.Base;
using Steepshot.Fragment;


namespace Steepshot.Adapter
{
    public class PagerAdapter : FragmentPagerAdapter
    {
        public int[] tabIcos = new int[] {
            Resource.Drawable.ic_home,
            Resource.Drawable.ic_browse,
            Resource.Drawable.ic_camera_new,
            Resource.Drawable.ic_profile_new
        };
        Context context;

        private List<Android.Support.V4.App.Fragment> tabs = new List<Android.Support.V4.App.Fragment>();

        public PagerAdapter(FragmentManager fm, Context context) : base(fm)
        {
            this.context = context;
            InitializeTabs();
        }

        public override int Count
        {
            get
            {
                return tabIcos.Length;
            }
        }

        public override Android.Support.V4.App.Fragment GetItem(int position)
        {
            return tabs[position];
        }

        private void InitializeTabs()
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                tabs.Add(new FeedFragment());
                return;
            }

            for (var i = 0; i < tabIcos.Length; i++)
            {
                Android.Support.V4.App.Fragment frag;
                switch (i)
                {
                    case 0:
                        frag = HostFragment.NewInstance(new FeedFragment(true));
                        break;
                    case 1:
                        frag = HostFragment.NewInstance(new FeedFragment());
                        break;
                    case 2:
                        frag = HostFragment.NewInstance(new PhotoFragment());
                        break;
                    case 3:
                        frag = HostFragment.NewInstance(new ProfileFragment(BasePresenter.User.Login));
                        break;
                    default:
                        frag = null;
                        break;
                }
                if (frag != null)
                    tabs.Add(frag);
            }
        }
    }
}