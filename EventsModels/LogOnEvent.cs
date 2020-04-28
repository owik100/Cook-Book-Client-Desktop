namespace Cook_Book_Client_Desktop.EventsModels
{
    public class LogOnEvent
    {
        public LogOnEvent(bool reloadNeeded, userOrPublicOrFavourites userOrPublicOrFavourites = userOrPublicOrFavourites.User)
        {
            ReloadNeeded = reloadNeeded;
            UserOrPublicOrFavourites = userOrPublicOrFavourites;
        }

        public bool ReloadNeeded { get; private set; }
        public userOrPublicOrFavourites UserOrPublicOrFavourites { get; private set; }
    }
}
