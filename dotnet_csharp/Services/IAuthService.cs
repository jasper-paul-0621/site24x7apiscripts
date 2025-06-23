using Site24x7Integration;

namespace Site24x7CustomReports.Services
{
    public interface IAuthService
    {
        bool Run();
        AuthZoho ZohoAuth { get; }
        Task<string> GetAccessTokenAsync();
    }
}
