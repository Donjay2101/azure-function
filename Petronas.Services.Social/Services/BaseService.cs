using System.Linq;
using System.Threading.Tasks;
using Petronas.Services.Social.Repositories.Interfaces;

namespace Petronas.Services.Social.Services
{
    public class BaseService
    {
        private readonly IApplicationRepository _applicationRepository;

        public BaseService(IApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        public async Task<bool> IsEnvironmentNotAllowed(string applicationId, string environment)
        {
            var application = await _applicationRepository.Get(applicationId, applicationId);

            if (application == null)
            {
                return false;
            }

            // Check if environment is exist
            var env = application.AllowedEnvironments.FirstOrDefault(x => !x.IsDeleted && x.Name.ToUpper() == environment.ToUpper());
            if (env == null)
                return true;
            else
                return false;
        }
    }
}
