using Cook_Book_Client_Desktop_Library.Models;

namespace Cook_Book_Client_Desktop.EventsModels
{
    public class RecipePreviewEvent
    {
        public RecipePreviewEvent(RecipeModel recipeModel)
        {
            RecipeModel = recipeModel;
        }

        public RecipeModel RecipeModel { get; private set; }
    }
}
