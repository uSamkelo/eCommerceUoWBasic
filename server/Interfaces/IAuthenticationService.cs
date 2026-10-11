using server.Models;

namespace server.Interfaces
{
    public interface IAuthenticationService
    {
        string GenerateToken(User user);
    }
}