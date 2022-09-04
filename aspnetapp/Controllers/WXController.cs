#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnetapp;
using aspnetapp.Common;

namespace aspnetapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WXController : ControllerBase
    {
        private readonly BusinessContext _context;

        public WXController(BusinessContext context)
        {
            _context = context;
        }

        // GET api/<PatientController>/5
        [HttpPost("GetOpenId/{code}")]
        public async Task<ActionResult> GetOpenId(string code)
        {
            try
            {
                var openid = await WXCommon.GetOpenId(code);
                return Ok(new Result() { code = "1", message = "sucess" ,data = openid });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
