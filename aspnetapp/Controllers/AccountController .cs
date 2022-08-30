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
        /// ��¼
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
                    return Ok(new Result() { code = "-1", message = "�û��������������" });
                }
            }
            catch (Exception)
            {

                throw;
            }
            return Ok(new Result() { code = "1", message = "success" });
        }
        /// <summary>
        /// �ǳ�
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
                    return Ok(new Result() { code = "-1", message = "���û�������ǳ�" });
                }
                await _signInManager.SignOutAsync();
                return Ok(new Result() { code = "1", message = $"{userName} �ǳ��ɹ�" });
            }
            catch (Exception ex)
            {
                return Ok(new Result() { code = "-1", message = "�ǳ�ʧ��---" + ex.Message });
            }
        }
        



        #region Ȩ�޺͵�¼��֤

        /// <summary>
        /// ���õ�¼����
        /// </summary>
        /// <param name="ui">�û�ID</param>
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
            ////�û�ID
            //var cookie = new HttpCookie("User-key", );
            //cookie.Expires = DateTime.Now.AddDays(1);
            ////����HttpOnly���ԣ�ͨ��js�ű����޷���ȡ��cookie��Ϣ������Ч�ķ�ֹXSS��������ȡcookie����
            //cookie.HttpOnly = true;
            //Response.Cookies.Add(cookie);

            ////��¼����
            //cookie = new HttpCookie("User-token", ui.Token);
            //cookie.Expires = DateTime.Now.AddDays(1);
            //cookie.HttpOnly = true;
            //Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// �����¼����
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
        /// �����¼����
        /// </summary>
        //private void ClearLoginSession()
        //{
        //    Session.Remove("active-user-info");
        //}

        /// <summary>
        /// IP��֤
        /// </summary>
        //private void IsVailidIP()
        //{
        //    //����������վIP��ַ
        //    string ipAllow = System.Configuration.ConfigurationManager.AppSettings["AllowSettingIP"];
        //    string ipClient = Request.UserHostAddress;
        //    if (string.IsNullOrEmpty(ipAllow))
        //    {
        //        ipAllow = Request.ServerVariables["LOCAL_ADDR"];
        //    }
        //    if (ipAllow.IndexOf(ipClient) < 0)
        //    {
        //        throw new Exception("����δ��Ȩ");
        //    }
        //}
        #endregion Ȩ�޺͵�¼��֤
    }
}
