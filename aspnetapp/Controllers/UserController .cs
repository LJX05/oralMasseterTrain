using entityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

public class RegisterViewModel
{
    public string userName { get; set; } = string.Empty;

    public string password { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;

    public string userMobile { get; set; } = string.Empty;
    /// <summary>
    /// 真是姓名
    /// </summary>
    public string realName { get; set; } = string.Empty;

    public string userSex { get; set; } = string.Empty;
    /// <summary>
    /// 要加入的角色名称
    /// </summary>
    public IList<string> roles { get; set; } 

}
public class UserEditViewModel
{
    public string userEmail { get; set; } = string.Empty;

    public string userMobile { get; set; } = string.Empty;
    /// <summary>
    /// 真是姓名
    /// </summary>
    public string realName { get; set; } = string.Empty;

    public string userSex { get; set; } = string.Empty;

    /// <summary>
    /// 要加入的角色名名称
    /// </summary>
    public IList<string> roles { get; set; } 
}

namespace aspnetapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly RoleManager<NoteRole> _roleManager;
        private readonly UserManager<NoteUser> _userManager;
        public UserController(RoleManager<NoteRole> context, UserManager<NoteUser> userManager)
        {
            _roleManager = context;
            _userManager = userManager;
        }
        [HttpPost("getUserList")]
        [Authorize]
        public ActionResult GetUserList(PageQuery pageQuery)
        {
            try
            {
                var count = _userManager.Users.Count(o => o.UserName.Contains(pageQuery.search));
                var roles = _userManager.Users.Where(o => o.UserName.Contains(pageQuery.search))
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
                            userName = o.UserName,
                            realName = "***",
                            userMobile = o.PhoneNumber,
                            userSex = "***",
                            userEmail = o.Email,
                            isLock = o.LockoutEnd == null,
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
        public async Task<ActionResult> Get(string id)
        {
            try
            {
                var user = _userManager.Users.FirstOrDefault(o => o.Id == id);
                if (user == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有找到用户" });
                }
                var claims = await _userManager.GetClaimsAsync(user);
                var realname = claims.FirstOrDefault(o => o.Type == ClaimTypes.Name);
                var sex = claims.FirstOrDefault(o => o.Type == ClaimTypes.Gender);
                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new Result()
                {
                    code = "1",
                    message = "success",
                    data = new
                    {
                        id = user.Id,
                        userName = user.UserName,
                        realName = realname?.Value,
                        userMobile = user.PhoneNumber,
                        userSex = sex?.Value,
                        userEmail = user.Email,
                        isLock = user.LockoutEnd == null,
                        editTime = user._ut_,
                        roles = roles,
                        editUser = user._uuid_
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<PatientController>/5
        [HttpGet("getUserByRoleName/{roleName}")]
        public async Task<ActionResult> GetUserByRoleName(string roleName)
        {
            try
            {
                var users = await _userManager.GetUsersInRoleAsync(roleName);
                if (users.Count==0)
                {
                    return Ok(new Result() { code = "-1", message = "没有找到用户" });
                }
                var list = new List<object>();
                foreach (var item in users)
                {
                    var rename =(await _userManager.GetClaimsAsync(item)).FirstOrDefault(o => o.Type == ClaimTypes.Name);
                    list.Add(new
                    {
                        id = item.Id,
                        userName = item.UserName,
                        name = rename?.Value,
                    });
                }
                return Ok(new Result()
                {
                    code = "1",
                    message = "success",
                    data = list
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <returns></returns>
        [HttpPost("register")]
        [Authorize]
        public async Task<ActionResult> Register(RegisterViewModel model, [FromServices] IdentityContext context)
        {
            try
            {
                var noteUser = new NoteUser()
                {
                    UserName = model.userName,
                    Email = model.email,
                    PhoneNumber = model.userMobile,
                };
                var password = "Abc@123";
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Gender, model.userSex));
                claims.Add(new Claim(ClaimTypes.Name, model.realName));
                using var transaction = await context.Database.BeginTransactionAsync();
                var result = await _userManager.CreateAsync(noteUser, password);
                try
                {
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRolesAsync(noteUser, model.roles);
                        result = await _userManager.AddClaimsAsync(noteUser, claims);
                        transaction.Commit();
                        return Ok(new Result() { code = "1", message = "success" });
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return Ok(new Result() { code = "-1", message = e.Message });
                }
                var msg = "";
                foreach (var error in result.Errors)
                {
                    msg += error.Description + ",";
                }
                return Ok(new Result() { code = "-1", message = msg });
            }
            catch (Exception ex)
            {
                return Ok(new Result() { code = "-1", message = "注册失败---" + ex.Message });
            }
        }

        // PUT api/<PatientController>/5
        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<ActionResult> Put(string id, [FromBody] UserEditViewModel model, 
            [FromServices] IdentityContext context)
        {
            try
            {
                var user = await  _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有找到该用户" });
                }
                var oldroles = await _userManager.GetRolesAsync(user);

                var oldSex = await context.UserClaims.FirstOrDefaultAsync(o => o.UserId == user.Id
                && o.ClaimType == ClaimTypes.Gender);

                var oldRealName = await context.UserClaims.FirstOrDefaultAsync(o => o.UserId == user.Id
                && o.ClaimType == ClaimTypes.Name);
                using var transaction = await context.Database.BeginTransactionAsync();
                user.PhoneNumber = model.userMobile;
                user.Email = model.userEmail;
                user._ut_ = DateTime.Now;
                user._uuid_ = User.Identity.Name;
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Gender, model.userSex));
                claims.Add(new Claim(ClaimTypes.Name, model.realName));
                try
                {
                    var result = await _userManager.RemoveClaimAsync(user, oldSex?.ToClaim());
                    result =await _userManager.RemoveClaimAsync(user, oldRealName?.ToClaim());
                    await _userManager.AddClaimsAsync(user, claims);
                    result = await _userManager.RemoveFromRolesAsync(user,oldroles);
                    result = await _userManager.AddToRolesAsync(user, model.roles);

                    if (result.Succeeded)
                    {
                        await transaction.CommitAsync();
                        return Ok(new Result() { code = "1", message = "更新success" });
                    }
                    return Ok(new Result() { code = "-1", message = string.Join(",", result.Errors.Select(o => o.Description)) });
                }
                catch (Exception)
                {

                    transaction.Rollback();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("lock/{id}")]
        [Authorize]
        public async Task<ActionResult> LockUser(string id,[FromQuery]bool isLock)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有找到该用户" });
                }
                DateTimeOffset? dateTimeOffset = isLock ? DateTimeOffset.MaxValue : null;
                var result = await _userManager.SetLockoutEndDateAsync(user, dateTimeOffset);
                if (result.Succeeded)
                {
                    return Ok(new Result() { code = "1", message = "success" });
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
        
        [HttpPut("pwd/{id}")]
        [Authorize]
        public async Task<ActionResult> ResetPassWord(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有找到该用户" });
                }
                var password = "Abc@123";
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, code, password);
                if (result.Succeeded)
                {
                    return Ok(new Result() { code = "1", message = "success" });
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
        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有找到该用户" });
                }
                
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return Ok(new Result() { code = "1", message = "删除success" });
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
