using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class ShellViewModel : Conductor<object>, IHandle<LogOnEvent>
    {
       private IEventAggregator _event;
        private RecipesViewModel _recipesViewModel;
        private SimpleContainer _container;
        public ShellViewModel(IEventAggregator eventAggregator, RecipesViewModel recipesViewModel,
            SimpleContainer container)
        {
            _event = eventAggregator;
            _event.Subscribe(this);
            _recipesViewModel = recipesViewModel;
            _container = container; 

            //Zawsze żądaj nowej instancji loginViewModelu
            ActivateItem(_container.GetInstance<LoginViewModel>());
        }

        public void Handle(LogOnEvent message)
        {
            ActivateItem(_recipesViewModel);   
        }
    }
}
