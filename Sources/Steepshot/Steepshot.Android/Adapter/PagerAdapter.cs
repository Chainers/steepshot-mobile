using System.Collections.Generic;
using Android.OS;
using Android.Support.V4.App;
using Steepshot.Core.Presenters;
using Steepshot.Fragment;

namespace Steepshot.Adapter
{
    public class PagerAdapter : FragmentStatePagerAdapter
    {
        public readonly int[] TabIconsInactive = {
            Resource.Drawable.home,
            Resource.Drawable.search,
            Resource.Drawable.create,
            Resource.Drawable.profile
        };

        public readonly int[] TabIconsActive = {
            Resource.Drawable.home_active,
            Resource.Drawable.search_active,
            Resource.Drawable.create_active,
            Resource.Drawable.profile_active
        };

        private readonly List<HostFragment> _tabs;

        public override int Count => TabIconsInactive.Length;


        public PagerAdapter(FragmentManager fm) : base(fm)
        {
            _tabs = new List<HostFragment>();
            InitializeTabs();
        }


        public override Android.Support.V4.App.Fragment GetItem(int position)
        {
            if (_tabs.Count > position && position > -1)
                return _tabs[position];

            return null;
        }

        private void InitializeTabs()
        {
            for (var i = 0; i < TabIconsInactive.Length; i++)
            {
                HostFragment frag;
                switch (i)
                {
                    case 0:
                        frag = HostFragment.NewInstance(new FeedFragment());
                        break;
                    case 1:
                        frag = HostFragment.NewInstance(new PreSearchFragment());
                        break;
                    case 2:
                        frag = new HostFragment();
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

        public override IParcelable SaveState()
        {
            return null;
        }
    }
}
