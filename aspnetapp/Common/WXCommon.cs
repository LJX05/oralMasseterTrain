using entityModel;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;

namespace aspnetapp.Common
{
    /// <summary>
    /// 微信工具类
    /// </summary>
    public class WXCommon
    {
        public static bool IsCloudEnv
        {
            get
            {
                return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MYSQL_USERNAME"));
            }
        }
        public static string APPID
        {
            get
            {
                return "wx5c9713ef9c61c63b";
            }
        }
        public static string APPSECRET
        {
            get
            {   //这个一定要妥善保存，
                return "aefaea3c58768b754c0d1eb51f8f74de";
            }
        }
        /// <summary>
        /// 云托管环境id
        /// </summary>
        public static string WxEnv
        {
            get
            {   //这个一定要妥善保存，
                return "prod-4gsbtrau8fb83a30";
            }
        }
 
        public static string ACCESS_TOKEN
        {
            get
            {
                var memoryCache = WebAppContext.Instance.ServiceProvider.GetService<IMemoryCache>();
                
                if(memoryCache?.TryGetValue("ACCESS_TOKEN",out var _ACCESS_TOKEN) == true)
                {
                    return _ACCESS_TOKEN + "";
                }
                _ACCESS_TOKEN = GetAccess_token().Result;
                memoryCache.Set("ACCESS_TOKEN", _ACCESS_TOKEN, TimeSpan.FromSeconds(7000));
                return memoryCache.Get("ACCESS_TOKEN") + "";
            }
        }
        private class AccessToken
        {
            public string access_token { get; set; } = String.Empty;
            public string expires_in { get; set; } = String.Empty;
        }
        private static async Task<string> GetAccess_token()
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={APPID}&secret={APPSECRET} ";
            var httpResponse = await new HttpClient().GetAsync(url);
            var str = await httpResponse.Content.ReadAsStringAsync();
            var accessToken = JsonConvert.DeserializeObject<AccessToken>(str);
            if (accessToken == null)
            {
                return string.Empty;
            }
            return accessToken.access_token;
        }
        /// <summary>
        /// 获取文件上传链接
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<object> GetUploadFileLink(string fileName)
        {
            var client = new HttpClient();
            var url = "https://api.weixin.qq.com/tcb/uploadfile?access_token=" + ACCESS_TOKEN;
            var obj = new
            {
                env = "prod-4gsbtrau8fb83a30",
                path = "Video/" + fileName
            };
            var content = new StringContent(JsonConvert.SerializeObject(obj));
            //获取文件连接
            var req = await client.PostAsync(url, content);
            var uploadLink = JsonConvert.DeserializeObject<UploadLink>(await req.Content.ReadAsStringAsync());
            if (uploadLink?.errcode != "0")
            {
                throw new Exception(uploadLink?.errmsg);
            }
            return new
            {
                key = obj.path,
                authorization = uploadLink.authorization,
                token = uploadLink.token,
                cos_file_id = uploadLink.cos_file_id,
                url = uploadLink.url,
                fileid = uploadLink.file_id,
            };
        }


        public static async Task<string> GetOpenId(string code)
        {
            var client = new HttpClient();
            var url = $"https://api.weixin.qq.com/sns/jscode2session?appid={APPID}&secret={APPSECRET}&js_code={code}&grant_type=authorization_code";
           
            //获取文件连接
            var req = await client.PostAsync(url, null);
            var jsonob = JObject.Parse(await req.Content.ReadAsStringAsync());
            var errcode = jsonob["errcode"] +"";
            var openid = jsonob["openid"] + "";
            var session_key = jsonob["openid"] + "";
            var unionid = jsonob["unionid"] + "";
            if (string.IsNullOrEmpty(openid))
            {
                throw new Exception("获取openid失败---errcode" + errcode); 
            }

            return openid;
        }

        /// <summary>
        /// 删除视频
        /// </summary>
        /// <param name="fileids"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<bool> DeleteUploadFile(string[] fileids)
        {
            using (var client = new HttpClient())
            {

                var url = "https://api.weixin.qq.com/tcb/batchdeletefile?access_token=" + ACCESS_TOKEN;
                var obj = new
                {
                    env = WxEnv,
                    fileid_list = fileids
                };
                using (var content = new StringContent(JsonConvert.SerializeObject(obj)))
                {

                    //获取文件连接
                    var req = await client.PostAsync(url, content);
                    var result = JsonConvert.DeserializeObject<WxBaseResult>(await req.Content.ReadAsStringAsync());
                    if (result?.errcode != "0")
                    {
                        throw new Exception(result?.errmsg);
                    }
                    return true;
                }
            }
        }
        private class FileTemporaryLink : WxBaseResult
        {
            public IList<WxFile> file_list { get; set; }

        }
        private class WxFile
        {
            public string fileid { get; set; } = String.Empty;
            public string download_url { get; set; } = String.Empty;
            public string status { get; set; } = String.Empty;
            public string errmsg { get; set; } = String.Empty;
        }
        /// <summary>
        /// 获取临时文件下载链接
        /// </summary>
        /// <param name="fileids"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<string> GetFileTemporaryLink(string fileid,int maxAge = 7200)
        {
            using var client = new HttpClient();
            var url = "";
            if (IsCloudEnv)
            {
                url = "https://api.weixin.qq.com/tcb/batchdownloadfile";
            }
            else
            {
                url = "https://api.weixin.qq.com/tcb/batchdownloadfile?access_token=" + ACCESS_TOKEN;
            }
            
            
            var flie = new
            {
                fileid = fileid,
                max_age = maxAge
            };
            var obj = new
            {
                env = WxEnv,
                file_list = new object[] { flie }
            };
            using var content = new StringContent(JsonConvert.SerializeObject(obj));
            content.Headers.Clear();
            content.Headers.Add("Content-Type", " application/json"); 
            //获取文件连接
            var req = await client.PostAsync(url, content);
            var result = JsonConvert.DeserializeObject<FileTemporaryLink>(await req.Content.ReadAsStringAsync());
            if (result?.errcode != "0")
            {
                throw new Exception(result?.errmsg);
            }
            return result.file_list[0].download_url;
        }


        /// <summary>
        /// 获取临时文件下载链接
        /// </summary>
        /// <param name="fileids"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<bool> SendMessage(WeMessageTemplate weMessage,object data)
        {
            using var client = new HttpClient();

            var url = "";
             url = "https://api.weixin.qq.com/cgi-bin/message/subscribe/send?access_token=" + ACCESS_TOKEN;
         
            var obj = new
            {
                touser = weMessage.OpenId,
                template_id = weMessage.TempId,
                data = data
            };
            using var content = new StringContent(JsonConvert.SerializeObject(obj));
            content.Headers.Clear();
            content.Headers.Add("Content-Type", " application/json");
            //获取文件连接
            var req = await client.PostAsync(url, content);
            var result = JsonConvert.DeserializeObject<WxBaseResult>(await req.Content.ReadAsStringAsync());
            if (result?.errcode != "0")
            {
                throw new Exception(result?.errmsg);
            }
            return true;
        }

        public static string PostMultipartFormData(string url, NameValueCollection nameValueCollection, byte[] file, string fileName)
        {
            using (var client = new HttpClient())
            {


                using (var content = new MultipartFormDataContent())
                {
                    string boundary = string.Format("--{0}", DateTime.Now.Ticks.ToString("x"));
                    content.Headers.Clear();
                    content.Headers.Add("ContentType", $"multipart/form-data, boundary={boundary}");
                    string[] allKeys = nameValueCollection?.AllKeys;
                    foreach (string key in allKeys)
                    {
                        var dataContent = new ByteArrayContent(Encoding.UTF8.GetBytes(nameValueCollection[key]));
                        dataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = key
                        };
                        content.Add(dataContent);
                    }
                    //处理文件内容
                    var fileContent = new ByteArrayContent(file);//填充文件字节
                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "file",
                        FileName = fileName
                    };
                    var result = client.PostAsync(url, content).Result;//post请求
                    string data = result.Content.ReadAsStringAsync().Result;
                    return data;//返回操作结果
                }
            }
        }

        /// <summary>
        /// 上传链接
        /// </summary>
        private class UploadLink : WxBaseResult
        {
            public string url { get; set; } = String.Empty;
            public string token { get; set; } = String.Empty;
            public string authorization { get; set; } = String.Empty;
            public string file_id { get; set; } = String.Empty;
            public string cos_file_id { get; set; } = String.Empty;
            public string key { get; set; } = String.Empty;
        }
        /// <summary>
        /// 微信通信基类
        /// </summary>
        class WxBaseResult
        {
            public string errcode { get; set; } = String.Empty;
            public string errmsg { get; set; } = String.Empty;
        }
    }



}
