﻿using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop_Library.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class LoginViewModel : Screen
    {
		private string _userName;
		private string _password;
		private IAPIHelper _apiHelper;
		private IEventAggregator _event;

		public LoginViewModel(IAPIHelper apiHelper, IEventAggregator eventAggregator)
		{
			_apiHelper = apiHelper;
			_event = eventAggregator;
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

		public bool IsErrorVisible
		{
			get {
				bool output = false;

				if(ErrorMessage?.Length> 0)
				{
					output = true;
				}

				return output;
			}
		}

		private string _errorMessage;
		public string ErrorMessage
		{
			get { return _errorMessage; }
			set
			{
				_errorMessage = value;
				NotifyOfPropertyChange(() => IsErrorVisible);
				NotifyOfPropertyChange(() => ErrorMessage);	
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
				ErrorMessage = "Connecting...";
				var result = await _apiHelper.Authenticate(UserName, Password);
				ErrorMessage = "";

				await _apiHelper.GetLoggedUserData(result.Access_Token);

				_event.BeginPublishOnUIThread(new LogOnEvent());
			}
			catch (Exception ex)
			{

				ErrorMessage = ex.Message;
			}
		}

	}
}
