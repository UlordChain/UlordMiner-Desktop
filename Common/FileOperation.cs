using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace Common
{
    public class FileOperation
    {
        public static void SolatedStorageWrite<T>(string file, T t)
        {
            SolatedStorageWrite(file, JsonConvert.SerializeObject(t));
        }
        public static void SolatedStorageWrite(string file, string text)
        {
            using (IsolatedStorageFile isolatedStrorageFile = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                if (!isolatedStrorageFile.FileExists(file))
                {
                    string dir = Path.GetDirectoryName(file);
                    if (!string.IsNullOrEmpty(dir) && !isolatedStrorageFile.DirectoryExists(dir))
                    {
                        isolatedStrorageFile.CreateDirectory(dir);
                    }
                }
                using (IsolatedStorageFileStream isolatedStorageFileStream = new IsolatedStorageFileStream(file, FileMode.Create, isolatedStrorageFile))
                {
                    using (StreamWriter streamWriter = new StreamWriter(isolatedStorageFileStream))
                    {
                        streamWriter.Write(text);
                    }
                }
            }
        }
        public static T SolatedStorageRead<T>(string file)
        {
            return JsonConvert.DeserializeObject<T>(SolatedStorageRead(file) ?? string.Empty);
        }
        public static string SolatedStorageRead(string file)
        {
            using (IsolatedStorageFile isolatedStrorageFile = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                if (!isolatedStrorageFile.FileExists(file))
                {
                    return default(string);
                }
                using (IsolatedStorageFileStream isolatedStorageFileStream = new IsolatedStorageFileStream(file, FileMode.Open, isolatedStrorageFile))
                {
                    using (StreamReader streamReader = new StreamReader(isolatedStorageFileStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
        public static void LocalWrite<T>(string file, T t)
        {
            if (string.IsNullOrEmpty(Path.GetPathRoot(file)))
            {
                file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
            }
            string dir = Path.GetDirectoryName(file);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(file, JsonConvert.SerializeObject(t));
        }
        public static T LocalRead<T>(string file)
        {
            if (string.IsNullOrEmpty(Path.GetPathRoot(file)))
            {
                file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
            }
            if (File.Exists(file))
            {
                string json = File.ReadAllText(file);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            return default(T);
        }
    }
}
