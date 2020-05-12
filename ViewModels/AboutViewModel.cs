using Caliburn.Micro;
using System.Reflection;

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

        public void GitHub()
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/owik100/Cook-Book-Client-Desktop");
        }
    }
}
