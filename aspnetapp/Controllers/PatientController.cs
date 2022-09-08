using aspnetapp.Common;
using entityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

public class PatientModel
{
    public string openId { get; set; } = string.Empty;

    public string name { get; set; } = string.Empty;

    public string sex { get; set; } = string.Empty;

    public string phoneNumber { get; set; } = string.Empty;

    public string doctorId { get; set; } = string.Empty;
}

namespace aspnetapp.Controllers
{
    /// <summary>
    /// 患者表
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly BusinessContext _context;

        public PatientController(BusinessContext context)
        {
            _context = context;
        }
        // GET api/<PatientController>/5
        [HttpPost("Authentication/{code}")]
        public async Task<ActionResult> Authentication(string code)
        {
            try
            {
                var openid = await WXCommon.GetOpenId(code);

                var patient = _context.Patients.SingleOrDefault(b => b.OpenId == openid);
                if (patient == null)
                {
                    return Ok(new Result() { code = "-10", message = "没有此微信用户,请先注册", data = openid });
                }
                return Ok(new Result() { code = "1", message = "sucess", data = openid });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        [Authorize]
        public ActionResult Get([FromQuery] PageQuery pageQuery)
        {
            try
            {
                var count = _context.Patients.Count(o => o.Name.Contains(pageQuery.search));
                var patients = _context.Patients.Where(o => o.Name.Contains(pageQuery.search))
                    .Skip(pageQuery.pageSize * (pageQuery.pageIndex - 1))
                    .Take(pageQuery.pageSize)
                    .ToList();
                var list = patients.Select(o =>
                {
                    var islast = o.LastCheckInTime.GetValueOrDefault().Date == DateTime.Now.Date;
                    int? cid = null;
                    if (islast) //如果是今天签到的话返回 签到的id 
                    {
                        cid = _context.ClockIns.FirstOrDefault(c => c.OpenId == o.OpenId && DateTime.Now.Date == c.CreatedAt.Date)?.Id;
                    }
                    var teachName = _context.Videos.FirstOrDefault(v => v.Id == o.TeachVideoId.GetValueOrDefault())?.Name;
                    return new
                    {
                        o.Id,
                        o.Name,
                        o.Sex,
                        o.CreatedAt,
                        o.OpenId,
                        o.UpdatedAt,
                        SameDayIsCheck = islast ? '是' : '否',
                        LastCheckInVideoId = islast ? o.LastCheckInVideoId : null,
                        teachName = teachName ?? "未设置",
                        todayClockInId = cid,//当天签到
                    };
                });
                return Ok(new Result()
                {
                    code = "1",
                    message = "success",
                    data = new PageResult
                    {
                        count = count,
                        list = list
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // GET api/<PatientController>/5
        [HttpGet("getDetail/{openId}")]
        public async Task<ActionResult> GetDetail(string openId, [FromServices] UserManager<NoteUser> userManger)
        {
            try
            {
                var patient = _context.Patients.SingleOrDefault(b => b.OpenId == openId);
                if (patient == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有此微信用户,请先注册" });
                }
                var doctor = (await userManger.GetUsersInRoleAsync("医生")).FirstOrDefault(o => o.Id == patient.DoctorId);
                var doctorName = "--";
                if (doctor != null)
                {
                    doctorName = (await userManger.GetClaimsAsync(doctor)).FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                }


                return Ok(new Result()
                {
                    code = "1",
                    message = "success",
                    data = new
                    {
                        patient.Id,
                        patient.Name,
                        patient.Sex,
                        doctorName,
                        patient.TeachVideoId
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getSetting/{id}")]
        public ActionResult GetSetting(int id)
        {
            try
            {
                var patient = _context.Patients.SingleOrDefault(b => b.Id == id);
                if (patient == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有此微信用户,请先注册" });
                }
                var activities = _context.PatientActivitys.Where(o => o.PId == patient.Id).ToList();
                return Ok(new Result()
                {
                    code = "1",
                    message = "success",
                    data = new
                    {
                        setTeachVideoId = patient.TeachVideoId,
                        activities = activities.Select(a => new
                        {
                            timestamp = a.CreatedAt,
                            content = a.Content
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<PatientController>
        [HttpPost("register")]
        public async Task<ActionResult> Post(PatientModel model)
        {
            try
            {
                var patient1 = _context.Patients.FirstOrDefault(o => o.OpenId == model.openId);
                if (patient1 != null)
                {
                    return Ok(new Result() { code = "-1", message = "该患者已登记无需登记" });
                }
                var patient = new Patient()
                {
                    DoctorId = model.doctorId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Name = model.name,
                    Sex = model.sex,
                    OpenId = model.openId
                };
                await _context.Patients.AddAsync(patient);
                await _context.SaveChangesAsync();
                return Ok(new Result() { code = "1", message = "success" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 设置教学视频
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] int tid)
        {
            try
            {
                var patient = _context.Patients.FirstOrDefault(o => o.Id == id);
                if (patient == null)
                {
                    return Ok(new Result() { code = "-1", message = "该患者未登记" });
                }
                var tvideo = _context.Videos.FirstOrDefault(o => o.Id == tid);
                if (tvideo == null)
                {
                    return Ok(new Result() { code = "-1", message = "未找到该视频" });
                }
                //插入一天记录
                var activity = new PatientActivity()
                {
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    OpenId = patient.OpenId,
                    PId = patient.Id,
                    VideoId = tvideo.Id,
                    Content = $"设置教学视频{tvideo.Name}",
                };
                using var transaction = await _context.Database.BeginTransactionAsync();
                patient.TeachVideoId = tid;
                patient.UpdatedAt = DateTime.Now;
                try
                {
                    _context.Patients.Update(patient);
                    await _context.PatientActivitys.AddAsync(activity);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new Result() { code = "1", message = "success" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Ok(new Result() { code = "-1", message = ex.Message });
                }

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
