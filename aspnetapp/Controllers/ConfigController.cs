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
    /// ģ��id
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

        private static List<(string id, string name)> Templates = new List<(string id , string name)>()
        {
            new ("FGW8j2DYLoDO2ZKf3w_D6pOggXQYQSsQK_lJ7fEgj6g","��ѯ�ظ�֪ͨ"),
            new ("d-zw8JJKqXqRPFxUcD-s385QWDdpaPzDAsvk82g-jbQ","������"), 
        };

        /// <summary>
        /// ��ȡģ��id
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
                return Ok(new SimpleResult() { code = "1", message = "success", data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// ��ȡģ��
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
                return Ok(new SimpleResult() { code = "1", message = "success", data = new PageResult
                {
                    count = count,
                    list = templateConfigs
                }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// ��ȡģ��
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
                    return Ok(new SimpleResult() { code = "-1", message = "ģ����ϢID�ظ�" + template.tempId });
                }
                var model = new WeMessageTemplateConfig()
                {
                    TempData = template.tempData,
                    TempId = template.tempId,
                    TempName = template.tempName,
                    TempType = "΢��"
                };
                await _context.TemplateConfigs.AddAsync(model);
                await _context.SaveChangesAsync();
                return Ok(new SimpleResult() { code = "1", message = "success" });
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
            return Ok(new SimpleResult() { code = "1", message = "success", data = new
            {
                envId = WXCommon.WxEnv,
                envName = WXCommon.CloudEnvName
            } });
        }

        /// <summary>
        /// ��ȡ�˵�
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpGet("GetMenus")]
        [AllowAnonymous]
        public ActionResult GetMenus()
        {
            var kv = new Dictionary<string, string>
            {
                { "���߹���-��ȡ", "index" },
                { "���߹���-�༭", "edit" },
                { "���߹���-ɾ��", "delete" },
                { "���߹���-����", "add" },

                { "��Ƶ����-��ȡ", "index" },
                { "��Ƶ����-�༭", "edit" },
                { "��Ƶ����-ɾ��", "delete" },
                { "��Ƶ����-����", "add" },

                { "�û�����-��ȡ", "index" },
                { "�û�����-�༭", "edit" },
                { "�û�����-ɾ��", "delete" },
                { "�û�����-����", "add" },

                { "��ɫ����-��ȡ", "index" },
                { "��ɫ����-�༭", "edit" },
                { "��ɫ����-ɾ��", "delete" },
                { "��ɫ����-����", "add" }
            };

            return Ok(new SimpleResult() { code = "1", message = "success", data = kv });
        }


        /// <summary>
        /// ����
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
                    var tem = Templates.FirstOrDefault(o => o.id == item.id);
                    var model = new WeMessageTemplate()
                    {
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        IS_Send = false,
                        OpenId = openId,
                        PId = patient.Id,
                        TempId = item.id,
                        TempName = tem.name,
                    };
                    ws.Add(model);
                }
                await _context.WeMessageTemplates.AddRangeAsync(ws);
                await _context.SaveChangesAsync();
                return Ok(new SimpleResult() { code = "1", message = "success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
