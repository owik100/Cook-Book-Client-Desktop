using Cook_Book_Shared_Code.Models;

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
