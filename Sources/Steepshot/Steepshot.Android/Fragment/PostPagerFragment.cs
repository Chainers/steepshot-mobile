using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V4.View;
using Android.Views;
using CheeseBind;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PostPagerFragment : BaseFragment
    {
        public Action<int> Close;
        private readonly BasePostPresenter _presenter;
        private readonly Post _openPost;
        private readonly Action<ActionType, Post> _postAction;
        private readonly Func<bool, Task> _fetchPresenter;
        private PostPagerAdapter<BasePostPresenter> _profilePagerAdapter;
        private PostPagerAdapter<BasePostPresenter> ProfilePagerAdapter
        {
            get
            {
                if (_profilePagerAdapter == null)
                {
                    _profilePagerAdapter = new PostPagerAdapter<BasePostPresenter>(_postPager, Context, _presenter);
                    _profilePagerAdapter.PostAction += _postAction;
                    _profilePagerAdapter.AutoLinkAction += AutoLinkAction;
                    _profilePagerAdapter.CloseAction += CloseAction;
                }
                return _profilePagerAdapter;
            }
        }

#pragma warning disable 649
        [BindView(Resource.Id.post_prev_pager)] private ViewPager _postPager;
#pragma warning restore 649

        public PostPagerFragment(BasePostPresenter presenter, Post openPost, Action<ActionType, Post> postAction, Func<bool, Task> fetchPresenter)
        {
            _presenter = presenter;
            _openPost = openPost;
            _postAction = postAction;
            _fetchPresenter = fetchPresenter;
            _presenter.SourceChanged += PresenterOnSourceChanged;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_post_pager_fragment, null);
                Cheeseknife.Bind(this, InflatedView);
            }
            ToggleTabBar(true);
            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                base.OnViewCreated(view, savedInstanceState);

                _postPager.SetClipToPadding(false);
                _postPager.SetPadding(Style.PostPagerMargin * 2, 0, Style.PostPagerMargin * 2, 0);
                _postPager.PageMargin = Style.PostPagerMargin;
                _postPager.PageScrolled += PostPagerOnPageScrolled;
                _postPager.Adapter = ProfilePagerAdapter;
                _postPager.SetPageTransformer(false, _profilePagerAdapter, (int)LayerType.None);
                _postPager.Visibility = ViewStates.Visible;
                ProfilePagerAdapter.NotifyDataSetChanged();
            }
            _postPager.SetCurrentItem(_presenter.IndexOf(_openPost), false);
        }

        private void PresenterOnSourceChanged(Status obj)
        {
            ProfilePagerAdapter.NotifyDataSetChanged();
        }

        public override void OnDetach()
        {
            _presenter.SourceChanged -= PresenterOnSourceChanged;
            base.OnDetach();
        }

        private void CloseAction()
        {
            ((BaseActivity)Activity).OnBackPressed();
        }

        public override bool OnBackPressed()
        {
            Close?.Invoke(_postPager.CurrentItem);
            return base.OnBackPressed();
        }

        private void PostPagerOnPageScrolled(object sender, ViewPager.PageScrolledEventArgs pageScrolledEventArgs)
        {
            if (pageScrolledEventArgs.Position == _presenter.Count)
            {
                if (!_presenter.IsLastReaded)
                    _fetchPresenter?.Invoke(false);
            }
        }
    }
}