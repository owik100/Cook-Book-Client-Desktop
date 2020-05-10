using AutoMapper;
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
using System.Linq;
using System.Windows.Media.Imaging;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class RecipesViewModel : Screen, IHandle<ReloadAllRecipes>
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private userOrPublicOrFavourites nowOpen;

        private IRecipesEndPointAPI _recipesEndPointAPI;
        private BindingList<RecipeModelDisplay> _recipes;
        private IEventAggregator _eventAggregator;
        private ILoggedUser _loggedUser;
        private IMapper _mapper;

        List<RecipeModelDisplay> tempRecipes = new List<RecipeModelDisplay>();
        private bool _isPublicRecipes;
        private bool _isUserRecipes;
        private bool _isFavouriteRecipes;
        private bool _canNext;
        private bool _canPrevious;

        private int pageSize = 30;
        private int totalPages = 1;
        private int pageNumberUserRecipes = 1;
        private int pageNumberPublicRecipes = 1;
        private int pageNumberFavouritesRecipes = 1;

        private string _pageInfo;

        private bool _noRecipes;
        private bool _noFavouriteRecipes;

        public RecipesViewModel(IRecipesEndPointAPI RecipesEndPointAPI, ILoggedUser loggedUser, IEventAggregator EventAggregator, IMapper mapper)
        {
            _mapper = mapper;
            _recipesEndPointAPI = RecipesEndPointAPI;
            _loggedUser = loggedUser;
            _eventAggregator = EventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        public async Task HandleAsync(ReloadAllRecipes message, CancellationToken cancellationToken)
        {
            if (message != null)
            {
                nowOpen = message.userOrPublicOrFavourites;

                if (message.userOrPublicOrFavourites == userOrPublicOrFavourites.User)
                {
                    CanPublicRecipes = true;
                    CanUserRecipes = false;
                    CanFavouriteRecipes = true;
                    await LoadRecipes(message.userOrPublicOrFavourites, pageSize, pageNumberUserRecipes);
                }
                else if(message.userOrPublicOrFavourites == userOrPublicOrFavourites.Public)
                {
                    CanPublicRecipes = false;
                    CanUserRecipes = true;
                    CanFavouriteRecipes = true;
                    await LoadRecipes(message.userOrPublicOrFavourites, pageSize, pageNumberPublicRecipes);
                }
                else if(message.userOrPublicOrFavourites == userOrPublicOrFavourites.Favourites)
                {
                    CanPublicRecipes = true;
                    CanUserRecipes = true;
                    CanFavouriteRecipes = false;
                    await LoadRecipes(message.userOrPublicOrFavourites, pageSize, pageNumberFavouritesRecipes);
                }           
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
        public bool CanPublicRecipes
        {
            get { return _isPublicRecipes; }
            set
            {
                _isPublicRecipes = value;
                NotifyOfPropertyChange(() => CanPublicRecipes);
            }
        }
        public bool CanUserRecipes
        {
            get { return _isUserRecipes; }
            set
            {
                _isUserRecipes = value;
                NotifyOfPropertyChange(() => CanUserRecipes);
            }
        }
        public bool CanFavouriteRecipes
        {
            get { return _isFavouriteRecipes; }
            set
            {
                _isFavouriteRecipes = value;
                NotifyOfPropertyChange(() => CanFavouriteRecipes);
            }
        }
        public bool CanNext
        {
            get { return _canNext; }
            set
            {
                _canNext = value;
                NotifyOfPropertyChange(() => CanNext);
            }
        }
        public bool CanPrevious
        {
            get { return _canPrevious; }
            set
            {
                _canPrevious = value;
                NotifyOfPropertyChange(() => CanPrevious);
            }
        }

        public bool NoRecipes
        {
            get { return _noRecipes; }
            set
            {
                _noRecipes = value;
                NotifyOfPropertyChange(() => NoRecipes);
            }
        }

        public bool NoFavouriteRecipes
        {
            get { return _noFavouriteRecipes; }
            set
            {
                _noFavouriteRecipes = value;
                NotifyOfPropertyChange(() => NoFavouriteRecipes);
            }
        }
        public string PageInfo
        {
            get { return _pageInfo; }
            set
            {
                _pageInfo = $"Strona {value} z {totalPages}";
                NotifyOfPropertyChange(() => PageInfo);
            }
        }
        #endregion

        private async Task LoadRecipes(userOrPublicOrFavourites userOrPublicOrFavourites ,int pageSize, int pageNumber)
        {
            try
            {
                tempRecipes.Clear();
                List<RecipeModel> recipes = new List<RecipeModel>();

                if(userOrPublicOrFavourites == userOrPublicOrFavourites.User)
                {
                    recipes = await _recipesEndPointAPI.GetRecipesLoggedUser(pageSize, pageNumber);
                }
                else if(userOrPublicOrFavourites == userOrPublicOrFavourites.Public)
                {
                    recipes = await _recipesEndPointAPI.GetPublicRecipes(pageSize, pageNumber);
                }
                else if (userOrPublicOrFavourites == userOrPublicOrFavourites.Favourites)
                {
                    recipes = await _recipesEndPointAPI.GetFavouritesRecipes(pageSize, pageNumber);
                }

                if(recipes.Count>0)
                {
                    totalPages = recipes.FirstOrDefault().TotalPages;
                }
                else
                {
                    totalPages = 1;
                }
               
                PageInfo = pageNumber.ToString();

                NavigationButtonsActiveDeactive(pageNumber);

                RecipeModelsToRecipeModelDisplay(recipes);

                await LoadImages();
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }

        private void RecipeModelsToRecipeModelDisplay(List<RecipeModel> recipeModels)
        {
            try
            {
                foreach (var item in recipeModels)
                {
                    RecipeModelDisplay recipeModelDisplaySingle = _mapper.Map<RecipeModelDisplay>(item);

                    bool displayAsPublic = false;
                    if (item.IsPublic && _loggedUser.UserName == item.UserName && (nowOpen == userOrPublicOrFavourites.Public || nowOpen == userOrPublicOrFavourites.User))
                    {
                        displayAsPublic = true;
                    }
                    recipeModelDisplaySingle.DisplayAsPublic = displayAsPublic;

                    bool displayAsFavourites = false;
                    if (_loggedUser.FavouriteRecipes.Contains(item.RecipeId.ToString()) && nowOpen == userOrPublicOrFavourites.Public)
                    {
                        displayAsFavourites = true;
                    }
                    recipeModelDisplaySingle.DisplayAsFavourites = displayAsFavourites;

                    tempRecipes.Add(recipeModelDisplaySingle);
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

                    if (downloadStatus)
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
            finally
            {
                if (Recipes.Count <= 0 && nowOpen == userOrPublicOrFavourites.User)
                {
                    NoRecipes = true;
                }
                else
                {
                    NoRecipes = false;
                }

                if (Recipes.Count <= 0 && nowOpen == userOrPublicOrFavourites.Favourites)
                {
                    NoFavouriteRecipes = true;
                }
                else
                {
                    NoFavouriteRecipes = false;
                }
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
                await _eventAggregator.PublishOnUIThreadAsync(new ReloadAllRecipes(userOrPublicOrFavourites.Public), new CancellationToken());
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
                await _eventAggregator.PublishOnUIThreadAsync(new ReloadAllRecipes(userOrPublicOrFavourites.User), new CancellationToken());
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }  
        
        public async Task FavouriteRecipes()
        {
            try
            {
                await _eventAggregator.PublishOnUIThreadAsync(new ReloadAllRecipes(userOrPublicOrFavourites.Favourites), new CancellationToken());
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
                RecipeModel recipeModel = _mapper.Map<RecipeModel>(model);
                await _eventAggregator.PublishOnUIThreadAsync(new RecipePreviewEvent(recipeModel, nowOpen), new CancellationToken());
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }

        }

        public async Task RecipesBack()
        {
            if (nowOpen == userOrPublicOrFavourites.User)
            {
                await LoadRecipes(userOrPublicOrFavourites.User, pageSize, --pageNumberUserRecipes);
            }
            else if(nowOpen == userOrPublicOrFavourites.Public)
            {
                await LoadRecipes(userOrPublicOrFavourites.Public, pageSize, --pageNumberPublicRecipes);
            }
            else if(nowOpen == userOrPublicOrFavourites.Favourites)
            {
                await LoadRecipes(userOrPublicOrFavourites.Favourites, pageSize, --pageNumberFavouritesRecipes);
            }
        }

        public async Task RecipesNext()
        {
            if (nowOpen == userOrPublicOrFavourites.User)
            {
                await LoadRecipes(userOrPublicOrFavourites.User,pageSize, ++pageNumberUserRecipes);
            }
            else if (nowOpen == userOrPublicOrFavourites.Public)
            {
                await LoadRecipes(userOrPublicOrFavourites.Public,pageSize, ++pageNumberPublicRecipes);
            }
            else if (nowOpen == userOrPublicOrFavourites.Favourites)
            {
                await LoadRecipes(userOrPublicOrFavourites.Favourites, pageSize, ++pageNumberFavouritesRecipes);
            }

        }

        private void NavigationButtonsActiveDeactive(int pageNumber)
        {
            if (pageNumber <= 1)
            {
                CanPrevious = false;
            }
            else
            {
                CanPrevious = true;
            }

            if (pageNumber >= totalPages)
            {
                CanNext = false;
            }
            else
            {
                CanNext = true;
            }
        }

        public void LogOffUser()
        {
            nowOpen = userOrPublicOrFavourites.User;
            CanPublicRecipes = true;
            CanUserRecipes = false;
            CanFavouriteRecipes = true;
            CanNext = false;
            CanPrevious = false;

            pageSize = 30;
            totalPages = 1;
            pageNumberUserRecipes = 1;
            pageNumberPublicRecipes = 1;

            tempRecipes.Clear();
            _recipes.Clear();
        }
    }
}
