﻿using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop.Helpers;
using Cook_Book_Shared_Code.API;
using Cook_Book_Shared_Code.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Cook_Book_Client_Desktop.ViewModels
{
    public class AddRecipeViewModel : Screen, IHandle<SendRecipe>
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _recipeName;
        private BindingList<string> _recipeIntegradts = new BindingList<string>();
        private string _selectedIngredient;
        private string _ingredientInsert;
        private string _recipeInstructions;
        private string _image;
        private bool _isPublic;

        private IRecipesEndPointAPI _recipesEndPointAPI;
        private IEventAggregator _eventAggregator;

        private AddOrEdit _addOrEdit = AddOrEdit.Add;
        private string _submitText;
        private int _recipeId;

        private bool reloadNeeded = false;

        public AddRecipeViewModel(IRecipesEndPointAPI RecipesEndPointAPI, IEventAggregator EventAggregator)
        {
            _recipesEndPointAPI = RecipesEndPointAPI;
            _eventAggregator = EventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            SubmitText = "Dodaj";
            ImagePath = ImageConstants.DefaultImage;
        }

        public async Task HandleAsync(SendRecipe message, CancellationToken cancellationToken)
        {
            try
            {
                if (message != null)
                {
                    _addOrEdit = AddOrEdit.Edit;
                    _recipeId = message.RecipeModel.RecipeId;
                    SubmitText = "Zaktualizuj";

                    RecipeName = message.RecipeModel.Name;
                    RecipeIngredients = new BindingList<string>(message.RecipeModel.Ingredients.ToList());
                    RecipeInstructions = message.RecipeModel.Instruction;
                    ImagePath = message.RecipeModel.ImagePath;
                    IsPublic = message.RecipeModel.IsPublic;
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
            }
        }

        #region Props
        public string SubmitText
        {
            get { return _submitText; }
            set
            {
                _submitText = value;
                NotifyOfPropertyChange(() => SubmitText);
            }
        }

        public string ImagePath
        {
            get { return _image; }
            set
            {
                _image = value;
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
                NotifyOfPropertyChange(() => CanAddRecipeSubmit);
            }
        }

        public BindingList<string> RecipeIngredients
        {
            get { return _recipeIntegradts; }
            set
            {
                _recipeIntegradts = value;
                NotifyOfPropertyChange(() => RecipeIngredients);
                NotifyOfPropertyChange(() => CanAddRecipeSubmit);
            }
        }

        public string SelectedIngredient
        {
            get { return _selectedIngredient; }
            set
            {
                _selectedIngredient = value;
                NotifyOfPropertyChange(() => SelectedIngredient);
                NotifyOfPropertyChange(() => RecipeIngredients);
                NotifyOfPropertyChange(() => CanDeleteIngredient);
            }
        }

        public string RecipeInstructions
        {
            get { return _recipeInstructions; }
            set
            {
                _recipeInstructions = value;
                NotifyOfPropertyChange(() => RecipeInstructions);
                NotifyOfPropertyChange(() => CanAddRecipeSubmit);
            }
        }

        public string IngredientInsert
        {
            get { return _ingredientInsert; }
            set
            {
                _ingredientInsert = value;
                NotifyOfPropertyChange(() => IngredientInsert);
                NotifyOfPropertyChange(() => CanAddIngredientTextBox);
            }
        }

        public bool IsPublic
        {
            get { return _isPublic; }
            set
            {
                _isPublic = value;
                NotifyOfPropertyChange(() => IsPublic);
            }
        }

        public bool CanAddIngredientTextBox
        {
            get
            {
                bool output = false;

                if (!string.IsNullOrWhiteSpace(IngredientInsert))
                {
                    output = true;
                }

                return output;
            }

        }

        public bool CanDeleteIngredient
        {
            get
            {
                bool output = false;

                if (!string.IsNullOrWhiteSpace(SelectedIngredient))
                {
                    output = true;
                }

                return output;
            }

        }

        public bool CanDeleteFileModel
        {
            get
            {
                bool output = false;

                if (!string.IsNullOrWhiteSpace(ImagePath) && ImagePath != ImageConstants.DefaultImage)
                {
                    output = true;
                }

                return output;
            }
        }

        public bool CanAddRecipeSubmit
        {
            get
            {
                bool output = false;

                if (!string.IsNullOrWhiteSpace(RecipeName) && !string.IsNullOrWhiteSpace(RecipeInstructions) && RecipeIngredients.Count > 0)
                {
                    output = true;
                }

                return output;
            }
        }

        #endregion

        public void OpenFile()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.DefaultExt = ".jpeg";
                dlg.Filter = "Image Files| *.jpg; *.jpeg; *.png; *.gif;";

                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    ImagePath = dlg.FileName;
                    NotifyOfPropertyChange(() => ImagePath);
                    NotifyOfPropertyChange(() => CanDeleteFileModel);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }

        }

        public void DeleteFileModel()
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show($"Na pewno chcesz usunąć obrazek?", "Potwierdź usunięcie", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    ImagePath = ImageConstants.DefaultImage;
                    NotifyOfPropertyChange(() => ImagePath);
                    NotifyOfPropertyChange(() => CanDeleteFileModel);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }

        public async Task AddRecipeSubmit()
        {
            try
            {
                RecipeModel recipeModel = new RecipeModel
                {
                    Name = RecipeName,
                    Ingredients = RecipeIngredients.ToList(),
                    Instruction = RecipeInstructions,
                    NameOfImage = ImagePath,
                    IsPublic = IsPublic,
                };

                if (recipeModel.NameOfImage == ImageConstants.DefaultImage)
                {
                    recipeModel.NameOfImage = "";
                }

                if (_addOrEdit == AddOrEdit.Add)
                {
                    await _recipesEndPointAPI.InsertRecipe(recipeModel);
                    reloadNeeded = true;

                    await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(reloadNeeded), new CancellationToken());
                }
                else if (_addOrEdit == AddOrEdit.Edit)
                {
                    recipeModel.RecipeId = _recipeId;
                    var result = await _recipesEndPointAPI.EditRecipe(recipeModel);

                    if (result)
                    {
                        if (recipeModel.NameOfImage == "")
                        {
                            recipeModel.NameOfImage = ImageConstants.DefaultImage;
                        }

                        NotifyOfPropertyChange(() => ImagePath);
                        NotifyOfPropertyChange(() => CanDeleteFileModel);

                        reloadNeeded = true;
                        MessageBox.Show("Zaktualizowano pomyślnie!", "Zaktualizowano", MessageBoxButton.OK, MessageBoxImage.Information);

                        await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(reloadNeeded), new CancellationToken());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }

        public void AddIngredientTextBox()
        {
            try
            {
                RecipeIngredients.Add(IngredientInsert);
                IngredientInsert = "";
                NotifyOfPropertyChange(() => CanAddRecipeSubmit);
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }

        }

        public void DeleteIngredient()
        {
            try
            {
                RecipeIngredients.Remove(SelectedIngredient);
                NotifyOfPropertyChange(() => CanAddRecipeSubmit);
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }

        public async Task Back()
        {
            try
            {
                await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(reloadNeeded), new CancellationToken());
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }
    }
}
