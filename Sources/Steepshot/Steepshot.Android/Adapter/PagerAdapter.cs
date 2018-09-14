using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Support.V4.App;
using Steepshot.Core.Authorization;
using Steepshot.Core.Models.Common;
using Steepshot.Fragment;
using Steepshot.Core.Utils;

namespace Steepshot.Adapter
{
    public sealed class PagerAdapter : FragmentStatePagerAdapter
    {
        public readonly int[] TabIconsInactive = {
            Resource.Drawable.ic_home,
            Resource.Drawable.ic_browse,
            Resource.Drawable.ic_create,
            Resource.Drawable.ic_profile
        };

        public readonly int[] TabIconsActive = {
            Resource.Drawable.ic_home_active,
            Resource.Drawable.ic_browse_active,
            Resource.Drawable.ic_create_active,
            Resource.Drawable.ic_profile_active
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
            var isFirstLoad = !AppSettings.Navigation.TabSettings.Any();
            for (var i = 0; i < TabIconsInactive.Length; i++)
            {
                HostFragment frag;
                switch (i)
                {
                    case 0:
                        frag = HostFragment.NewInstance(new FeedFragment());
                        break;
                    case 1:
                        if (isFirstLoad)
                            AppSettings.SetTabSettings(nameof(FeedFragment), new TabOptions());
                        frag = HostFragment.NewInstance(new PreSearchFragment());
                        break;
                    case 2:
                        frag = new HostFragment();
                        break;
                    case 3:
                        if (isFirstLoad)
                        {
                            AppSettings.SetTabSettings(nameof(ProfileFragment), new TabOptions());
                            AppSettings.SetTabSettings($"User_{nameof(ProfileFragment)}", new TabOptions { IsGridView = true });
                        }
                        frag = HostFragment.NewInstance(new ProfileFragment(AppSettings.User.Login));
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
