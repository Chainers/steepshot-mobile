﻿using System;
using System.Threading;
using PureLayout.Net;
using Steepshot.Core.Errors;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public class RegistrationViewController : BaseViewControllerWithPresenter<LolPresenter>
    {
        private UITextField _username;
        private UITextField _email;
        private UIButton _createAcc;
        private UIActivityIndicatorView _loader;
        private UIActivityIndicatorView _registrationLoader;
        private UILabel _usernameLabel;
        private UILabel _emailLabel;
        private UIView _usernameUnderline;
        private UIView _emailUnderline;
        private Timer _timer;
        private StringHelper _mailChecker;

        private void OnTimer(object state)
        {
            InvokeOnMainThread(() =>
            {
                CheckLogin();
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.White;

            var tapGesture = new UITapGestureRecognizer(() => 
            {
                _email.ResignFirstResponder();
                _username.ResignFirstResponder();
            });
            View.AddGestureRecognizer(tapGesture);
            View.UserInteractionEnabled = true;

            _timer = new Timer(OnTimer);
            _mailChecker = new StringHelper();

            _createAcc = new UIButton();
            _createAcc.SetTitle("Create account", UIControlState.Normal);
            Constants.CreateShadow(_createAcc, Constants.R231G72B0, 0.5f, 25, 10, 12);
            _createAcc.Font = Constants.Bold14;
            _createAcc.TouchDown += CreateAccount;
            View.Add(_createAcc);

            _createAcc.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            _createAcc.AutoSetDimension(ALDimension.Height, 50);
            _createAcc.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            _createAcc.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);

            _emailUnderline = new UIView();
            _emailUnderline.BackgroundColor = UIColor.FromRGB(240, 240, 240);
            View.AddSubview(_emailUnderline);

            _emailUnderline.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, _createAcc, -50);
            _emailUnderline.AutoSetDimension(ALDimension.Height, 1);
            _emailUnderline.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 35);
            _emailUnderline.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 35);

            _emailLabel = new UILabel();
            _emailLabel.TextColor = Constants.R255G34B5;
            _emailLabel.Text = "invalid email";
            _emailLabel.Font = Constants.Semibold12;
            _emailLabel.Hidden = true;
            View.AddSubview(_emailLabel);

            _emailLabel.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _emailUnderline);
            _emailLabel.AutoPinEdge(ALEdge.Right, ALEdge.Right, _emailUnderline);

            _email = new UITextField();
            var emailDelegate = new BaseTextFieldDelegate();
            _email.Delegate = emailDelegate;
            emailDelegate.DoneTapped += () =>
            {
                _email.ResignFirstResponder();
                CreateAccount(null, null);
            };
            _email.Placeholder = "Email address";
            _email.KeyboardType = UIKeyboardType.EmailAddress;
            _email.AutocorrectionType = UITextAutocorrectionType.No;
            _email.AutocapitalizationType = UITextAutocapitalizationType.None;
            _email.TextAlignment = UITextAlignment.Center;
            _email.ReturnKeyType = UIReturnKeyType.Go;
            _email.EditingChanged += CheckMail;

            View.AddSubview(_email);

            _email.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, _emailUnderline, -7);
            _email.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 35);
            _email.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 35);
            _email.AutoSetDimension(ALDimension.Height, 30);

            _usernameUnderline = new UIView();
            _usernameUnderline.BackgroundColor = Constants.R240G240B240;
            View.AddSubview(_usernameUnderline);

            _usernameUnderline.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, _email, -35);
            _usernameUnderline.AutoSetDimension(ALDimension.Height, 1);
            _usernameUnderline.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 35);
            _usernameUnderline.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 35);

            _usernameLabel = new UILabel();
            _usernameLabel.TextColor = Constants.R255G34B5;
            _usernameLabel.Text = "username already taken";
            _usernameLabel.Font = Constants.Semibold12;
            _usernameLabel.Hidden = true;
            View.AddSubview(_usernameLabel);

            _usernameLabel.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _usernameUnderline);
            _usernameLabel.AutoPinEdge(ALEdge.Right, ALEdge.Right, _usernameUnderline);

            _username = new UITextField();
            var usernameDelegate = new UsernameDelegate();
            _username.Delegate = usernameDelegate;
            usernameDelegate.DoneTapped += () => 
            {
                _email.BecomeFirstResponder();
            };
            _username.Placeholder = "Username";
            _username.AutocorrectionType = UITextAutocorrectionType.No;
            _username.AutocapitalizationType = UITextAutocapitalizationType.None;
            _username.TextAlignment = UITextAlignment.Center;
            _username.ReturnKeyType = UIReturnKeyType.Next;
            _username.EditingChanged += (object sender, EventArgs e) => 
            {
                _timer.Change(500, Timeout.Infinite);
            };
            View.AddSubview(_username);

            _username.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, _usernameUnderline, -7);
            _username.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 35);
            _username.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 35);
            _username.AutoSetDimension(ALDimension.Height, 30);

            _loader = new UIActivityIndicatorView();
            _loader.Color = Constants.R231G72B0;
            _loader.HidesWhenStopped = true;

            View.AddSubview(_loader);

            _loader.AutoPinEdge(ALEdge.Left, ALEdge.Right, _username);
            _loader.AutoAlignAxis(ALAxis.Horizontal, _username);

            _registrationLoader = new UIActivityIndicatorView();
            _registrationLoader.Color = UIColor.White;
            _registrationLoader.HidesWhenStopped = true;

            View.AddSubview(_registrationLoader);

            _registrationLoader.AutoAlignAxis(ALAxis.Horizontal, _createAcc);
            _registrationLoader.AutoAlignAxis(ALAxis.Vertical, _createAcc);

            SetBackButton();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            Constants.CreateGradient(_createAcc, 25);
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;
            NavigationItem.Title = "Create account"; //AppSettings.LocalizationManager.GetText(LocalizationKeys.Comments);
        }

        private void CheckMail(object sender, EventArgs e)
        {
            var isValid = _mailChecker.IsValidEmail(_email.Text);

            if (isValid)
            {
                _emailUnderline.BackgroundColor = Constants.R240G240B240;
                _emailLabel.Hidden = true;
            }
            else
            {
                _emailUnderline.BackgroundColor = Constants.R255G0B0;
                _emailLabel.Hidden = false;
            }
        }

        private async void CheckLogin()
        {
            if(_username.Text.Length < 3)
            {
                _usernameLabel.Text = "min length 3 symbols";
                _usernameUnderline.BackgroundColor = Constants.R255G0B0;
                _usernameLabel.Hidden = false;
                return;
            }

            _loader.StartAnimating();

            var error = await _presenter.TryGetAccountInfo(_username.Text);

            if (error is CanceledError)
                return;

            if (error?.Message == "User not found")
            {
                _usernameUnderline.BackgroundColor = Constants.R240G240B240;
                _usernameLabel.Hidden = true;
            }
            else
            {
                _usernameLabel.Text = "username already taken";
                _usernameUnderline.BackgroundColor = Constants.R255G0B0;
                _usernameLabel.Hidden = false;
            }

            _loader.StopAnimating();
        }

        private void CreateAccount(object sender, EventArgs e)
        {
            /*
            if (_loader.IsAnimating)
                return;

            if (string.IsNullOrEmpty(_username.Text))
            {
                _usernameUnderline.BackgroundColor = Constants.R255G0B0;
                _usernameLabel.Hidden = false;
                _usernameLabel.Text = "invalid username";
                return;
            }

            CheckMail(null, null);
            if (!_usernameLabel.Hidden || !_emailLabel.Hidden)
                return;
            
            _registrationLoader.StartAnimating();
            _createAcc.SetTitleColor(UIColor.Clear, UIControlState.Normal); */

            var myViewController = new RegistrationCompletionViewController();
            NavigationController.PushViewController(myViewController, true);
        }
    }
}
