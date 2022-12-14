using aspnetapp.Common;
using EntityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.Formula.Functions;
using NuGet.Packaging;
using System.Data;
using System.IO;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
public class ClockModel
{
    /// <summary>
    /// 微信的openid
    /// </summary>
    public string OpenId { get; set; }
    /// <summary>
    /// 日常视频
    /// </summary>
    public string DailyVideoFileId { get; set; } = string.Empty;


    public string DailyVideoFileName { get; set; } = string.Empty;
}
namespace aspnetapp.Controllers
{
    /// <summary>
    /// 日常签到
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ClockInController : BaseController
    {
        private readonly BusinessContext _context;
        public ClockInController(ILogger<ClockInController> logger,BusinessContext context)
            :base(logger)
        {
            _context = context;
        }
        // GET: api/<PatientController>
        [HttpGet]
        public ActionResult Get(PageQuery pageQuery)
        {
            try
            {
                var count = _context.ClockIns.Count();
                var clocks = _context.ClockIns
                    .Skip(pageQuery.pageSize * pageQuery.pageIndex)
                    .Take(pageQuery.pageSize)
                    .ToList();
                return OkResult(new PageResult
                {
                    count = count,
                    list = clocks.Select(o => new
                    {

                    })
                }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{openid}")]
        public ActionResult Get(string openid)
        {
            try
            {
                var clocks =  _context.ClockIns.Where(b => b.OpenId  == openid);

                return OkResult(clocks.Select(o => new
                {
                    o.Id,
                    o.FeedbackComments,
                    date = o.CreatedAt.ToString("yyyy-MM-dd"), //时间
                    videos = o.Videos.Select(v => new
                    {
                        id = v.Id,
                        name = v.Name
                    })
                })
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// 签到
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // POST api/<PatientController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] ClockModel clockModel)
        {
            try
            {
                //应该先保存文件
                var patient = _context.Patients.FirstOrDefault(o => o.OpenId == clockModel.OpenId);
                if (patient == null)
                {
                    return Error("没有此患者" );
                }
                var clockIn = _context.ClockIns.FirstOrDefault(o => o.OpenId == clockModel.OpenId && o.CreatedAt.Date == DateTime.Now.Date);

                var video = new Video()
                {
                    Name = clockModel.DailyVideoFileName,
                    FileId = clockModel.DailyVideoFileId,
                    CreatedAt = DateTime.Now,
                    Describe = patient.Name + DateTime.Now.ToString("yyyyMMddHHmmss") + "每日打卡",
                    UpdatedAt = DateTime.Now,
                    Type = "打卡视频",
                    UploaderId = patient.Id
                };
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    
                    if (clockIn == null)
                    {
                        clockIn = new ClockIn()
                        {
                            CreatedAt = DateTime.Now,
                            TeachVideoId = string.Join(",",patient.PToVList.Select(pt=>pt.TVId)),
                            OpenId = clockModel.OpenId,
                            DoctorId = patient.DoctorId
                        };
                       var entityEntry = await _context.ClockIns.AddAsync(clockIn);
                       await _context.SaveChangesAsync();
                       video.ClockId = entityEntry.Entity.Id;
                       patient.LastCheckInId = entityEntry.Entity.Id;
                       await _context.Videos.AddAsync(video);
                    }
                    else
                    {
                        video.ClockId = clockIn.Id;
                        clockIn.CreatedAt = DateTime.Now;
                        clockIn.UpdatedAt = DateTime.Now;
                        patient.LastCheckInId = clockIn.Id;
                        var video1 = clockIn.Videos.FirstOrDefault(v => v.Name == video.Name && v.CreatedAt.Date == video.CreatedAt.Date);
                        if (video1 != null)
                        {
                            await WXCommon.DeleteUploadFile(new string[] { video1.FileId });
                            video1.UpdatedAt = DateTime.Now;
                            video1.FileId = clockModel.DailyVideoFileId;
                            _context.Videos.Update(video1);
                        }
                        else
                        {
                          await  _context.Videos.AddAsync(video);
                        }
                        _context.ClockIns.Update(clockIn);
                    }

                    patient.LastCheckInTime = DateTime.Now;
                    _context.Patients.Update(patient);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return OkResult();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex.Message);
                    return Error(ex.Message );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 医生反馈
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [HttpPut("feedback/{id}")]
        public async Task<ActionResult> FeedbackAsync(int id, [FromBody] string content,
            [FromServices] UserManager<NoteUser> userManager)
        {
            try
            {
                var model = _context.ClockIns.FirstOrDefault(o => o.Id == id);
                if (model == null)
                {
                    return Error("没有打卡记录" );
                }
                model.FeedbackComments = content;
                model.UpdatedAt = DateTime.Now;
                _context.ClockIns.Update(model);
                await _context.SaveChangesAsync();

                var messageTemplate = _context.WeMessageTemplates
                    .FirstOrDefault(o =>o.TempName == "咨询回复通知" && o.OpenId == model.OpenId && o.IS_Send ==false && o.CreatedAt.Date == model.CreatedAt.Date);
                if (messageTemplate != null)
                {
                    var  p =  _context.Patients.FirstOrDefault(o => o.OpenId == model.OpenId);
                    var docotr =  await userManager.FindByIdAsync(model.DoctorId);
                    var docotrName = (await userManager.GetClaimsAsync(docotr)).FirstOrDefault(c=>c.Type == ClaimTypes.Name)?.Value;
                    var data = new
                    {
                        thing6 = new
                        {
                            value = p?.Name
                        },
                        name1 = new
                        {
                            value = docotrName
                        },
                        time2 = new
                        {
                            value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        },
                        thing3 = new
                        {
                            value = content
                        },
                    };

                    var send = await WXCommon.SendMessage(messageTemplate, data,this.Request);
                    if (send)
                    {
                        messageTemplate.IS_Send = true;
                        _context.WeMessageTemplates.Update(messageTemplate);
                        _context.SaveChanges();
                    }
                }
                return OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 医生反馈
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [HttpPut("remindClockIn/{openId}")]
        public async Task<ActionResult> RemindClockIn(string openId,
            [FromServices] UserManager<NoteUser> userManager)
        {
            try
            {
                var model = await _context.WeMessageTemplates.OrderBy(o=>o.CreatedAt).LastOrDefaultAsync(o => o.OpenId == openId && o.TempName == "打卡提醒" && o.IS_Send == false);
                if (model == null)
                {
                    return Error("当前用户没有授权提醒，请打电话提醒！" );
                }
                var data = new
                {
                    thing4 = new
                    {
                        value = "提醒打卡"
                    },
                    time2 = new
                    {
                        value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    },
                    thing3 = new //备注
                    {
                        value = "今天需要打卡了！"
                    },
                };
                var send = await WXCommon.SendMessage(model, data, this.Request);
                if (send)
                {
                    model.IS_Send = true;
                    _context.WeMessageTemplates.Update(model);
                    _context.SaveChanges();
                    return OkResult();
                }
                else
                {
                    return Error("发送消息失败" );
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// 医生自定义反馈
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [HttpPut("remindCustom/{openId}")]
        public async Task<ActionResult> RemindCustom(string openId,[FromBody]string remark,
            [FromServices] UserManager<NoteUser> userManager)
        {
            try
            {

                var model = await _context.WeMessageTemplates.OrderBy(o => o.CreatedAt).LastOrDefaultAsync(o => o.OpenId == openId && o.TempName == "打卡提醒" && o.IS_Send == false);
                if (model == null)
                {
                    return Error("当前用户没有授权提醒，请打电话提醒！");
                }
                var data = new
                {
                    thing4 = new
                    {
                        value = "提醒打卡"
                    },
                    time2 = new
                    {
                        value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    },
                    thing3 = new //备注
                    {
                        value = remark
                    },
                };
                var send = await WXCommon.SendMessage(model, data, this.Request);
                if (send)
                {
                    model.IS_Send = true;
                    _context.WeMessageTemplates.Update(model);
                    _context.SaveChanges();
                    return OkResult();
                }
                else
                {
                    return Error("发送消息失败");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/<PatientController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageQuery"></param>
        /// <param name="userManager"></param>
        /// <returns></returns>
        [HttpPost("exportExcel")]
        public async Task<ActionResult> ExportExcel([FromBody] PageQuery pageQuery,
            [FromServices] UserManager<NoteUser> userManager)
        {
            try
            {
                var dataTable = new DataTable();
                var simpleItems = getSimpleItems();
                var dateItems = new List<SimpleItem>();
                foreach (var item in simpleItems)
                {
                    dataTable.Columns.Add(item.text);
                }
                for (DateTime time = pageQuery.date1; time <= pageQuery.date2; time = time.AddDays(1))
                {
                    dateItems.Add(new SimpleItem { text = time.ToString("yyyy-MM-dd"), value = time.ToString("yyyy-MM-dd"), tag = 12 });
                    dataTable.Columns.Add(time.ToString("yyyy-MM-dd"));
                };
                simpleItems.AddRange(dateItems);
                var patients = await _context.Patients.Where(o => o.Status != 1).ToListAsync();
                var clockIns = await _context.ClockIns.Where(o => o.CreatedAt >= pageQuery.date1 && o.CreatedAt <= pageQuery.date2).ToListAsync();
                
                foreach (var patient in patients)
                {
                    var data = new List<string>();  
                    data.Add(patient.Name);
                    data.Add(patient.Sex);
                    data.Add(GetAgeByBirthdate(patient.BirthDate));
                    data.Add(patient.Telephone);
                    data.Add(patient.CreatedAt.ToString("yyyy-MM-dd"));
                    data.Add(string.Join(",",patient.PToVList.Select(o=>o.TVideo.Name)));
                    var clocks = clockIns.Where(c => c.OpenId == patient.OpenId);
                    foreach (var item in dateItems)
                    {
                        if (clocks.Any(c=> c.CreatedAt.ToString("yyyy-MM-dd") == item.text))
                        {
                            data.Add("打卡");
                        }
                        else
                        {
                            data.Add("未打卡");
                        }
                    }
                    dataTable.Rows.Add(data.ToArray());
                }
                using (var stream = new MemoryStream())
                {
                    NPOIHelper.exportToExcel(dataTable, simpleItems, stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.xls"); ;
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        private string GetAgeByBirthdate(DateTime? date)
        {
            if (date == null)
            {
                return "-";
            }
            var birthdate = date.Value;
            DateTime now = DateTime.Now;
            int age = now.Year - birthdate.Year;
            if (now.Month < birthdate.Month || (now.Month == birthdate.Month && now.Day < birthdate.Day))
            {
                age--;
            }
            return (age < 0 ? 0 : age) +$"岁【{birthdate.ToString("yyyy-MM-dd")}】";
        }
        private IList<SimpleItem> getSimpleItems()
        {
            var list = new List<SimpleItem>();
            list.Add(new SimpleItem { text = "姓名", value = "姓名", tag = 15 });
            list.Add(new SimpleItem { text = "性别", value = "性别", tag = 12 });
            list.Add(new SimpleItem { text = "年龄", value = "年龄", tag = 18 });
            list.Add(new SimpleItem { text = "联系方式", value = "联系方式", tag = 15 });
            list.Add(new SimpleItem { text = "初诊时间", value = "初诊时间", tag = 15 });
            list.Add(new SimpleItem { text = "肌训内容", value = "肌训内容", tag = 15 });
            return list;
        }
    }

    

}
