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
using EntityModel;
using aspnetapp.Common;

public class SubscribeInfo
{
    public string id { get; set; } = string.Empty;

    public string result { get; set; } = string.Empty;
}
public class TemplateConfig
{
    /// <summary>
    /// 模板id
    /// </summary>
    public string tempId { get; set; } = string.Empty;
    public string tempType { get; set; } = string.Empty;
    public string tempName { get; set; } = string.Empty;
    public string tempData { get; set; } = string.Empty;
}
namespace aspnetapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : BaseController
    {
        private readonly BusinessContext _context;

        public ConfigController(ILogger<ConfigController> logger, BusinessContext context):base(logger)
        {
            _context = context;
        }


        /// <summary>
        /// 获取模板id
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpGet("GetTemplateIds")]
        [AllowAnonymous]
        public async Task<ActionResult> GetTemplateIds()
        {
            try
            {
                var Templates = await _context.TemplateConfigs.ToListAsync();
                string[] list = Templates.Select(o => o.TempId).ToArray();
                return OkResult(list );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// 获取模板
        /// </summary>
        /// <param PageQuery="pageQuery"></param>
        /// <returns></returns>
        [HttpGet("getTemplates")]
        [Authorize]
        public async Task<ActionResult> GetTemplates([FromQuery] PageQuery pageQuery)
        {
            try
            {
                var count = await _context.TemplateConfigs.CountAsync(o => o.TempName.Contains(pageQuery.search));
                var templateConfigs = await _context.TemplateConfigs.Where(o => o.TempName.Contains(pageQuery.search))
                    .Skip(pageQuery.pageSize * (pageQuery.pageIndex - 1))
                    .Take(pageQuery.pageSize)
                    .ToListAsync();
                return OkResult(new PageResult
                {
                    count = count,
                    list = templateConfigs
                }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// 获取模板
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("addTemplate")]
        [Authorize]
        public async Task<ActionResult> AddTemplate([FromBody] TemplateConfig template)
        {
            try
            {
                var templateConfig = await _context.TemplateConfigs.FirstOrDefaultAsync(o => o.TempId == template.tempId);
                if (templateConfig != null)
                {
                    return Error("模板消息ID重复" + template.tempId );
                }
                var model = new WeMessageTemplateConfig()
                {
                    TempData = template.tempData,
                    TempId = template.tempId,
                    TempName = template.tempName,
                    TempType = "微信"
                };
                await _context.TemplateConfigs.AddAsync(model);
                await _context.SaveChangesAsync();
                return OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetENVID")]
        public ActionResult GetENVID()
        {
            return OkResult(new
            {
                envId = WXCommon.WxEnv,
                envName = WXCommon.CloudEnvName
            });
        }

        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpGet("GetMenus")]
        [AllowAnonymous]
        public ActionResult GetMenus()
        {
            var kv = new Dictionary<string, string>
            {
                { "患者管理-读取", "index" },
                { "患者管理-编辑", "edit" },
                { "患者管理-删除", "delete" },
                { "患者管理-增加", "add" },

                { "视频管理-读取", "index" },
                { "视频管理-编辑", "edit" },
                { "视频管理-删除", "delete" },
                { "视频管理-增加", "add" },

                { "用户管理-读取", "index" },
                { "用户管理-编辑", "edit" },
                { "用户管理-删除", "delete" },
                { "用户管理-增加", "add" },

                { "角色管理-读取", "index" },
                { "角色管理-编辑", "edit" },
                { "角色管理-删除", "delete" },
                { "角色管理-增加", "add" }
            };

            return OkResult(kv);
        }


        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("subscribe/{openId}")]
        [AllowAnonymous]
        public async Task<ActionResult> Subscribe(string openId,[FromBody]IList<SubscribeInfo> subscribeInfos)
        {
            try
            {
                var patient = _context.Patients.FirstOrDefault(o => o.OpenId == openId);
                var ws = new List<WeMessageTemplate>();
                foreach (var item in subscribeInfos)
                {
                    if (item.result != "accept")
                    {
                        continue;
                    }
                    var tem = _context.TemplateConfigs.FirstOrDefault(o => o.TempId == item.id);
                    var model = new WeMessageTemplate()
                    {
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        IS_Send = false,
                        OpenId = openId,
                        PId = patient.Id,
                        TempId = tem.TempId,
                        TempName = tem.TempName,
                    };
                    ws.Add(model);
                }
                await _context.WeMessageTemplates.AddRangeAsync(ws);
                await _context.SaveChangesAsync();
                return OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
