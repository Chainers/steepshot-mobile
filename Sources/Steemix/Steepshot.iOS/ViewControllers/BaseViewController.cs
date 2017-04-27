using System;
using CoreGraphics;
using Foundation;
using Sweetshot.Library.HttpClient;
using UIKit;

namespace Steepshot.iOS
{
	public class BaseViewController : UIViewController
	{
		protected BaseViewController(IntPtr handle) : base(handle)
		{
		}

		protected UIView activeview;
		private nfloat scroll_amount = 0.0f;
		private nfloat bottom = 0.0f;
		private nfloat offset = 10.0f;
		private bool moveViewUp = false;
		protected NSObject showKeyboardToken;
		protected NSObject closeKeyboardToken;

		private static SteepshotApiClient _apiClient;

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			showKeyboardToken = NSNotificationCenter.DefaultCenter.AddObserver
			(UIKeyboard.DidShowNotification, KeyBoardUpNotification);
			closeKeyboardToken = NSNotificationCenter.DefaultCenter.AddObserver
			(UIKeyboard.WillHideNotification, KeyBoardDownNotification);
		}

		public override void ViewDidDisappear(bool animated)
		{
			NSNotificationCenter.DefaultCenter.RemoveObservers(new NSObject[2] { closeKeyboardToken, showKeyboardToken });
			showKeyboardToken.Dispose();
			closeKeyboardToken.Dispose();
			base.ViewDidDisappear(animated);
		}

		protected static SteepshotApiClient Api
		{
			get
			{
				if (_apiClient == null)
					SwitchApiAddress();
				return _apiClient;
			}
		}

		protected static void SwitchApiAddress()
		{
			if (UserContext.Instanse.Network == Constants.Steem)
			{
				if(UserContext.Instanse.IsDev)
					_apiClient = new SteepshotApiClient("https://qa.steepshot.org/api/v1/");
				else
					_apiClient = new SteepshotApiClient("https://steepshot.org/api/v1/");
			}
			else
			{
				if(UserContext.Instanse.IsDev)
					_apiClient = new SteepshotApiClient("https://qa.golos.steepshot.org/api/v1/");
				else
					_apiClient = new SteepshotApiClient("https://golos.steepshot.org/api/v1/");
			}
		}

		protected virtual void KeyBoardUpNotification(NSNotification notification)
		{
			CGRect r = UIKeyboard.FrameBeginFromNotification(notification);
			foreach (UIView view in this.View.Subviews)
			{
				if (view.IsFirstResponder)
					activeview = view;
			}
			bottom = (activeview.Frame.Y + activeview.Frame.Height + offset);
			scroll_amount = (r.Height - (View.Frame.Size.Height - bottom));
			if (scroll_amount > 0)
			{
				moveViewUp = true;
				ScrollTheView(moveViewUp);
			}
			else
				moveViewUp = false;
		}

		protected virtual void KeyBoardDownNotification(NSNotification notification)
		{
			if (moveViewUp)
				ScrollTheView(false);
		}

		protected virtual void ScrollTheView(bool move)
		{
			UIView.BeginAnimations(string.Empty, System.IntPtr.Zero);
			UIView.SetAnimationDuration(0.1);
			CGRect frame = View.Frame;
			if (move)
				frame.Y -= scroll_amount;
			else
			{
				frame.Y += scroll_amount;
				scroll_amount = 0;
			}
			View.Frame = frame;
			UIView.CommitAnimations(); 
		}
	}
}
