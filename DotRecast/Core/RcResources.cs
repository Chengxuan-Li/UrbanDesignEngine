using System.IO;

namespace DotRecast.Core
{
    public static class RcResources
    {
        public static byte[] Load(string filename)
        {
            var filepath = filename;
            if (!File.Exists(filepath))
            {
                filepath = RcDirectory.SearchFile($"resources/{filename}");
            }
            
            var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read); // using
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);

            return buffer;
        }
    }
}