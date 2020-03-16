using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop_Library.API;
using Cook_Book_Client_Desktop_Library.API.Interfaces;
using Cook_Book_Client_Desktop_Library.Models;
using Cook_Book_Client_Desktop_Library.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class ShellViewModel : Conductor<object>, IHandle<LogOnEvent>, IHandle<AddRecipeWindowEvent>, 
        IHandle<RegisterWindowEvent>, IHandle<LoginWindowEvent>, IHandle<RecipePreviewEvent>
    {
        private IEventAggregator _eventAggregator;
        private RecipesViewModel _recipesViewModel;
        private ILoggedUser _loggedUser;
        private IAPIHelper _apiHelper;

        public ShellViewModel(IEventAggregator eventAggregator, RecipesViewModel RecipesViewModel,
            ILoggedUser LoggedUser, IAPIHelper APIHelper)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            _recipesViewModel = RecipesViewModel;
            _loggedUser = LoggedUser;
            _apiHelper = APIHelper;

            //Zawsze żądaj nowej instancji loginViewModelu
            ActivateItemAsync(IoC.Get<LoginViewModel>(), new CancellationToken());
        }

        public bool IsLogged
        {
            get
            {
                bool output = false;

                if (string.IsNullOrWhiteSpace(_loggedUser.Token) == false)
                {
                    output = true;
                }

                return output;
            }
        }

        public async Task LogOut()
        {
            _loggedUser.LogOffUser();
            _apiHelper.LogOffUser();
            NotifyOfPropertyChange(() => IsLogged);
            await ActivateItemAsync(IoC.Get<LoginViewModel>(), new CancellationToken());
        }

        public async Task HandleAsync(AddRecipeWindowEvent message, CancellationToken cancellationToken)
        {
            if(message.AddOrEdit == AddOrEdit.Add)
            {
                await ActivateItemAsync(IoC.Get<AddRecipeViewModel>(), cancellationToken);
            }
            else if (message.AddOrEdit == AddOrEdit.Edit)
            {
                await ActivateItemAsync(IoC.Get<AddRecipeViewModel>(), cancellationToken);
                await _eventAggregator.PublishOnUIThreadAsync(new SendRecipe(message.RecipeModel), new CancellationToken());
            }            
        }

        public async Task HandleAsync(LogOnEvent message, CancellationToken cancellationToken)
        {
            await ActivateItemAsync(_recipesViewModel, cancellationToken);
            NotifyOfPropertyChange(() => IsLogged);
        }

        public async Task HandleAsync(RegisterWindowEvent message, CancellationToken cancellationToken)
        {
            //Uruchom okno z nowa instancja
            await ActivateItemAsync(IoC.Get<RegisterViewModel>(), cancellationToken);
        }

        public async Task HandleAsync(LoginWindowEvent message, CancellationToken cancellationToken)
        {
            //Uruchom okno z nowa instancja
            await  ActivateItemAsync(IoC.Get<LoginViewModel>(), new CancellationToken());
        }

        public void ExitApplication()
        {
            TryCloseAsync();
        }

        public async Task HandleAsync(RecipePreviewEvent message, CancellationToken cancellationToken)
        {
            await ActivateItemAsync(IoC.Get<RecipePreviewViewModel>(), new CancellationToken());
        }
    }
}
