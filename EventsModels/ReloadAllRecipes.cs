using System;
using System.Collections.Generic;
using System.Text;

namespace Cook_Book_Client_Desktop.EventsModels
{
    public class ReloadAllRecipes
    {
        public ReloadAllRecipes(UserOrPublic userOrPublic)
        {
            UserOrPublic = userOrPublic;
        }
        public UserOrPublic UserOrPublic { get; set; }
    }

   

    public enum UserOrPublic
    {
        User,
        Public
    }
}
