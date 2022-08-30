using aspnetapp.Common;
using entityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
public class VideoModel
{
    public string name { get; set; } = string.Empty;

    public string type { get; set; } = string.Empty;

    public string fileId { get; set; } = string.Empty;

    public string describe { get; set; } = string.Empty;
}


namespace aspnetapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly BusinessContext _context;

        public VideoController(BusinessContext context)
        {
            _context = context;
        }
        [HttpGet]
        public ActionResult Get([FromQuery]PageQuery pageQuery)
        {
            try
            {
                var count = _context.Videos.Count(o => o.Name.Contains(pageQuery.search));
                var videos = _context.Videos.Where(o => o.Name.Contains(pageQuery.search))
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
                        list = videos
                    }
                });
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

                var video = _context.Videos.FirstOrDefault(o => o.Id == id);
                if (video == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有找到视频文件" });
                }
                return Ok(new Result()
                {
                    code = "1",
                    message = "success",
                    data = new
                    {
                        video.Name,
                        video.FileId,
                        video.UploaderId,
                        video.Describe,
                        video.UpdatedAt,
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetVideoUrl/{id}")]
        public async Task<ActionResult> GetVideoUrl(int id, [FromServices] IMemoryCache cache)
        {
            try
            {
                var ret = new Result() { code = "1", message = "success" };
                if (cache.TryGetValue(nameof(GetVideoUrl) + "$" + id, out var url))
                {
                    ret.data = url;
                    return Ok(ret);
                }
                var video = _context.Videos.FirstOrDefault(o => o.Id == id);
                if (video == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有找到视频文件" });
                }
                ret.data = await WXCommon.GetFileTemporaryLink(video.FileId);
                cache.Set(nameof(GetVideoUrl) + "$" + id, ret.data, TimeSpan.FromSeconds(7200));
                return Ok(ret);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post(VideoModel model)
        {
            try
            {
                var video = new Video()
                {
                    Name = model.name,
                    FileId = model.fileId,
                    CreatedAt = DateTime.Now,
                    Describe = model.describe,
                    UpdatedAt = DateTime.Now,
                    Type = model.type,
                    UploaderId = 0
                };
                await _context.Videos.AddAsync(video);
                await _context.SaveChangesAsync();
                return Ok(new Result { code = "1",message ="保存成功"});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/<PatientController>/5
        [HttpPut("{id}")]
        public ActionResult Put(int id, string name,string describe)
        {
            try
            {

                var video = _context.Videos.FirstOrDefault(o => o.Id == id);
                if (video == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有找到该视频" });
                }
                video.Name = name;
                video.Describe = describe;
                video.UpdatedAt = DateTime.Now;
                _context.Update(video);
                _context.SaveChanges();
                return Ok(new Result() { code = "1", message = "success" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/<PatientController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var video = await _context.Videos.FindAsync(id);
                if (video == null)
                {
                    return Ok(new Result() { code = "-1", message = "没有找到该视频" });
                }
                var ret = await WXCommon.DeleteUploadFile(new string[] { video.FileId });
                if (!ret)
                {
                    return Ok(new Result() { code = "-1", message = "删除失败" });
                }
                _context.Videos.Remove(video);
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
