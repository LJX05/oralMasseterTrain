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
using entityModel;
using aspnetapp.Common;

public class SubscribeInfo
{
    public string id { get; set; } = string.Empty;

    public string result { get; set; } = string.Empty;
}

namespace aspnetapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly BusinessContext _context;

        public ConfigController(BusinessContext context)
        {
            _context = context;
        }

        private static List<(string id, string name)> Templates = new List<(string id , string name)>()
        {
            new ("FGW8j2DYLoDO2ZKf3w_D6pOggXQYQSsQK_lJ7fEgj6g","��ѯ�ظ�֪ͨ"),
            new ("d-zw8JJKqXqRPFxUcD-s385QWDdpaPzDAsvk82g-jbQ","������"), 
        };

        /// <summary>
        /// ��ȡģ��
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpGet("GetTemplateIds")]
        [AllowAnonymous]
        public  ActionResult GetTemplateIds()
        {
            string[] list = Templates.Select(o=>o.id).ToArray();
            return Ok(new Result() { code = "1", message = "success",data = list });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetENVID")]
        public ActionResult GetENVID()
        {
            string envid = WXCommon.WxEnv;
            return Ok(new Result() { code = "1", message = "success", data = envid });
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

            return Ok(new Result() { code = "1", message = "success", data = kv });
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
                return Ok(new Result() { code = "1", message = "success" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
