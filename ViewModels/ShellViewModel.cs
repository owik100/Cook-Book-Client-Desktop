using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop_Library.API;
using Cook_Book_Client_Desktop_Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class ShellViewModel : Conductor<object>, IHandle<LogOnEvent>, IHandle<AddNewRecipeEvent>
    {
        private IEventAggregator _event;
        private RecipesViewModel _recipesViewModel;
        private ILoggedUser _loggedUser;
        private IAPIHelper _apiHelper;
        public ShellViewModel(IEventAggregator eventAggregator, RecipesViewModel recipesViewModel,
            ILoggedUser loggedUser, IAPIHelper aPIHelper)
        {
            _event = eventAggregator;
            _event.SubscribeOnPublishedThread(this);

            _recipesViewModel = recipesViewModel;
            _loggedUser = loggedUser;
            _apiHelper = aPIHelper;

            //Zawsze żądaj nowej instancji loginViewModelu
            ActivateItemAsync(IoC.Get<LoginViewModel>(), new CancellationToken());
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
            TryCloseAsync();
        }

        public async Task LogOut()
        {
            _loggedUser.LogOffUser();
            _apiHelper.LogOffUser();
            NotifyOfPropertyChange(() => IsLogged);
           await ActivateItemAsync(IoC.Get<LoginViewModel>(), new CancellationToken());
        }

        //public void Handle(AddNewRecipeEvent message)
        //{
        //    //Uruchom okno z nowa instancja
        //    ActivateItem(IoC.Get<AddRecipeViewModel>());
        //}

        public async Task HandleAsync(AddNewRecipeEvent message, CancellationToken cancellationToken)
        {
            //Uruchom okno z nowa instancja
            await ActivateItemAsync(IoC.Get<AddRecipeViewModel>(), cancellationToken);
        }

        //public void Handle(LogOnEvent message)
        //{
        //    ActivateItem(_recipesViewModel);
        //    NotifyOfPropertyChange(() => IsLogged);
        //}

        public async Task HandleAsync(LogOnEvent message, CancellationToken cancellationToken)
        {
            //Uruchom okno z nowa instancja
           await  ActivateItemAsync(_recipesViewModel, cancellationToken);
            NotifyOfPropertyChange(() => IsLogged);
        }
    }
}
