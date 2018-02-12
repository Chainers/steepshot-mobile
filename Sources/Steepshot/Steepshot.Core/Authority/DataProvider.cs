using System.Collections.Generic;
using System.Linq;
using Steepshot.Core.Services;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Authority
{
    public sealed class DataProvider : IDataProvider
    {
        private readonly Dictionary<string, LocalizationModel> _localizationModel;
        private readonly List<UserInfo> _set;
        private readonly ISaverService _saverService;

        public DataProvider(ISaverService saverService)
        {
            _saverService = saverService;
            _set = _saverService.Get<List<UserInfo>>(Constants.UserContextKey);
            _localizationModel = _saverService.Get<Dictionary<string, LocalizationModel>>(Constants.Localization);
        }

        public List<UserInfo> Select()
        {
            return _set;
        }

        public void Delete(UserInfo userInfo)
        {
            for (var i = 0; i < _set.Count; i++)
            {
                if (_set[i].Id == userInfo.Id)
                {
                    _set.RemoveAt(i);
                    break;
                }
            }
            Save();
        }

        public void Insert(UserInfo currentUserInfo)
        {
            if (currentUserInfo.Id == 0)
                currentUserInfo.Id = _set.Any() ? _set.Max(i => i.Id) + 1 : 1;
            _set.Add(currentUserInfo);
            Save();
        }

        public List<UserInfo> Select(KnownChains chain)
        {
            return _set.Where(i => i.Chain == chain).ToList();
        }

        public void Update(UserInfo userInfo)
        {
            for (var i = 0; i < _set.Count; i++)
            {
                if (_set[i].Id == userInfo.Id)
                {
                    _set[i] = userInfo;
                    break;
                }
            }
            Save();
        }

        private void Save()
        {
            _saverService.Save(Constants.UserContextKey, _set);
        }

        public LocalizationModel SelectLocalization(string lang)
        {
            if (_localizationModel.ContainsKey(lang))
                return _localizationModel[lang];
            return null;
        }

        public void UpdateLocalization(LocalizationModel model)
        {
            if (_localizationModel.ContainsKey(model.Lang))
            {
                _localizationModel[model.Lang] = model;
            }
            else
            {
                _localizationModel.Add(model.Lang, model);
            }

            _saverService.Save(Constants.Localization, _localizationModel);
        }
    }
}
