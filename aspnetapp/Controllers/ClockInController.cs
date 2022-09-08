using aspnetapp.Common;
using entityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
public class ClockModel
{
    /// <summary>
    /// 微信的openid
    /// </summary>
    public string OpenId { get; set; }
    /// <summary>
    /// 教学视频
    /// </summary>
    public int TeachVideoId { get; set; }
    /// <summary>
    /// 日常视频
    /// </summary>
    public string DailyVideoFileId { get; set; } = string.Empty;
}
namespace aspnetapp.Controllers
{
    /// <summary>
    /// 日常签到
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ClockInController : ControllerBase
    {
        private readonly BusinessContext _context;

        public ClockInController(BusinessContext context)
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
                return Ok(new Result()
                {
                    code = "1",
                    message = "success",
                    data = new PageResult
                    {
                        count = count,
                        list = clocks.Select(o => new
                        {

                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{openid}")]
        public ActionResult Get(string openid)
        {
            try
            {
                var clocks =  _context.ClockIns.Where(b => b.OpenId  == openid);

                return Ok(new Result()
                {
                    code = "1",
                    message = "success",
                    data = clocks.Select(o => new
                    {
                        o.Id,
                        videoId = o.DailyVideoId,//视频
                        date = o.CreatedAt.ToString("yyyy-MM-dd"), //时间
                        date1 = o.CreatedAt.ToString("yyyy/MM/dd"), //时间
                    })
                });
            }
            catch (Exception ex)
            {
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
                    return Ok(new Result() { code = "-1", message = "没有此患者" });
                }
                var clockIn = _context.ClockIns.FirstOrDefault(o => o.OpenId == clockModel.OpenId && o.CreatedAt.Date == DateTime.Now.Date);
                var video = new Video()
                {
                    Name = patient.Name + DateTime.Now.ToString("yyyyMMddHHmmss") + "每日打卡",
                    FileId = clockModel.DailyVideoFileId,
                    CreatedAt = DateTime.Now,
                    Describe = "每日打卡视频" + patient.Name,
                    UpdatedAt = DateTime.Now,
                    Type = "打卡视频",
                    UploaderId = patient.Id
                };

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var entity = await _context.Videos.AddAsync(video);
                    await _context.SaveChangesAsync();
                    if (clockIn == null)
                    {
                        clockIn = new ClockIn()
                        {
                            CreatedAt = DateTime.Now,
                            DailyVideoId = entity.Entity.Id,
                            TeachVideoId = patient.TeachVideoId.GetValueOrDefault(),
                            OpenId = clockModel.OpenId,
                            DoctorId = patient.DoctorId
                        };
                        await _context.ClockIns.AddAsync(clockIn);
                    }
                    else
                    {
                        clockIn.CreatedAt = DateTime.Now;
                        clockIn.UpdatedAt = DateTime.Now;
                        clockIn.DailyVideoId = entity.Entity.Id;
                        _context.ClockIns.Update(clockIn);
                    }

                    patient.LastCheckInVideoId = entity.Entity.Id;
                    patient.LastCheckInTime = DateTime.Now;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new Result() { code = "1", message = "success" });

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return Ok(new Result() { code = "-1", message = e.Message });
                }

                return Ok(new Result() { code = "1", message = "success" });
            }
            catch (Exception ex)
            {
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
                    return Ok(new Result() { code = "-1", message = "没有打卡记录" });
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

                    var send = await WXCommon.SendMessage(messageTemplate, data);
                    if (send)
                    {
                        messageTemplate.IS_Send = true;
                        _context.WeMessageTemplates.Update(messageTemplate);
                        _context.SaveChanges();
                    }
                }
                return Ok(new Result() { code = "1", message = "success" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/<PatientController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
