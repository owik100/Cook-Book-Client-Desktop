namespace Cook_Book_Client_Desktop.EventsModels
{
    public class ReloadAllRecipes
    {
        public ReloadAllRecipes(userOrPublicOrFavourites userOrPublic)
        {
            userOrPublicOrFavourites = userOrPublic;
        }
        public userOrPublicOrFavourites userOrPublicOrFavourites { get; set; }
    }



    public enum userOrPublicOrFavourites
    {
        User,
        Public,
        Favourites
    }
}
