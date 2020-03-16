using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop_Library.API.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class RecipePreviewViewModel : Screen, IHandle<SendRecipe>
    {
        private string _recipeName;
        private List<string> _recipeIntegradts;
        private string _recipeInstructions;
        private int _recipeId;

        private IRecipesEndPointAPI _recipesEndPointAPI;
        private IEventAggregator _eventAggregator;

        public RecipePreviewViewModel(IRecipesEndPointAPI RecipesEndPointAPI, IEventAggregator EventAggregator)
        {
            _recipesEndPointAPI = RecipesEndPointAPI;
            _eventAggregator = EventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        public async Task HandleAsync(SendRecipe message, CancellationToken cancellationToken)
        {
            _recipeId = message.RecipeModel.RecipeId;

            RecipeName = message.RecipeModel.Name;
            RecipeIngredients =  (message.RecipeModel.Ingredients).ToList();
            RecipeInstructions = message.RecipeModel.Instruction;

            await Task.CompletedTask;
        }

        public string RecipeName
        {
            get { return _recipeName; }
            set
            {
                _recipeName = value;
                NotifyOfPropertyChange(() => RecipeName);
            }
        }

        public List<string>RecipeIngredients
        {
            get { return _recipeIntegradts; }
            set
            {
                _recipeIntegradts = value;
                NotifyOfPropertyChange(() => RecipeIngredients);
            }
        }

        public string RecipeInstructions
        {
            get { return _recipeInstructions; }
            set
            {
                _recipeInstructions = value;
                NotifyOfPropertyChange(() => RecipeInstructions);
            }
        }

        public async Task Back()
        {
            await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(), new CancellationToken());
        }
    }
}
