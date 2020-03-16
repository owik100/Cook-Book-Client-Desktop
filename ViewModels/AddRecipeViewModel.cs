using System;
using System.Collections.Generic;
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
        private string _recipeName;
        private string _recipeIntegradts;
        private string _recipeInstructions;

        private string _fileName;
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
            if (message != null)
            {
                _addOrEdit = AddOrEdit.Edit;
                _recipeId = message.RecipeModel.RecipeId;
                SubmitText = "Zaktualizuj";

                RecipeName = message.RecipeModel.Name;
                RecipeIngredients = string.Join(";", message.RecipeModel.Ingredients);
                RecipeInstructions = message.RecipeModel.Instruction;
            }

            await Task.CompletedTask;
        }

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


        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                NotifyOfPropertyChange(() => FileName);
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

        public string RecipeIngredients
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

        public void OpenFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpeg";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                FileName = dlg.FileName;
                ImagePath = dlg.FileName;
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
                    Ingredients = RecipeIngredients.Split(';').ToList(),
                    Instruction = RecipeInstructions,
                    ImagePath = ImagePath
                };

                if (_addOrEdit == AddOrEdit.Add)
                {
                    

                    await _recipesEndPointAPI.InsertRecipe(recipeModel);
                    await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(), new CancellationToken());
                }
                else
                {
                    recipeModel.RecipeId = _recipeId;
                    var result = await _recipesEndPointAPI.EditRecipe(recipeModel);

                    if(result)
                    {
                        MessageBox.Show("Zaktualizowano pomyślnie!", "Zaktualizowano");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }

        }

        public async Task Back()
        {
            await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(), new CancellationToken());
        }


    }
}
