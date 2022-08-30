#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnetapp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class UserModel
{
    public string username { get; set; }

    public string password { get; set; }

    public bool rememberMe { get; set; }
}

namespace aspnetapp.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<NoteUser> _userManager;

        private readonly SignInManager<NoteUser> _signInManager;
        public AccountController(UserManager<NoteUser> userManager, SignInManager<NoteUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("signIn")]
        [AllowAnonymous]
        public async Task<ActionResult> SignIn(UserModel model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.username, model.password, model.rememberMe, false);
                if (!result.Succeeded)
                {
                    return Ok(new Result() { code = "-1", message = "用户名或者密码错误" });
                }
            }
            catch (Exception)
            {

                throw;
            }
            return Ok(new Result() { code = "1", message = "success" });
        }
        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        
        [HttpGet("loginOut")]
        public async Task<ActionResult> LoginOut()
        {
            try
            {
                var userName = User.Identity.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    return Ok(new Result() { code = "-1", message = "无用户，无需登出" });
                }
                await _signInManager.SignOutAsync();
                return Ok(new Result() { code = "1", message = $"{userName} 登出成功" });
            }
            catch (Exception ex)
            {
                return Ok(new Result() { code = "-1", message = "登出失败---" + ex.Message });
            }
        }
        



        #region 权限和登录认证

        /// <summary>
        /// 设置登录缓存
        /// </summary>
        /// <param name="ui">用户ID</param>
        private void SetLoginCookie(string ui)
        {
            var keyCookieOptions = new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(1),
                HttpOnly = true
            };
            Response.Cookies.Append("User-key", "User-key-value", keyCookieOptions);
            var tokenCookieOptions = new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(1),
                HttpOnly = true
            };
            Response.Cookies.Append("User-token", "User-token-value", tokenCookieOptions);
            ////var user = UserAuthority.online(ui);
            ////用户ID
            //var cookie = new HttpCookie("User-key", );
            //cookie.Expires = DateTime.Now.AddDays(1);
            ////设置HttpOnly属性，通过js脚本将无法读取到cookie信息，能有效的防止XSS攻击，窃取cookie内容
            //cookie.HttpOnly = true;
            //Response.Cookies.Add(cookie);

            ////登录令牌
            //cookie = new HttpCookie("User-token", ui.Token);
            //cookie.Expires = DateTime.Now.AddDays(1);
            //cookie.HttpOnly = true;
            //Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// 清除登录缓存
        /// </summary>
        private void ClearLoginCookie()
        {
            var cookie = Request.Cookies["User-key"];
            if (cookie != null)
            {
                Response.Cookies.Delete("User-key");
            }
            cookie = Request.Cookies["User-token"];
            if (cookie != null)
            {
                Response.Cookies.Delete("User-token");
            }
        }

        /// <summary>
        /// 清除登录缓存
        /// </summary>
        //private void ClearLoginSession()
        //{
        //    Session.Remove("active-user-info");
        //}

        /// <summary>
        /// IP认证
        /// </summary>
        //private void IsVailidIP()
        //{
        //    //允许设置网站IP地址
        //    string ipAllow = System.Configuration.ConfigurationManager.AppSettings["AllowSettingIP"];
        //    string ipClient = Request.UserHostAddress;
        //    if (string.IsNullOrEmpty(ipAllow))
        //    {
        //        ipAllow = Request.ServerVariables["LOCAL_ADDR"];
        //    }
        //    if (ipAllow.IndexOf(ipClient) < 0)
        //    {
        //        throw new Exception("机器未授权");
        //    }
        //}
        #endregion 权限和登录认证
    }
}
