using System;
using System.Threading.Tasks;
using Android.App;
using Android.Runtime;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using Steemstagram.Shared;

namespace Steemix.Android
{
	[Application]
	public class SteemixApp : Application
	{
		public SteemixApp(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

		public static Manager Manager;
		public override void OnCreate()
		{
			base.OnCreate();
			Manager = new Manager();
			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
		}

		private static ViewModelLocator viewModelLocator;

		public static ViewModelLocator ViewModelLocator
		{
			get { return viewModelLocator ?? (viewModelLocator = new ViewModelLocator()); }
		}
	}
}
