﻿using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop_Library.API;
using Cook_Book_Client_Desktop_Library.API.Interfaces;
using Cook_Book_Client_Desktop_Library.Helpers;
using Cook_Book_Client_Desktop_Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class RecipesViewModel : Screen
    {
        private IRecipesEndPointAPI _recipesEndPointAPI;
        private BindingList<RecipeModel> _recipes;
        private IEventAggregator _eventAggregator;

        List<RecipeModel> tempRecipes = new List<RecipeModel>();

        public RecipesViewModel(IRecipesEndPointAPI RecipesEndPointAPI, IEventAggregator EventAggregator)
        {
            _recipesEndPointAPI = RecipesEndPointAPI;
            _eventAggregator = EventAggregator;
        }

        protected async override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            await LoadRecipes();
            await LoadImages();

            //await Task.WhenAll(task1, task2);
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
            try
            {
                tempRecipes = await _recipesEndPointAPI.GetAllRecipesLoggedUser();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }

        private async Task LoadImages()
        {
            try
            {
                foreach (var item in tempRecipes)
                {
                    if (item.NameOfImage == null)
                    {
                        //TODO dać ścieżkę na temp do configa
                        item.ImagePath = "pack://application:,,,/Resources/food template.png";
                        continue;
                    }

                    if (TempData.ImageExistOnDisk(item.NameOfImage))
                    {
                        item.ImagePath = TempData.GetImagePath(item.NameOfImage);
                        continue;
                    }

                    var downloadStatus = await _recipesEndPointAPI.DownloadImage(item.NameOfImage);

                    if(downloadStatus)
                    {
                        item.ImagePath = TempData.GetImagePath(item.NameOfImage);
                    }
                   
                }

                Recipes = new BindingList<RecipeModel>(tempRecipes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }

        }

        public async Task AddRecipe()
        {
            try
            {
                await _eventAggregator.PublishOnUIThreadAsync(new AddRecipeWindowEvent(AddOrEdit.Add), new CancellationToken());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }

        public async Task RecipePreview(RecipeModel model)
        {
            try
            {
                await _eventAggregator.PublishOnUIThreadAsync(new RecipePreviewEvent(model), new CancellationToken());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }

        }

        public async Task EditRecipe(RecipeModel model)
        {
            try
            {
                await _eventAggregator.PublishOnUIThreadAsync(new AddRecipeWindowEvent(AddOrEdit.Edit, model), new CancellationToken());

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }

        public async Task DeleteRecipe(RecipeModel model)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show($"Na pewno chcesz usunąć {model.Name} ?", "Potwierdź usunięcie", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    var result = await _recipesEndPointAPI.DeleteRecipe(model.RecipeId.ToString());

                    await LoadRecipes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }
    }
}
