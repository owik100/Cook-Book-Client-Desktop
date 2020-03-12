using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop_Library.API;
using Cook_Book_Client_Desktop_Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class RecipesViewModel : Screen
    {
        private IRecipesEndPointAPI _recipesEndPointAPI;
        private BindingList<RecipeModel> _recipes;
        private IEventAggregator _event;

        public RecipesViewModel(IRecipesEndPointAPI recipesEndPointAPI, IEventAggregator eventAggregator)
        {
            _recipesEndPointAPI = recipesEndPointAPI;
            _event = eventAggregator;
        }

        protected async override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            await LoadRecipes();
        }

        public BindingList<RecipeModel> Recipes
        {
            get { return _recipes; }
            set
            {
                _recipes = value;
                NotifyOfPropertyChange(() => Recipes);
            }
        }

        private async Task LoadRecipes()
        {
            var recipes = await _recipesEndPointAPI.GetAllRecipesLoggedUser();
            Recipes = new BindingList<RecipeModel>(recipes);
        }

        public void AddRecipe()
        {
            _event.BeginPublishOnUIThread(new AddNewRecipeEvent());
        }
    }
}
