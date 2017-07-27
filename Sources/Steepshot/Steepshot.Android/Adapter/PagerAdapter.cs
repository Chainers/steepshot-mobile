using System.Collections.Generic;
using Android.Content;
using Android.Support.V4.App;
using Steepshot.Base;
using Steepshot.Fragment;


namespace Steepshot.Adapter
{
    public class PagerAdapter : FragmentPagerAdapter
    {
        public int[] TabIcos = new int[] {
            Resource.Drawable.ic_home,
            Resource.Drawable.ic_browse,
            Resource.Drawable.ic_camera_new,
            Resource.Drawable.ic_profile_new
        };
        Context _context;

        private List<Android.Support.V4.App.Fragment> _tabs = new List<Android.Support.V4.App.Fragment>();

        public PagerAdapter(FragmentManager fm, Context context) : base(fm)
        {
            _context = context;
            InitializeTabs();
        }

        public override int Count
        {
            get
            {
                return TabIcos.Length;
            }
        }

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

            for (var i = 0; i < TabIcos.Length; i++)
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
                    _tabs.Add(frag);
            }
        }
    }
}