using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop.Helpers;
using Cook_Book_Client_Desktop_Library.Helpers;
using Cook_Book_Shared_Code.API;
using Cook_Book_Shared_Code.Models;

namespace Cook_Book_Client_Desktop.ViewModels
{
   public class PublicRecipesViewModel : Screen, IHandle<ReloadPublicRecipes>
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IRecipesEndPointAPI _recipesEndPointAPI;
        private BindingList<RecipeModel> _recipes;
        List<RecipeModel> tempRecipes = new List<RecipeModel>();

        private IEventAggregator _eventAggregator;
        public PublicRecipesViewModel(IRecipesEndPointAPI RecipesEndPointAPI, IEventAggregator EventAggregator)
        {
            _recipesEndPointAPI = RecipesEndPointAPI;
            _eventAggregator = EventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        public async Task HandleAsync(ReloadPublicRecipes message, CancellationToken cancellationToken)
        {
            await LoadRecipes();
            await LoadImages();
        }

        #region Props
        public BindingList<RecipeModel> Recipes
        {
            get { return _recipes; }
            set
            {
                _recipes = value;
                NotifyOfPropertyChange(() => Recipes);
            }
        }
        #endregion

        private async Task LoadRecipes()
        {
            try
            {
                tempRecipes = await _recipesEndPointAPI.GetPublicRecipes();
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

                Recipes = new BindingList<RecipeModel>(tempRecipes);
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
                await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(reloadNeeded: false), new CancellationToken());
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
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
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }

        }
    }
}
