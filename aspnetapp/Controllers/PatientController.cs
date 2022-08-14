using entityModel;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace aspnetapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly CounterContext _context;

        public PatientController(CounterContext context)
        {
            _context = context;
        }
        // GET: api/<PatientController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<PatientController>/5
        [HttpGet("{id}")]
        public ActionResult Get(int id)
        {
            try
            {
                var  patient =  _context.Patients.SingleOrDefault(b => b.Id  == id);

                return Ok(patient);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<PatientController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] string value)
        {
            try
            {
                var patient = new Patient ();
                await _context.Patients.AddAsync(patient);
                await _context.SaveChangesAsync();
                return Ok();
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
