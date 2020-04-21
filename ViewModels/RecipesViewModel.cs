using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop.Helpers;
using Cook_Book_Client_Desktop_Library.Helpers;
using Cook_Book_Shared_Code.API;
using Cook_Book_Shared_Code.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class RecipesViewModel : Screen, IHandle<ReloadAllRecipes>
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IRecipesEndPointAPI _recipesEndPointAPI;
        private BindingList<RecipeModelDisplay> _recipes;
        private IEventAggregator _eventAggregator;
        private ILoggedUser _loggedUser;

        List<RecipeModelDisplay> tempRecipes = new List<RecipeModelDisplay>();
        private bool _isPublicRecipes;
        private bool _isUserRecipes;

        public RecipesViewModel(IRecipesEndPointAPI RecipesEndPointAPI, ILoggedUser loggedUser, IEventAggregator EventAggregator)
        {
            _recipesEndPointAPI = RecipesEndPointAPI;
            _loggedUser = loggedUser;
            _eventAggregator = EventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        public async Task HandleAsync(ReloadAllRecipes message, CancellationToken cancellationToken)
        {
           if(message!=null)
            {
                if(message.UserOrPublic == UserOrPublic.User)
                {
                    IsPublicRecipes = false;
                    IsUserRecipes = true;
                    await LoadUserRecipes();
                }
                else
                {
                    IsPublicRecipes = true;
                    IsUserRecipes = false;
                    await LoadPublicRecipes();
                }
               
                await LoadImages();
            }
        }

        #region Props
        public BindingList<RecipeModelDisplay> Recipes
        {
            get { return _recipes; }
            set
            {
                _recipes = value;
                NotifyOfPropertyChange(() => Recipes);
            }
        } 
        public bool IsPublicRecipes
        {
            get { return _isPublicRecipes; }
            set
            {
                _isPublicRecipes = value;
                NotifyOfPropertyChange(() => IsPublicRecipes);
            }
        }
        public bool IsUserRecipes
        {
            get { return _isUserRecipes; }
            set
            {
                _isUserRecipes = value;
                NotifyOfPropertyChange(() => IsUserRecipes);
            }
        }
        #endregion

        private async Task LoadUserRecipes()
        {
            try
            {
                tempRecipes.Clear();
                var recipes = await _recipesEndPointAPI.GetRecipesLoggedUser();

                foreach (var item in recipes)
                {
                    bool displayAsPublic = false;
                    if (item.IsPublic && _loggedUser.UserName == item.UserName)
                    {
                        displayAsPublic = true;
                    }


                    tempRecipes.Add(new RecipeModelDisplay
                    {
                        UserName = item.UserName,
                        Ingredients = item.Ingredients,
                        Instruction = item.Instruction,
                        Name = item.Name,
                        NameOfImage = item.NameOfImage,
                        UserId = item.UserId,
                        IsPublic = item.IsPublic,
                        RecipeId = item.RecipeId,
                        ImagePath = item.ImagePath,

                        DisplayAsPublic = displayAsPublic
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }

        private async Task LoadPublicRecipes()
        {
            try
            {
                tempRecipes.Clear();
                var recipes = await _recipesEndPointAPI.GetPublicRecipes();

                foreach (var item in recipes)
                {
                    bool displayAsPublic = false;
                    if (item.IsPublic && _loggedUser.UserName == item.UserName)
                    {
                        displayAsPublic = true;
                    }


                    tempRecipes.Add(new RecipeModelDisplay
                    {
                        UserName = item.UserName,
                        Ingredients = item.Ingredients,
                        Instruction = item.Instruction,
                        Name = item.Name,
                        NameOfImage = item.NameOfImage,
                        UserId = item.UserId,
                        IsPublic = item.IsPublic,
                        RecipeId = item.RecipeId,
                        ImagePath = item.ImagePath,

                        DisplayAsPublic = displayAsPublic
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }

        private async Task LoadImages()
        {
            try
            {
                List<string> DontDeletetheseImages = new List<string>();

                foreach (var item in tempRecipes)
                {
                    if (item.NameOfImage == null)
                    {
                        item.ImagePath = ImageConstants.DefaultImage;
                        continue;
                    }

                    if (TempData.ImageExistOnDisk(item.NameOfImage))
                    {
                        item.ImagePath = TempData.GetImagePath(item.NameOfImage);
                        DontDeletetheseImages.Add(item.NameOfImage);
                        continue;
                    }

                    var downloadStatus = await _recipesEndPointAPI.DownloadImage(item.NameOfImage);

                    if(downloadStatus)
                    {
                        item.ImagePath = TempData.GetImagePath(item.NameOfImage);
                        DontDeletetheseImages.Add(item.NameOfImage);
                    }
                   
                }

                TempData.DeleteUnusedImages(DontDeletetheseImages);

                Recipes = new BindingList<RecipeModelDisplay>(tempRecipes);
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
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
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        } 
        
        public async Task PublicRecipes()
        {
            try
            {
                await _eventAggregator.PublishOnUIThreadAsync(new ReloadAllRecipes(UserOrPublic.Public), new CancellationToken());
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }

        public async Task UserRecipes()
        {
            try
            {
                await _eventAggregator.PublishOnUIThreadAsync(new ReloadAllRecipes(UserOrPublic.User), new CancellationToken());
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }



        public async Task RecipePreview(RecipeModelDisplay model)
        {
            try
            {
                RecipeModel recipeModel = new RecipeModel
                {
                    ImagePath = model.ImagePath,
                    Ingredients = model.Ingredients,
                    Instruction = model.Instruction,
                    UserId = model.UserId,
                    RecipeId = model.RecipeId,
                    IsPublic = model.IsPublic,
                    Name = model.Name,
                    NameOfImage = model.NameOfImage,
                    UserName = model.UserName,
                };
                await _eventAggregator.PublishOnUIThreadAsync(new RecipePreviewEvent(recipeModel), new CancellationToken());
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }

        }   
    }
}
