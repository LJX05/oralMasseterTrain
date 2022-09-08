using aspnetapp.Common;
using entityModel;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace aspnetapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {

        public FileController()
        {
            
        }

        [HttpPost]
        public async Task<ActionResult> Post(string fileName)
        {
            try
            {
                var ret = await WXCommon.GetUploadFileLink(fileName);
                return Ok(new Result() { code = "1", data = ret });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


       
    }
}
