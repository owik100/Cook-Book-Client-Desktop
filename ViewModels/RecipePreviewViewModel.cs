using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop.Helpers;
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
        private bool _canEdit;
        private string _userName;
        private bool _displayUserName;

        private string _favouritesImage;
        private bool _canAddDeleteFavourites;
        private AddOrdDeleteFromFavourites _AddOrdDeleteFavourites;

        private IRecipesEndPointAPI _recipesEndPointAPI;
        private IEventAggregator _eventAggregator;
        private ILoggedUser _loggedUser;
        private IAPIHelper _aPIHelper;

        private bool _reloadNeeded = false;
        private userOrPublicOrFavourites backTo = userOrPublicOrFavourites.User;

        public RecipePreviewViewModel(IRecipesEndPointAPI RecipesEndPointAPI, ILoggedUser loggedUser, IEventAggregator EventAggregator, IAPIHelper aPIHelper)
        {
            _recipesEndPointAPI = RecipesEndPointAPI;
            _loggedUser = loggedUser;
            _aPIHelper = aPIHelper;
            _eventAggregator = EventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            CanEdit = false;
        }

        public async Task HandleAsync(SendRecipe message, CancellationToken cancellationToken)
        {
            try
            {
                backTo = message.BackTo;

                currentRecipe = message.RecipeModel;

                _recipeId = currentRecipe.RecipeId;
                RecipeName = currentRecipe.Name;
                RecipeIngredients = (currentRecipe.Ingredients).ToList();
                RecipeInstructions = currentRecipe.Instruction;
                ImagePath = currentRecipe.ImagePath;
                
                if(!currentRecipe.IsPublic || currentRecipe.UserName == _loggedUser.UserName)
                {
                    CanEdit = true;
                    DisplayUserName = false;
                    CanAddDeleteFavourites = false;
                }
                else
                {
                    CanEdit = false;
                    DisplayUserName = true;
                    UserName = "Autor przepisu: " + currentRecipe.UserName;
                    CanAddDeleteFavourites = true;

                    if(AlreadyFavourites())
                    {
                        _AddOrdDeleteFavourites = AddOrdDeleteFromFavourites.Delete;
                        FavouritesImage = ImageConstants.StarFull;
                    }
                    else
                    {
                        _AddOrdDeleteFavourites = AddOrdDeleteFromFavourites.Add;
                        FavouritesImage = ImageConstants.StarEmpty;
                    }
                }

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

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                NotifyOfPropertyChange(() => UserName);
            }
        } 
        public string FavouritesImage
        {
            get { return _favouritesImage; }
            set
            {
                _favouritesImage = value;
                NotifyOfPropertyChange(() => FavouritesImage);
            }
        }

        public bool CanEdit
        {
            get { return _canEdit; }
            set
            {
                _canEdit = value;
                NotifyOfPropertyChange(() => CanEdit);
            }
        } 
        public bool DisplayUserName
        {
            get { return _displayUserName; }
            set
            {
                _displayUserName = value;
                NotifyOfPropertyChange(() => DisplayUserName);
            }
        }
           public bool CanAddDeleteFavourites
        {
            get { return _canAddDeleteFavourites; }
            set
            {
                _canAddDeleteFavourites = value;
                NotifyOfPropertyChange(() => CanAddDeleteFavourites);
            }
        }


        #endregion


        public async Task Back()
        {
            try
            {
                await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(reloadNeeded: _reloadNeeded, backTo), new CancellationToken());
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

        public async Task AddOrDeleteFavourites()
        {
            try
            {
                if(_AddOrdDeleteFavourites == AddOrdDeleteFromFavourites.Add)
                {
                    _loggedUser.FavouriteRecipes.Add(_recipeId.ToString());
                }
                else
                {
                    _loggedUser.FavouriteRecipes.Remove(_recipeId.ToString());
                }

                LoggedUser loggedUser = new LoggedUser
                {
                    Id = _loggedUser.Id,
                    Email = _loggedUser.Email,
                    UserName = _loggedUser.UserName,
                    FavouriteRecipes = _loggedUser.FavouriteRecipes
                };

                 var result = await _aPIHelper.EditUser(loggedUser);

                _reloadNeeded = true;

                await Back();
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }

        private bool AlreadyFavourites()
        {
           bool output = false;

            try
            {
                if(_loggedUser.FavouriteRecipes.Contains(_recipeId.ToString()))
                {
                    output = true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }

            return output;
        }
    }

    public enum AddOrdDeleteFromFavourites
    {
        Add,
        Delete
    }
}
