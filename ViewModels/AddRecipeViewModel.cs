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
    public class AddRecipeViewModel : Screen
    {
        private string _recipeName;
        private string _recipeIntegradts;
        private string _recipeInstructions;

        private string _fileName;
        private string _image;

        private IRecipesEndPointAPI _recipesEndPointAPI;
        private IEventAggregator _eventAggregator;

        public AddRecipeViewModel(IRecipesEndPointAPI RecipesEndPointAPI, IEventAggregator EventAggregator)
        {
            _recipesEndPointAPI = RecipesEndPointAPI;
            _eventAggregator = EventAggregator;
        }


        public string ImageF
        {
            get { return _image; }
            set { 
                _image = value;
                NotifyOfPropertyChange(() => ImageF);
            }
        }


        public string FileName
        {
            get { return _fileName; }
            set { 
                _fileName = value;
                NotifyOfPropertyChange(() => FileName);
            }
        }


        public string RecipeName
        {
            get { return _recipeName; }
            set { 
                _recipeName = value;
                NotifyOfPropertyChange(() => RecipeName);
            }
        }

        public string RecipeIngredients
        {
            get { return _recipeIntegradts; }
            set { 
                _recipeIntegradts = value;
                NotifyOfPropertyChange(() => RecipeIngredients);
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

        public  void OpenFile()
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();
            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if (result == true)
            {
                FileName = openFileDlg.FileName;

               string strfilename = openFileDlg.InitialDirectory + openFileDlg.FileName;

                using (Image image = Image.FromFile(strfilename))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        ImageF = base64String;
                    }
                }

                

            }
        }

        public async Task AddRecipeSubmit()
        {
            try
            {
                //TODO Zrobic fabryke na to
                // Walidacja
                RecipeModel recipeModel = new RecipeModel
                {
                    Name = RecipeName,
                    Ingredients = RecipeIngredients.Split(',').ToList(),
                    Instruction = RecipeInstructions,
                    Image = ImageF
                };



                //var imageDataByteArray = Convert.FromBase64String(recipeModel.Image);
                //var imageDataStream = new MemoryStream(imageDataByteArray);
                //imageDataStream.Position = 0;

                //using (FileStream file = new FileStream(@"D:\Projects\AllInOne\Cook-Book\Cook-Book-API\wwwroot\images\kot.jpeg", FileMode.Create, System.IO.FileAccess.Write))
                //{
                //    byte[] bytes = new byte[imageDataStream.Length];
                //    imageDataStream.Read(bytes, 0, (int)imageDataStream.Length);
                //    file.Write(bytes, 0, bytes.Length);
                //    imageDataStream.Close();
                //}

                //using (FileStream file = new FileStream(@"D:\Projects\AllInOne\Cook-Book\Cook-Book-API\wwwroot\images\kot.jpeg", FileMode.Create, System.IO.FileAccess.Write))
                //    imageDataStream.CopyTo(file);


                //System.IO.File.WriteAllBytes(@"D:\Projects\AllInOne\Cook-Book\Cook-Book-API\wwwroot\images\kot.jpeg", imageDataByteArray);



                await _recipesEndPointAPI.InsertRecipe(recipeModel);
                await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent(), new CancellationToken());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }

        }

        public async Task Back()
        {
            await _eventAggregator.PublishOnUIThreadAsync(new  LogOnEvent(), new CancellationToken());
        }
    }
}
