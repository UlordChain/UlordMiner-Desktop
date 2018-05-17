using Miner_WPF.Models;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;

namespace Miner_WPF.Commons
{
    public class FileHelper
    {
        public static AppInfo GetAppInfo(string url)
        {
            using (WebClient webClient = new WebClient() { Proxy = null })
            {
                string json = webClient.DownloadString(url);
                return JsonConvert.DeserializeObject<AppInfo>(json);
            }
        }
        public static void DownloadFile(string url, string path)
        {
            byte[] bytes = new byte[256 * 1024];
            int length = 0;
            using (WebResponse response = WebRequest.Create(url).GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (FileStream writer = new FileStream(path, FileMode.Create, FileAccess.Write))
                    {
                        while ((length = stream.Read(bytes, 0, bytes.Length)) > 0)
                        {
                            writer.Write(bytes, 0, length);
                        }
                    }
                }
            }
        }
        public static string ComputeFileMD5(string path)
        {
            using (FileStream reader = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
                byte[] bys = md5.ComputeHash(reader);
                StringBuilder sb = new StringBuilder();
                foreach (byte item in bys)
                {
                    sb.Append(item.ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
