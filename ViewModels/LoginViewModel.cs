using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop_Library.Helpers;
using Cook_Book_Shared_Code.API;
using Cook_Book_Shared_Code.Models;
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
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _userName;
        private string _password;
        private string _loginInfoMessage;
        private bool _remeberMe;
        private bool _duringOperation;

        private IAPIHelper _apiHelper;
        private IEventAggregator _eventAggregator;


        public LoginViewModel(IAPIHelper ApiHelper, IEventAggregator EventAggregator)
        {
            _apiHelper = ApiHelper;
            _eventAggregator = EventAggregator;

            LoadCredentials();
        }

        #region Props

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

                if (UserName?.Length > 0 && Password?.Length > 0 && !_duringOperation)
                {
                    output = true;
                }

                return output;
            }

        }
        #endregion

        private void LoadCredentials()
        {
            try
            {
                var cred = WindowsCredentials.LoadLoginPassword();
                if (cred.UserName?.Length > 0 && cred.Password?.Length > 0)
                {
                    UserName = cred.UserName;
                    Password = cred.Password;
                    RemeberMe = true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
            }        
        }


        public async Task LogIn()
        {
            try
            {
                _duringOperation = true;
                NotifyOfPropertyChange(() => CanLogIn);
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

                await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(reloadNeeded: true), new CancellationToken());
                _duringOperation = false;
                NotifyOfPropertyChange(() => CanLogIn);
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                LoginInfoMessage = ex.Message;
                _duringOperation = false;
                NotifyOfPropertyChange(() => CanLogIn);
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
                _logger.Error("Got exception", ex);
                LoginInfoMessage = ex.Message;
            }
        }
    }
}
