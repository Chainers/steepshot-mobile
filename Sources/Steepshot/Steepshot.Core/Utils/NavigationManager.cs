using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Utils
{
    public sealed class NavigationManager
    {
        private const string NavigationKey = "Navigation";
        private readonly ISaverService _saverService;

        private Navigation _navigation;
        public Navigation Navigation => _navigation ?? (_navigation = _saverService.Get<Navigation>(NavigationKey) ?? new Navigation());



        public NavigationManager(ISaverService saverService)
        {
            _saverService = saverService;
        }

        public void Save()
        {
            _saverService.Save(NavigationKey, Navigation);
        }

        public void SetTabSettings(string tabKey, TabOptions value)
        {
            if (Navigation.TabSettings.ContainsKey(tabKey))
                Navigation.TabSettings[tabKey] = value;
            else
                Navigation.TabSettings.Add(tabKey, value);
        }

        public TabOptions GetTabSettings(string tabKey)
        {
            if (!Navigation.TabSettings.ContainsKey(tabKey))
                Navigation.TabSettings.Add(tabKey, new TabOptions());

            return Navigation.TabSettings[tabKey];
        }

        public int SelectedTab
        {
            get => Navigation.SelectedTab;
            set
            {
                Navigation.SelectedTab = value;
                Save();
            }
        }
    }
}