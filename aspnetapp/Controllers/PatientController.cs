using entityModel;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

public class PatientModel 
{
    public string openId { get; set; } = string.Empty;

    public string name { get; set; } = string.Empty;

    public string sex { get; set; } = string.Empty;

    public int doctorId { get; set; }
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
        [HttpPost("{openId}")]
        public ActionResult Authentication(string openId)
        {
            try
            {
                var patient = _context.Patients.SingleOrDefault(b => b.OpenId == openId);
                if (patient == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有此微信用户,请先注册" });
                }
                return  Ok(new Result() { code = "1", message = "sucess" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<PatientController>/5
        [HttpGet("{id}")]
        public ActionResult Get(int id)
        {
            try
            {
                var patient = _context.Patients.SingleOrDefault(b => b.Id == id);
                if (patient == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有此微信用户,请先注册" });
                }
                return Ok(new Result() { code = "1", message = "success", data = patient });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<PatientController>
        [HttpPost]
        public async Task<ActionResult> Post(PatientModel model)
        {
            try
            {
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
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/<PatientController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {

        }

        // DELETE api/<PatientController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
