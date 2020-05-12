using Cook_Book_Shared_Code.Models;

namespace Cook_Book_Client_Desktop.EventsModels
{
    public class AddRecipeWindowEvent
    {
        public AddRecipeWindowEvent(AddOrEdit addOrEdit, RecipeModel recipeModel = null)
        {
            RecipeModel = recipeModel;
            AddOrEdit = addOrEdit;
        }

        public RecipeModel RecipeModel { get; private set; }
        public AddOrEdit AddOrEdit { get; private set; }
    }

    public enum AddOrEdit
    {
        Add,
        Edit
    }
}

