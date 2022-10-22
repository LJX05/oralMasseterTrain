using EntityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Security.Claims;

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
    public class RoleController : BaseController
    {
        private readonly RoleManager<NoteRole> _roleManager;

        public RoleController(ILogger<RoleController> logger,RoleManager<NoteRole> context)
            :base(logger)
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
                var data = new PageResult()
                {
                    count = count,
                    list = roles.Select(o => new
                    {
                        id = o.Id,
                        roleName = o.Name,
                        roleNo = o.Code,
                        mark = o.Mark,
                        isPreset = o.IsPreset,
                        editTime = o._ut_,
                        editUser = o._uuid_
                    })
                };
                return OkResult(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
                    return Error("没有找到该角色" );
                }
                return OkResult( new
                    {
                        name = role.Name,
                        id = role.Id,
                        mark = role.Mark,
                        isPreset = role.IsPreset,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
                    return OkResult();
                }
                return Error(string.Join(",", result.Errors.Select(o => o.Description )));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<ActionResult> Put(string id, [FromBody] RoleEditViewModel model)
        {
            try
            {
                var role = await  _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return Error("没有找到该角色");
                }
                role.Name = model.roleName;
                role.Code = model.roleNo;
                role._ut_ = DateTime.Now;
                role._uuid_ = User.Identity.Name;
                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return OkResult();
                }
                return Error(string.Join(",", result.Errors.Select(o => o.Description)) );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("authorizeRole/{id}")]
        [Authorize]
        public async Task<ActionResult> AuthorizeRole(string id, [FromBody] string[] rids,
            [FromServices] IdentityContext identityContext)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return Error("没有找到该角色");
                }
                var claims = await _roleManager.GetClaimsAsync(role);
                var transaction = await identityContext.Database.BeginTransactionAsync();
                try
                {
                    foreach (var item in claims)
                    {
                        if (!rids.Any(o=> o == item.Type))
                        {
                            await _roleManager.RemoveClaimAsync(role,item);
                        }
                    }
                    foreach (var item in rids)
                    {

                        var oldclaim = claims.FirstOrDefault(o => o.Type == item);
                        if (oldclaim != null)
                        {
                            continue;
                        }
                        else
                        {
                            var val = ConfigController.GetFunctions().FirstOrDefault(o => o.Id + "" == item)?.Name;
                            var claim = new Claim(item, val);
                            await _roleManager.AddClaimAsync(role, claim);
                        }
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex.Message);
                    return BadRequest(ex.Message);
                }
                return OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getCurrentPermission")]
        [Authorize]
        public async Task<ActionResult> GetCurrentUserPermission([FromServices] UserManager<NoteUser> userManager)
        {
            try
            {
                var user =  await userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Error("没有找到用户");
                }
                var functions = ConfigController.GetFunctions();
                if (user.UserName == "admin")
                {
                    return OkResult(AuthSubTree(functions, 0));
                }
                var roles = await userManager.GetRolesAsync(user);
                var claims = new List<Claim>();
                foreach (var item in roles)
                {
                    var role = await _roleManager.FindByNameAsync(item);
                    claims.AddRange(await _roleManager.GetClaimsAsync(role));
                }
                
                var list = new List<FunctionItem>();
                foreach (var item in claims.Distinct())
                {
                    var function = functions.FirstOrDefault(o => o.Id + "" == item.Type);
                    if (function != null)
                    {
                        list.Add(function);
                    }
                }
                return OkResult(AuthSubTree(list,0));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetRolePermission/{id}")]
        [Authorize]
        public async Task<ActionResult> GetRolePermission(string id,[FromServices] UserManager<NoteUser> userManager)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return Error("没有找到角色");
                }
                var functions = ConfigController.GetFunctions();
                
                var claims = await _roleManager.GetClaimsAsync(role);
                var list = new List<FunctionItem>();
                foreach (var item in claims.Distinct())
                {
                    var function = functions.FirstOrDefault(o => o.Id + "" == item.Type);
                    if (function != null)
                    {
                        list.Add(function);
                    }
                }
                return OkResult(list.Select(o=>o.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
                    return Error("没有找到该角色" );
                }
                else if (role.IsPreset)
                {
                    return Error("预置角色无法删除" );
                }
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return OkResult();
                }
                return Error(string.Join(",", result.Errors.Select(o => o.Description)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// 递归加载
        /// </summary>
        private dynamic AuthSubTree(IList<FunctionItem> items, int pid)
        {
            var tree = items.Where(o => o.Pid == pid).ToList();
            //递归结束条件
            if (tree.Count == 0)
            {
                return null;
            }
            var node = new ExpandoObject() as IDictionary<string, object>;
            foreach (var item in tree)
            {
                var children = AuthSubTree(items, item.Id);
                if (children == null)
                {
                    node.Add(item.Name, true);
                }
                else
                {
                    node.Add(item.Name, children);
                }
            }
            return node;
        }
    }
}
