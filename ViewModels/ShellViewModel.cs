using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
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
        public ShellViewModel(IEventAggregator eventAggregator, RecipesViewModel recipesViewModel, AddRecipeViewModel addRecipeViewModel)
        {
            _event = eventAggregator;
            _event.Subscribe(this);

            _recipesViewModel = recipesViewModel;
            _addRecipeViewModel = addRecipeViewModel;

            //Zawsze żądaj nowej instancji loginViewModelu
            ActivateItem(IoC.Get<LoginViewModel>());
        }

        public void Handle(LogOnEvent message)
        {
            ActivateItem(_recipesViewModel);      
        }
        public void Handle(AddNewRecipeEvent message)
        {
            //Uruchom okno z nowa instancja
            ActivateItem(IoC.Get<AddRecipeViewModel>());
        }
    }
}
