using Cook_Book_Client_Desktop.Models;
using System.Threading.Tasks;

namespace Cook_Book_Client_Desktop.Helpers
{
    public interface IAPIHelper
    {
        Task<AuthenticatedUser> Authenticate(string username, string password);
    }
}