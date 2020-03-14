using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop_Library.API;
using Cook_Book_Client_Desktop_Library.API.Interfaces;
using Cook_Book_Client_Desktop_Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class LoginViewModel : Screen
    {
		private string _userName= "Jan";
		private string _password= "Pwd12345.";
		private string _loginInfoMessage;

		private IAPIHelper _apiHelper;
		private IEventAggregator _eventAggregator;

		public LoginViewModel(IAPIHelper ApiHelper, IEventAggregator EventAggregator)
		{
			_apiHelper = ApiHelper;
			_eventAggregator = EventAggregator;
		}

		public string UserName
		{
			get { return _userName; }
			set { 
				_userName = value;
				NotifyOfPropertyChange(() => UserName);
				NotifyOfPropertyChange(() => CanLogIn);
			}
		}

		public string Password
		{
			get { return _password; }
			set { 
				_password = value;
				NotifyOfPropertyChange(() => Password);
				NotifyOfPropertyChange(() => CanLogIn);
			}
		}

		public bool IsLoginInfoMessageVisible
		{
			get {
				bool output = false;

				if(LoginInfoMessage?.Length> 0)
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
				LoginInfoMessage = "Connecting...";
				AuthenticatedUser user = await _apiHelper.Authenticate(UserName, Password);
				LoginInfoMessage = "";

				await _apiHelper.GetLoggedUserData(user.Access_Token);

				await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(), new CancellationToken());
			}
			catch (Exception ex)
			{
				LoginInfoMessage = ex.Message;
			}
		}

	}
}
