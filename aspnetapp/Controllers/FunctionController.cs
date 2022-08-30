using entityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace aspnetapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FunctionController : ControllerBase
    {
        private readonly UserManager<NoteUser> _userManager;

        private readonly RoleManager<NoteRole> _roleManager;

        public FunctionController(UserManager<NoteUser> userManager, RoleManager<NoteRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public ActionResult Get([FromServices] ISwaggerProvider swaggerProvider)
        {
            try
            {
               var result = swaggerProvider.GetSwagger("v1");

                return Ok(new Result()
                {
                    code = "1",
                    message = "success",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //    [HttpGet]
        //    public ActionResult Get(PageQuery pageQuery)
        //    {
        //        try
        //        {
        //            var count = _context.Functions.Count(o => o.ModuleName.Contains(pageQuery.search));
        //            var roles = _context.Functions.Where(o => o.ModuleName.Contains(pageQuery.search))
        //                .Skip(pageQuery.pageSize * pageQuery.pageNum)
        //                .Take(pageQuery.pageSize)
        //                .ToList();

        //            return Ok(new Result()
        //            {
        //                code = "1",
        //                message = "success",
        //                data = new PageResult
        //                {
        //                    pageCount = count,
        //                    list = roles
        //                }
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            return BadRequest(ex.Message);
        //        }
        //    }

        //    // GET api/<PatientController>/5
        //    [HttpGet("{id}")]
        //    public ActionResult Get(int id)
        //    {
        //        try
        //        {

        //            var role = _context.Roles.FirstOrDefault(o => o.RoleId == id);
        //            if (role == null)
        //            {
        //                return Ok(new Result() { code = "-1", message = "没有找到该角色" });
        //            }
        //            return Ok(new Result()
        //            {
        //                code = "1",
        //                message = "success",
        //                data = new
        //                {
        //                    roleName = role.RoleName,
        //                    roleId = role.RoleId,
        //                    roleMark = role.RoleMark
        //                }
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            return BadRequest(ex.Message);
        //        }
        //    }

        //    // POST api/<PatientController>
        //    [HttpPost]
        //    public async Task<ActionResult> Post([FromBody] RoleModel model)
        //    {
        //        try
        //        {
        //            var role = new Role()
        //            {
        //                RoleName = model.name,
        //                RoleMark = model.mark,
        //                Creator = 0,
        //            };
        //            await _context.Roles.AddAsync(role);
        //            await _context.SaveChangesAsync();
        //            return Ok();
        //        }
        //        catch (Exception ex)
        //        {
        //            return BadRequest(ex.Message);
        //        }
        //    }

        //    // PUT api/<PatientController>/5
        //    [HttpPut("{id}")]
        //    public ActionResult Put(int id, string name)
        //    {
        //        try
        //        {

        //            var function = _context.Functions.FirstOrDefault(o => o.FunctionId == id);
        //            if (function == null)
        //            {
        //                return Ok(new Result() { code = "-1", message = "没有找到该角色" });
        //            }
        //            function.ModuleName = name;
        //            _context.Update(function);
        //            _context.SaveChanges();
        //            return Ok(new Result() { code = "1", message = "success" });
        //        }
        //        catch (Exception ex)
        //        {
        //            return BadRequest(ex.Message);
        //        }
        //    }

        //    // DELETE api/<PatientController>/5
        //    [HttpDelete("{id}")]
        //    public ActionResult Delete(int id)
        //    {
        //        try
        //        {

        //            var role = _context.Functions.FirstOrDefault(o => o.FunctionId == id);
        //            if (role == null)
        //            {
        //                return Ok(new Result() { code = "-1", message = "没有找到该角色" });
        //            }
        //            _context.Remove(role);
        //            _context.SaveChanges();
        //            return Ok(new Result() { code = "1", message = "success" });
        //        }
        //        catch (Exception ex)
        //        {
        //            return BadRequest(ex.Message);
        //        }
        //    }
    }
}
