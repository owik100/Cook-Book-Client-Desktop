namespace Cook_Book_Client_Desktop.EventsModels
{
    public class LogOnEvent
    {
        public LogOnEvent(bool reloadNeeded)
        {
            ReloadNeeded = reloadNeeded;
        }

        public bool ReloadNeeded { get; private set; }
    }
}
