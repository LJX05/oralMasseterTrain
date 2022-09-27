using aspnetapp.Common;
using EntityModel;
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
                var ret = await WXCommon.GetUploadFileLink(fileName,this.Request);
                return Ok(new SimpleResult() { code = 1, data = ret });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


       
    }
}
