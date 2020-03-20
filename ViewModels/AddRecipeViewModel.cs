using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Cook_Book_Client_Desktop.EventsModels;
using Cook_Book_Client_Desktop_Library.API;
using Cook_Book_Client_Desktop_Library.API.Interfaces;
using Cook_Book_Client_Desktop_Library.Models;

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

        private IRecipesEndPointAPI _recipesEndPointAPI;
        private IEventAggregator _eventAggregator;

        private AddOrEdit _addOrEdit = AddOrEdit.Add;
        private string _submitText;
        private int _recipeId;

        public AddRecipeViewModel(IRecipesEndPointAPI RecipesEndPointAPI, IEventAggregator EventAggregator)
        {
            _recipesEndPointAPI = RecipesEndPointAPI;
            _eventAggregator = EventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            SubmitText = "Dodaj";
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
            }
        }

        public BindingList<string> RecipeIngredients
        {
            get { return _recipeIntegradts; }
            set
            {
                _recipeIntegradts = value;
                NotifyOfPropertyChange(() => RecipeIngredients);
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

                if (!string.IsNullOrWhiteSpace(ImagePath) && ImagePath != "pack://application:,,,/Resources/food template.png")
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
                dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

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
                MessageBoxResult messageBoxResult = MessageBox.Show($"Na pewno chcesz usunąć obrazek?", "Potwierdź usunięcie", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    ImagePath = "pack://application:,,,/Resources/food template.png";
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
                //TODO Zrobic fabryke na to
                // Walidacja
                //Delegat?
                RecipeModel recipeModel = new RecipeModel
                {
                    Name = RecipeName,
                    Ingredients = RecipeIngredients.ToList(),
                    Instruction = RecipeInstructions,
                    NameOfImage = ImagePath
                };

                if (recipeModel.NameOfImage == "pack://application:,,,/Resources/food template.png")
                {
                    recipeModel.NameOfImage = "";
                }

                if (_addOrEdit == AddOrEdit.Add)
                {
                    await _recipesEndPointAPI.InsertRecipe(recipeModel);
                    await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(), new CancellationToken());
                }
                else if (_addOrEdit == AddOrEdit.Edit)
                {
                    recipeModel.RecipeId = _recipeId;
                    var result = await _recipesEndPointAPI.EditRecipe(recipeModel);

                    if (result)
                    {
                        if(recipeModel.NameOfImage == "")
                        {
                            recipeModel.NameOfImage = "pack://application:,,,/Resources/food template.png";
                        }

                        NotifyOfPropertyChange(() => ImagePath);
                        NotifyOfPropertyChange(() => CanDeleteFileModel);

                        MessageBox.Show("Zaktualizowano pomyślnie!", "Zaktualizowano");
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
                await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(), new CancellationToken());
            }
            catch (Exception ex)
            {
                _logger.Error("Got exception", ex);
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }      
        }
    }
}
