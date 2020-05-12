using Cook_Book_Shared_Code.Models;

namespace Cook_Book_Client_Desktop.EventsModels
{
    public class RecipePreviewEvent
    {
        public RecipePreviewEvent(RecipeModel recipeModel, userOrPublicOrFavourites backTo = userOrPublicOrFavourites.User)
        {
            RecipeModel = recipeModel;
            BackTo = backTo;
        }

        public RecipeModel RecipeModel { get; private set; }
        public userOrPublicOrFavourites BackTo { get; private set; }
    }
}
