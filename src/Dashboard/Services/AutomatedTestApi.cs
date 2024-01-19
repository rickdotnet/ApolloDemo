using Dashboard.Endpoints;
using Refit;

namespace Dashboard.Services;

public interface AutomatedTestApi
{
    [Get("/automatedtestresults")]
    Task<IEnumerable<AutomatedTestResultEvent>> GetAllTestResults();
}