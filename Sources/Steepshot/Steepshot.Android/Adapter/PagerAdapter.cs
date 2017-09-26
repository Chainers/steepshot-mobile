using System.Collections.Generic;
using Android.Support.V4.App;
using Steepshot.Core.Presenters;
using Steepshot.Fragment;

namespace Steepshot.Adapter
{
    public class PagerAdapter : FragmentPagerAdapter
    {
        public int[] TabIconsInactive = new[] {
            Resource.Drawable.home,
            Resource.Drawable.search,
            Resource.Drawable.create,
            Resource.Drawable.profile
        };

        public int[] TabIconsActive = new[] {
            Resource.Drawable.home_active,
            Resource.Drawable.search_active,
            Resource.Drawable.create_active,
            Resource.Drawable.profile_active
        };

        private readonly List<Android.Support.V4.App.Fragment> _tabs = new List<Android.Support.V4.App.Fragment>();

        public PagerAdapter(FragmentManager fm) : base(fm)
        {
            InitializeTabs();
        }

        public override int Count => TabIconsInactive.Length;

        public override Android.Support.V4.App.Fragment GetItem(int position)
        {
            return _tabs[position];
        }

        private void InitializeTabs()
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                _tabs.Add(new FeedFragment());
                return;
            }

            for (var i = 0; i < TabIconsInactive.Length; i++)
            {
                Android.Support.V4.App.Fragment frag;
                switch (i)
                {
                    case 0:
                        frag = HostFragment.NewInstance(new FeedFragment(true));
                        break;
                    case 1:
                        frag = HostFragment.NewInstance(new PreSearchFragment());
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
                    _tabs.Add(frag);
            }
        }
    }
}