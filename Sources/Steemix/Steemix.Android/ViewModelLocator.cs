using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using Sweetshot.Library.HttpClient;

namespace Steemix.Droid
{
    public class ViewModelLocator
    {
        private static SteepshotApiClient _apiClient;
        //TODO:KOA: move to some config
        //<add key="sweetshot_url" value="http://138.197.40.124/api/v1/" />
        public static SteepshotApiClient Api
        {
            get
            {
                if (_apiClient == null)
                    _apiClient = new SteepshotApiClient("http://138.197.40.124/api/v1/");
                return _apiClient;
            }
        }


        public ViewModelLocator() { }

        private void RegisterServices() { }

        private void RegisterViewModels() { }

        public T GetViewModel<T>() where T : ViewModelBase
        {

            if (!SimpleIoc.Default.IsRegistered<T>())
                SimpleIoc.Default.Register<T>();

            return ServiceLocator.Current.GetInstance<T>();
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
