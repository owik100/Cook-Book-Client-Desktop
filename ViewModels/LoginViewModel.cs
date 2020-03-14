using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop_Library.API;
using Cook_Book_Client_Desktop_Library.API.Interfaces;
using Cook_Book_Client_Desktop_Library.Helpers;
using Cook_Book_Client_Desktop_Library.Models;
using Meziantou.Framework.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class LoginViewModel : Screen
    {
        private string _userName;
        private string _password;
        private string _loginInfoMessage;
        private bool _remeberMe;

        private IAPIHelper _apiHelper;
        private IEventAggregator _eventAggregator;

        public LoginViewModel(IAPIHelper ApiHelper, IEventAggregator EventAggregator)
        {
            _apiHelper = ApiHelper;
            _eventAggregator = EventAggregator;

            LoadCredentials();
        }

        private void LoadCredentials()
        {
            var cred = WindowsCredentials.LoadLoginPassword();
            if (cred.UserName?.Length > 0 && cred.Password?.Length > 0)
            {
                UserName = cred.UserName;
                Password = cred.Password;
                RemeberMe = true;
            }
        }

        public bool RemeberMe
        {
            get { return _remeberMe; }
            set
            {
                _remeberMe = value;
                NotifyOfPropertyChange(() => RemeberMe);
            }
        }


        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                NotifyOfPropertyChange(() => UserName);
                NotifyOfPropertyChange(() => CanLogIn);
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                NotifyOfPropertyChange(() => Password);
                NotifyOfPropertyChange(() => CanLogIn);
            }
        }

        public bool IsLoginInfoMessageVisible
        {
            get
            {
                bool output = false;

                if (LoginInfoMessage?.Length > 0)
                {
                    output = true;
                }

                return output;
            }
        }

        public string LoginInfoMessage
        {
            get { return _loginInfoMessage; }
            set
            {
                _loginInfoMessage = value;
                NotifyOfPropertyChange(() => IsLoginInfoMessageVisible);
                NotifyOfPropertyChange(() => LoginInfoMessage);
            }
        }

        public bool CanLogIn
        {
            get
            {
                bool output = false;

                if (UserName?.Length > 0 && Password?.Length > 0)
                {
                    output = true;
                }

                return output;
            }

        }

        public async Task LogIn()
        {
            try
            {
                LoginInfoMessage = "Łączenie..";
                AuthenticatedUser user = await _apiHelper.Authenticate(UserName, Password);
                LoginInfoMessage = "";

                await _apiHelper.GetLoggedUserData(user.Access_Token);

                if (RemeberMe)
                {
                    WindowsCredentials.SaveLoginPassword(UserName, Password);
                }
                else
                {
                    WindowsCredentials.DeleteLoginPassword();
                }

                await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(), new CancellationToken());
            }
            catch (Exception ex)
            {
                LoginInfoMessage = ex.Message;
            }
        }

        public async Task RegisterForm()
        {
            try
            {
                await _eventAggregator.PublishOnUIThreadAsync(new RegisterWindowEvent(), new CancellationToken());
            }
            catch (Exception ex)
            {
                LoginInfoMessage = ex.Message;
            }
        }
    }
}
