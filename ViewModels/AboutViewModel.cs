using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class AboutViewModel : Screen
    {
        private string _appVersion;
        public AboutViewModel()
        {
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();   
        }

        #region Props

        public string AppVersion
        {
            get { return _appVersion; }
            set
            {
                _appVersion = $"Cook Book {value} ";
                NotifyOfPropertyChange(() => _appVersion);
            }
        }
        #endregion
    }
}
