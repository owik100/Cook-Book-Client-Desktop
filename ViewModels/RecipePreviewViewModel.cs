﻿using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Shared_Code.API;
using Cook_Book_Shared_Code.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class RecipePreviewViewModel : Screen, IHandle<SendRecipe>
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private RecipeModel currentRecipe;

        private string _recipeName;
        private List<string> _recipeIntegradts;
        private string _recipeInstructions;
        private string _imagePath="";
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
            try
            {
                currentRecipe = message.RecipeModel;

                //TODO Mapper
                _recipeId = currentRecipe.RecipeId;
                RecipeName = currentRecipe.Name;
                RecipeIngredients = (currentRecipe.Ingredients).ToList();
                RecipeInstructions = currentRecipe.Instruction;
                ImagePath = currentRecipe.ImagePath;

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
            }  
        }

        #region Props
        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                _imagePath = value;
                NotifyOfPropertyChange(() => ImagePath);
            }
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

        public List<string> RecipeIngredients
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
        #endregion


        public async Task Back()
        {
            try
            {
                await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(reloadNeeded: false), new CancellationToken());
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
            
        }

        public async Task EditRecipe()
        {
            try
            {
                await _eventAggregator.PublishOnUIThreadAsync(new AddRecipeWindowEvent(AddOrEdit.Edit, currentRecipe), new CancellationToken());
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }

        public async Task DeleteRecipe()
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show($"Na pewno chcesz usunąć {currentRecipe.Name} ?", "Potwierdź usunięcie", MessageBoxButton.YesNo,MessageBoxImage.Warning);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    var result = await _recipesEndPointAPI.DeleteRecipe(currentRecipe.RecipeId.ToString());

                    await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(reloadNeeded:true), new CancellationToken());
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }
    }
}
