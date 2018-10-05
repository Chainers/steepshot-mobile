using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Utils
{
    public class SettingsManager
    {
        private const string AppSettingsKey = "AppSettings";
        private readonly ISaverService _saverService;

        private AppSettingsModel _appSettingsModel;
        public AppSettingsModel Settings => _appSettingsModel ?? (_appSettingsModel = _saverService.Get<AppSettingsModel>(AppSettingsKey) ?? new AppSettingsModel());

        public SettingsManager(ISaverService saverService)
        {
            _saverService = saverService;
        }

        public void Save()
        {
            _saverService.Save(AppSettingsKey, _appSettingsModel);
        }
    }
}
