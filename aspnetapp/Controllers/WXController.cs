#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnetapp;



namespace aspnetapp.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class WXController : ControllerBase
    {
        private readonly BusinessContext _context;

        public WXController(BusinessContext context)
        {
            _context = context;
        }

        

    }
}
