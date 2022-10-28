using aspnetapp.Common;
using EntityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

public class PatientModel
{
    public string openId { get; set; } = string.Empty;

    public string name { get; set; } = string.Empty;

    public string sex { get; set; } = string.Empty;

    public DateTime? birthDate{ get; set; }
    
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
    public class PatientController : BaseController
    {
        private readonly BusinessContext _context;

        public PatientController(ILogger<PatientController>logger,BusinessContext context)
            :base(logger)
        {
            _context = context;
        }
        // GET api/<PatientController>/5
        [HttpPost("Authentication/{code}")]
        public async Task<ActionResult> Authentication(string code)
        {
            try
            {
                var openid = "";
                if (WXCommon.IsCloudEnv)
                {
                     openid = this.Request.Headers["x-wx-openid"];
                    if (string.IsNullOrEmpty(openid))
                    {
                        throw new Exception("云托管环境获取openid失败");
                    }
                }
                else
                {
                    openid = await WXCommon.GetOpenId(code);
                }
                var patient = _context.Patients.SingleOrDefault(b => b.OpenId == openid);
                if (patient == null)
                {
                    return Ok(new SimpleResult() { code = -10, message = "没有此微信用户,请先注册", data = openid });
                }
                return Ok(new SimpleResult() { code = 1, message = "成功", data = openid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Get([FromBody] PageQuery pageQuery, 
            [FromServices] UserManager<NoteUser> userManager)
        {
            try
            {
                IQueryable<Patient> queryable;
                queryable = _context.Patients.Where(o => o.Name.Contains(pageQuery.search));
                if (!(User.Identity?.Name == "admin" || User.IsInRole("主任医生")))
                {
                    var uid = userManager.GetUserId(User);
                    queryable = queryable.Where(o => o.DoctorId == uid);
                }
                var count = queryable.Count();
                pageQuery.pageSize = 50;
                var patients = await queryable.Skip(pageQuery.pageSize * (pageQuery.pageIndex - 1))
                    .Take(pageQuery.pageSize)
                    .ToListAsync();
                var list = patients.Select(o =>
                {
                    var islast = o.LastCheckInTime.GetValueOrDefault().Date == DateTime.Now.Date;
                    int? cid = null;
                    if (islast) //如果是今天签到的话返回 签到的id 
                    {
                        cid = _context.ClockIns.FirstOrDefault(c => c.OpenId == o.OpenId && DateTime.Now.Date == c.CreatedAt.Date)?.Id;
                    }
                    var teachName = "未设置";
                    if(o.PToVList != null)
                    {
                     teachName = o.PToVList.Count > 0 ? string.Join(",", o.PToVList.Select(pt => pt.TVideo.Name)) : "未设置";
                    }
                    
                    return new
                    {
                        o.Id,
                        o.Name,
                        o.Sex,
                        o.CreatedAt,
                        o.OpenId,
                        o.UpdatedAt,
                        SameDayIsCheck = islast ? '是' : '否',
                        LastCheckInVideos = islast ? o.LastCheckIn?.Videos
                        .Select(v => new
                        {
                            id = v.Id,
                            name = v.Name,
                        }) : null,
                        teachName = teachName,
                        todayClockInId = cid,//当天签到
                    };
                });
                return OkResult(new PageResult{
                        count = count,
                        list = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
                    return Error("没有此微信用户,请先注册");
                }
                var doctor = (await userManger.GetUsersInRoleAsync("医生")).FirstOrDefault(o => o.Id == patient.DoctorId);
                var doctorName = "--";
                if (doctor != null)
                {
                    doctorName = (await userManger.GetClaimsAsync(doctor)).FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                }
                return OkResult(new
                {
                    patient.Id,
                    patient.Name,
                    patient.Sex,
                    patient.Telephone,
                    doctorName,
                    TeachVideos = patient.PToVList.Select(o => new
                    {
                        id = o.TVId,
                        name = o.TVideo.Name
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

        [HttpGet("getSetting/{id}")]
        public ActionResult GetSetting(int id)
        {
            try
            {
                var patient = _context.Patients.SingleOrDefault(b => b.Id == id);
                if (patient == null)
                {
                    return Error("没有此微信用户,请先注册" );
                }
                var activities = _context.PatientActivitys.Where(o => o.PId == patient.Id).ToList();
                return OkResult(new
                {
                    setTeachVideos = patient.PToVList.Select(o => new
                    {
                        name = o.TVideo.Name,
                        id = o.TVId,
                    }),
                    activities = activities.Select(a => new
                    {
                        timestamp = a.CreatedAt,
                        content = a.Content
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // POST api/<PatientController>
        [HttpPost("register")]
        public async Task<ActionResult> Post(PatientModel model, [FromServices] UserManager<NoteUser> userManger)
        {
            try
            {
                var patient1 = _context.Patients.FirstOrDefault(o => o.OpenId == model.openId);
                if (patient1 != null)
                {
                    return Error("该患者已登记无需登记" );
                }
                var user = userManger.FindByIdAsync(model.doctorId);
                if (user == null)
                {
                    return Error("未找到选择的医生" );
                }
                var patient = new Patient()
                {
                    DoctorId = model.doctorId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    BirthDate = model.birthDate,
                    Telephone = model.phoneNumber,
                    LastCheckInId =-1,
                    Name = model.name,
                    Sex = model.sex,
                    OpenId = model.openId
                };
                await _context.Patients.AddAsync(patient);
                await _context.SaveChangesAsync();
                return OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // POST api/<PatientController>
        [HttpPost("edit")]
        public async Task<ActionResult> Edit(PatientModel model)
        {
            try
            {
                var patient = _context.Patients.FirstOrDefault(o => o.OpenId == model.openId);
                if (patient == null)
                {
                    return Error("未找到该患者");
                }
                patient.Telephone = model.phoneNumber;
                _context.Patients.Update(patient);
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
        /// 设置教学视频
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] int[] tid)
        {
            try
            {
                var patient = _context.Patients.FirstOrDefault(o => o.Id == id);
                if (patient == null)
                {
                    return Error("该患者未登记" );
                }
                var tvideos = _context.Videos.Where(o => tid.Contains(o.Id));
                if (tvideos.Count() == 0)
                {
                    return Error("未找到该视频" );
                }
                //插入一天记录
                var activity = new PatientActivity()
                {
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    OpenId = patient.OpenId,
                    PId = patient.Id,
                    VideoIds = string.Join(",", tvideos.Select(o => o.Id)),
                    Content = $"设置教学视频{string.Join(",", tvideos.Select(o => o.Name))}"
                };
                using var transaction = await _context.Database.BeginTransactionAsync();

                patient.UpdatedAt = DateTime.Now;
                try
                {
                    foreach (var item in patient.PToVList)
                    {
                        _context.PatientToTeachVideos.Remove(item);
                    }
                    foreach (var item in tvideos)
                    {
                        var toTeachVideo = new PatientToTeachVideo()
                        {
                            TVId = item.Id,
                            PId = patient.Id,
                            OpenId = patient.OpenId
                        };
                        _context.PatientToTeachVideos.Add(toTeachVideo);
                    }
                    _context.Patients.Update(patient);
                    await _context.PatientActivitys.AddAsync(activity);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return OkResult();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Error(ex.Message );
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
    }
}
