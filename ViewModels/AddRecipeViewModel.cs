using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop_Library.API;
using Cook_Book_Client_Desktop_Library.Models;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class AddRecipeViewModel : Screen
    {
        private string _recipeName;
        private string _recipeIntegradts;
        private string _recipeInstructions;

        private IRecipesEndPointAPI _recipesEndPointAPI;
        private IEventAggregator _eventAggregator;

        public AddRecipeViewModel(IRecipesEndPointAPI RecipesEndPointAPI, IEventAggregator EventAggregator)
        {
            _recipesEndPointAPI = RecipesEndPointAPI;
            _eventAggregator = EventAggregator;
        }

        public string RecipeName
        {
            get { return _recipeName; }
            set { 
                _recipeName = value;
                NotifyOfPropertyChange(() => RecipeName);
            }
        }

        public string RecipeIngredients
        {
            get { return _recipeIntegradts; }
            set { 
                _recipeIntegradts = value;
                NotifyOfPropertyChange(() => RecipeIngredients);
            }
        }

        public string RecipeInstructions
        {
            get { return _recipeInstructions; }
            set { 
                _recipeInstructions = value;
                NotifyOfPropertyChange(() => RecipeInstructions);
            }
        }

        public async Task AddRecipeSubmit()
        {
            try
            {
                //TODO Zrobic fabryke na to
                RecipeModel recipeModel = new RecipeModel
                {
                    Name = RecipeName,
                    Ingredients = RecipeIngredients.Split(',').ToList(),
                    Instruction = RecipeInstructions,

                };

                await _recipesEndPointAPI.InsertRecipe(recipeModel);
                await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(), new CancellationToken());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        public async Task Back()
        {
            await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(), new CancellationToken());
        }
    }
}
