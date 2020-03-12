﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private IRecipesEndPointAPI _iRecipesEndPointAPI;
        private IEventAggregator _eventAggregator;

        public AddRecipeViewModel(IRecipesEndPointAPI IRecipesEndPointAPI, IEventAggregator EventAggregator)
        {
            _iRecipesEndPointAPI = IRecipesEndPointAPI;
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

        public string RecipeIntegradts
        {
            get { return _recipeIntegradts; }
            set { 
                _recipeIntegradts = value;
                NotifyOfPropertyChange(() => RecipeIntegradts);
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
                    Ingredients = RecipeIntegradts.Split(',').ToList(),
                    Instruction = RecipeInstructions,

                };

                await _iRecipesEndPointAPI.InsertRecipe(recipeModel);
                _eventAggregator.PublishOnUIThread(new LogOnEvent());
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }

        }

        public void Back()
        {
            _eventAggregator.PublishOnUIThread(new LogOnEvent());
        }
    }
}
