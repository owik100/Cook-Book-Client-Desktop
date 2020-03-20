using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop_Library.API.Interfaces;
using Cook_Book_Client_Desktop_Library.Models;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class RegisterViewModel : Screen
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _userName;
        private string _password;
        private string _passwordRepeat;
        private string _email;
        private string _registerInfoMessage;

        private IAPIHelper _apiHelper;
        private IEventAggregator _eventAggregator;

        public RegisterViewModel(IEventAggregator EventAggregator, IAPIHelper ApiHelper)
        {
            _eventAggregator = EventAggregator;
            _apiHelper = ApiHelper;
        }

        #region Props
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                NotifyOfPropertyChange(() => UserName);
                NotifyOfPropertyChange(() => CanRegister);
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                NotifyOfPropertyChange(() => Password);
                NotifyOfPropertyChange(() => CanRegister);
            }
        }

        public string PasswordRepeat
        {
            get { return _passwordRepeat; }
            set
            {
                _passwordRepeat = value;
                NotifyOfPropertyChange(() => PasswordRepeat);
                NotifyOfPropertyChange(() => CanRegister);
            }
        }

        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                NotifyOfPropertyChange(() => Email);
                NotifyOfPropertyChange(() => CanRegister);
            }
        }

        public string RegisterInfoMessage
        {
            get { return _registerInfoMessage; }
            set
            {
                _registerInfoMessage = value;
                NotifyOfPropertyChange(() => IsRegisterInfoMessageVisible);
                NotifyOfPropertyChange(() => RegisterInfoMessage);
            }
        }

        public bool IsRegisterInfoMessageVisible
        {
            get
            {
                bool output = false;

                if (RegisterInfoMessage?.Length > 0)
                {
                    output = true;
                }

                return output;
            }
        }

        public bool CanRegister
        {
            get
            {
                bool output = false;

                if (UserName?.Length > 0 && Email?.Length > 0 && Password?.Length > 0 && PasswordRepeat?.Length > 0)
                {
                    if (Password.Equals(PasswordRepeat))
                    {
                        RegisterInfoMessage = "";
                        output = true;
                    }
                    else
                    {
                        RegisterInfoMessage = "Hasła nie są takie same";
                    }

                }

                return output;
            }

        }
        #endregion

        public async Task Back()
        {
            try
            {
                await _eventAggregator.PublishOnUIThreadAsync(new LoginWindowEvent(), new CancellationToken());
            }
            catch (Exception ex)
            {

                _logger.Error("Got exception", ex);
            }
        }

        public async Task Register()
        {
            try
            {
                RegisterInfoMessage = "Rejestracja...";

                RegisterModel user = new RegisterModel
                {
                    UserName = UserName,
                    Email = Email,
                    Password = Password,
                    ConfirmPassword = PasswordRepeat
                };

                var result = await _apiHelper.Register(user);
                RegisterInfoMessage = "Rejestracja pomyślna. Możesz się teraz zalogować";

                Clear();
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                RegisterInfoMessage = ex.Message;
            }
        }

        private void Clear()
        {
            UserName = "";
            Password = "";
            Email = "";
            PasswordRepeat = "";
        }
    }
}
