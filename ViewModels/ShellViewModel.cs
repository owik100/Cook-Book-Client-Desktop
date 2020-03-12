using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop_Library.API;
using Cook_Book_Client_Desktop_Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class ShellViewModel : Conductor<object>, IHandle<LogOnEvent>, IHandle<AddNewRecipeEvent>
    {
        private IEventAggregator _event;
        private RecipesViewModel _recipesViewModel;
        private AddRecipeViewModel _addRecipeViewModel;
        private ILoggedUser _loggedUser;
        private IAPIHelper _apiHelper;
        public ShellViewModel(IEventAggregator eventAggregator, RecipesViewModel recipesViewModel, AddRecipeViewModel addRecipeViewModel,
            ILoggedUser loggedUser, IAPIHelper aPIHelper)
        {
            _event = eventAggregator;
            _event.Subscribe(this);

            _recipesViewModel = recipesViewModel;
            _addRecipeViewModel = addRecipeViewModel;
            _loggedUser = loggedUser;
            _apiHelper = aPIHelper;

            //Zawsze żądaj nowej instancji loginViewModelu
            ActivateItem(IoC.Get<LoginViewModel>());
        }

        public void Handle(LogOnEvent message)
        {
            ActivateItem(_recipesViewModel);
            NotifyOfPropertyChange(() => IsLogged);
        }
        public void Handle(AddNewRecipeEvent message)
        {
            //Uruchom okno z nowa instancja
            ActivateItem(IoC.Get<AddRecipeViewModel>());
        }

        public bool IsLogged
        {
            get {
                bool output = false;

                if (string.IsNullOrWhiteSpace(_loggedUser.Token) == false)
                {
                    output = true;
                }

                return output;
            }
        }

        public void ExitApplication()
        {
            TryClose();
        }

        public void LogOut()
        {
            _loggedUser.LogOffUser();
            _apiHelper.LogOffUser();
            NotifyOfPropertyChange(() => IsLogged);
            ActivateItem(IoC.Get<LoginViewModel>());
        }
    }
}
