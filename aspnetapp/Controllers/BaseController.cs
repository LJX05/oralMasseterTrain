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

        protected OkObjectResult OkResult(object data)
        {
            return base.Ok(new SimpleResult() { code = 1, message = "成功", data = data });
        }
        protected OkObjectResult OkResult(string msg)
        {
            return base.Ok(new SimpleResult() { code = 1, message = msg });
        }
        protected OkObjectResult OkResult()
        {
            return base.Ok(new SimpleResult() { code = 1, message = "成功" });
        }
        protected OkObjectResult Error(string msg)
        {
            return base.Ok(new SimpleResult() { code = -1, message = msg });
        }
    }
}
