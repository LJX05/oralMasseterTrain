using entityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

public class RoleViewModel
{
    public string roleName { get; set; } = String.Empty;

    public string roleNo { get; set; } = String.Empty;

    public string mark { get; set; } = String.Empty;
}

public class RoleEditViewModel
{
    public string roleName { get; set; } = String.Empty;

    public string roleNo { get; set; } = String.Empty;
}

namespace aspnetapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<NoteRole> _roleManager;

        public RoleController(RoleManager<NoteRole> context)
        {
            _roleManager = context;
        }
        [HttpPost("getRoleList")]
        [Authorize]
        public ActionResult GetRoleList(PageQuery pageQuery)
        {
            try
            {
                var count = _roleManager.Roles.Count(o => o.Name.Contains(pageQuery.search));
                var roles = _roleManager.Roles.Where(o => o.Name.Contains(pageQuery.search))
                    .Skip(pageQuery.pageSize * (pageQuery.pageIndex - 1))
                    .Take(pageQuery.pageSize)
                    .ToList();

                return Ok(new Result()
                {
                    code = "1",
                    message = "success",
                    data = new PageResult
                    {
                        count = count,
                        list = roles.Select(o => new
                        {
                            id = o.Id,
                            roleName = o.Name,
                            roleNo =o.Code, 
                            mark = o.Mark,
                            isPreset = o.IsPreset,
                            editTime = o._ut_,
                            editUser = o._uuid_
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<PatientController>/5
        [HttpGet("{id}")]
        [Authorize]
        public ActionResult Get(string id)
        {
            try
            {

                var role = _roleManager.Roles.FirstOrDefault(o => o.Id == id);
                if (role == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有找到该角色" });
                }
                return Ok(new Result()
                {
                    code = "1",
                    message = "success",
                    data = new
                    {
                        name = role.Name,
                        id = role.Id,
                        mark = role.Mark,
                        isPreset = role.IsPreset,
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<PatientController>
        [HttpPost("add")]
        [Authorize]
        public async Task<ActionResult> Post([FromBody] RoleViewModel model)
        {
            try
            {
                var role = new NoteRole()
                {
                    Name = model.roleName,
                    Mark = model.mark,
                    IsPreset = false,
                    Code =model.roleNo,
                    _ut_ = DateTime.Now,
                    _ct_ = DateTime.Now,
                    _uuid_ = User.Identity.Name,
                    _cuid_ = User.Identity.Name
                };
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return Ok(new Result() { code = "1", message = "success" });
                }
                return Ok(new Result() { code = "-1", message = string.Join(",", result.Errors.Select(o => o.Description )) });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/<PatientController>/5
        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<ActionResult> Put(string id, [FromBody] RoleEditViewModel model)
        {
            try
            {
                var role = await  _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有找到该角色" });
                }
                role.Name = model.roleName;
                role.Code = model.roleNo;
                role._ut_ = DateTime.Now;
                role._uuid_ = User.Identity.Name;
                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return Ok(new Result() { code = "1", message = "更新success" });
                }
                return Ok(new Result() { code = "-1", message = string.Join(",", result.Errors.Select(o => o.Description)) });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/<PatientController>/5
        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有找到该角色" });
                }
                else if (role.IsPreset)
                {
                    return Ok(new Result() { code = "-1", message = "预置角色无法删除" });
                }
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return Ok(new Result() { code = "1", message = "更新success" });
                }
                return Ok(new Result()
                {
                    code = "-1",
                    message = string.Join(",", result.Errors.Select(o => o.Description))
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
