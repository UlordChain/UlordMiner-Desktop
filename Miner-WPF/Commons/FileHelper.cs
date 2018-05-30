using Miner_WPF.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Miner_WPF.Commons
{
    public class FileHelper
    {
        public static void SaveString(string file, string text)
        {
            File.WriteAllBytes(file, Encoding.UTF8.GetBytes(text));
        }
        public static string LoadString(string file)
        {
            return Encoding.UTF8.GetString(File.ReadAllBytes(file));
        }
        public static AppInfo GetAppInfo(string url)
        {
            using (WebClient webClient = new WebClient() { Proxy = null, Encoding = Encoding.UTF8 })
            {
                string json = webClient.DownloadString(url);
                return JsonConvert.DeserializeObject<AppInfo>(json);
            }
        }
        public static void DownloadFile(string url, string path, Action<string> action = default(Action<string>))
        {
            // Test address
            // url = "https://download.microsoft.com/download/6/7/4/674A281B-84BF-4B49-848C-14873B22F977/SQLManagementStudio_x64_ENU.exe";
            using (WebResponse response = WebRequest.Create(url).GetResponse())
            {
                long totalBytes = response.ContentLength;
                long receivedBytes = 0;
                int bufferSize = 1024 * 16;
                int chunkCount = 2;
                long chunkBytes = (int)Math.Ceiling(totalBytes * 1.0 / chunkCount);
                long lastChunkBytes = totalBytes - chunkBytes * (chunkCount - 1);
                Tuple<long, long>[] chunkInfos = new Tuple<long, long>[chunkCount];
                for (int i = 0; i < chunkCount; i++)
                {
                    if (i < chunkCount - 1)
                    {
                        chunkInfos[i] = new Tuple<long, long>(chunkBytes * i, chunkBytes);
                    }
                    else
                    {
                        chunkInfos[i] = new Tuple<long, long>(chunkBytes * i, lastChunkBytes);
                    }
                }
                chunkInfos.AsParallel().ForAll(p =>
                {
                    DownloadFile(url, path, p.Item1, p.Item2, bufferSize, totalBytes, ref receivedBytes, action);
                });
            }
        }
        private static void DownloadFile(string url, string path, long offset, long count, int bufferSize, long totalBytes, ref long receivedBytes, Action<string> action = default(Action<string>))
        {
            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            httpWebRequest.AddRange(offset, offset + count - 1);
            using (WebResponse response = httpWebRequest.GetResponse())
            {
                byte[] bytes = new byte[bufferSize];
                int length = 0;
                using (Stream stream = response.GetResponseStream())
                {
                    using (FileStream writer = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, bufferSize, true))
                    {
                        while ((length = stream.Read(bytes, 0, bytes.Length)) > 0)
                        {
                            writer.Seek(offset, SeekOrigin.Begin);
                            writer.Write(bytes, 0, length);
                            offset += length;
                            receivedBytes += length;
                            action?.Invoke($"更新进度: {(receivedBytes * 1.0 / totalBytes):P}");
                        }
                    }
                }
            }
            httpWebRequest.Abort();
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
