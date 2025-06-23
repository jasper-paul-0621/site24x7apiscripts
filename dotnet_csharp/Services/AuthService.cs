using Site24x7Integration;
using System.Threading.Tasks;

namespace Site24x7CustomReports.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthZoho _authZoho;

        public AuthService()
        {
            _authZoho = new AuthZoho();
        }

        public bool Run()
        {
            return _authZoho.Authorize();
        }

        public AuthZoho ZohoAuth => _authZoho;

        public async Task<string> GetAccessTokenAsync()
        {
            return await _authZoho.GetAccessTokenAsync() ?? string.Empty;
        }
    }
}
