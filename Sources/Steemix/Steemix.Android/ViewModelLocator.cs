using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace Steemix.Droid
{
    public class ViewModelLocator {
        public ViewModelLocator() {}

        private void RegisterServices() {}

        private void RegisterViewModels() {}

        public T GetViewModel<T>() where T : ViewModelBase {

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
