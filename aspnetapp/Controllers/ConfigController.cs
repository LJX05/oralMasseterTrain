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
using System.Dynamic;

public class FunctionItem
{
    public int Id { get; set; }

    /// <summary>
    /// ��Id
    /// </summary>
    public int Pid { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }

    public IList<FunctionItem> Childrens { get; }

    public FunctionItem(int id, string name,string code ,int pid = 0)
    {
        Id = id;
        Name = name;
        Code = code;
        Pid = pid;
        Childrens = new List<FunctionItem>();
    }
}
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
                return OkResult(list );
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
                    return Error("ģ����ϢID�ظ�" + template.tempId );
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

        public static IList<FunctionItem> GetFunctions()
        {
            var list = new List<FunctionItem>();
            var item1 = new FunctionItem(1, "���߹���", "patientManger");
            var item2 = new FunctionItem(11, "��ȡ","patient_read",1);
            var item3 = new FunctionItem(12, "�༭","patient_edit",1);
            var item4 = new FunctionItem(13, "ɾ��","patient_dele",1);
            var item5 = new FunctionItem(14, "����","patient_add", 1);
            list.Add(item1);
            list.Add(item2);
            list.Add(item3);
            list.Add(item4);
            list.Add(item5);
            var item21 = new FunctionItem(2, "��Ƶ����", "videoManger");
            var item22 = new FunctionItem(21, "��ȡ","video_read", 2);
            var item23 = new FunctionItem(22, "�༭","video_edit", 2);
            var item24 = new FunctionItem(23, "ɾ��","video_dele", 2);
            var item25 = new FunctionItem(24, "����","video_add", 2);
            list.Add(item21);
            list.Add(item22);
            list.Add(item23);
            list.Add(item24);
            list.Add(item25);

            var item31 = new FunctionItem(3, "�û�����", "userManger");
            var item32 = new FunctionItem(31, "��ȡ","user_read", 3);
            var item33 = new FunctionItem(32, "�༭","user_edit", 3);
            var item34 = new FunctionItem(33, "ɾ��","user_dele", 3);
            var item35 = new FunctionItem(34, "����","user_add", 3);
            list.Add(item31);
            list.Add(item32);
            list.Add(item33);
            list.Add(item34);
            list.Add(item35);
            var item41 = new FunctionItem(4, "��ɫ����","roleManger");
            var item42 = new FunctionItem(41, "��ȡ","role_read", 4);
            var item43 = new FunctionItem(42, "�༭","role_edit", 4);
            var item44 = new FunctionItem(43, "ɾ��","role_dele", 4);
            var item45 = new FunctionItem(44, "����","role_add", 4);
            list.Add(item41);
            list.Add(item42);
            list.Add(item43);
            list.Add(item44);
            list.Add(item45);
            return list;
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
            return OkResult(GenerateTree(GetFunctions()));
        }

        /// <summary>
        /// �ݹ����
        /// </summary>
        private IList<FunctionItem> GenerateTree(IList<FunctionItem> items)
        {
            foreach (var item in items)
            {
                if (item.Pid == 0)
                {
                    continue;
                }
                var pitem = items.FirstOrDefault(o => o.Id == item.Pid);
                pitem.Childrens.Add(item);
            }
            return items.Where(o => o.Childrens.Count != 0).ToList();
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
