namespace Site24x7CustomReports.Services
{
    public interface IApiService
    {
        Task<string> GetMonitorsJsonAsync();
    }
}
