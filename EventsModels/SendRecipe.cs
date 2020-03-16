using Cook_Book_Client_Desktop_Library.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cook_Book_Client_Desktop.EventsModels
{
    public class SendRecipe
    {
        public SendRecipe(RecipeModel recipeModel)
        {
            RecipeModel = recipeModel;
        }

        public RecipeModel RecipeModel { get; private set; }
    }
}
