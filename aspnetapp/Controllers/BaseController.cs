using Microsoft.AspNetCore.Mvc;

namespace aspnetapp.Controllers
{
    public class BaseController: ControllerBase
    {
        protected readonly ILogger _logger;

        public BaseController(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}
