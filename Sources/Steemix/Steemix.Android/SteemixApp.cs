using System;
using Android.App;
using Android.Runtime;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace Steemix.Droid
{
	[Application]
	public class SteemixApp : Application
	{
		public SteemixApp(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
        
		public override void OnCreate()
		{
			base.OnCreate();
			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
		}

		private static ViewModelLocator viewModelLocator;

		public static ViewModelLocator ViewModelLocator
		{
			get { return viewModelLocator ?? (viewModelLocator = new ViewModelLocator()); }
		}
	}
}
