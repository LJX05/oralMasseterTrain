#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnetapp;

public class UserModel
{
    public string account { get; set; }

    public string password { get; set; }
}

namespace aspnetapp.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly CounterContext _context;

        public AccountController(CounterContext context)
        {
            _context = context;
        }
 
        [HttpPost]
        public ActionResult SignIn(UserModel data)
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
            return Ok(new Result() { code = "1",message="success"}) ;
        }

        [HttpGet]
        public ActionResult SignOut()
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
            return Ok(new Result() { code = "1", message = "success" });
        }


        private void SetToken()
        {
             
        }
        private void ClearToken()
        {

        }
    }
}
