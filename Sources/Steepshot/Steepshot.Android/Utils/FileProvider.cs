using System.IO;
using Java.IO;
using Java.Net;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Utils;

namespace Steepshot.Utils
{
    public class FileProvider : IFileProvider
    {
        public Stream GetFileStream(string filePath)
        {
            var file = new Java.IO.File(filePath);
            var fileInputStream = new FileInputStream(file);
            var stream = new StreamConverter(fileInputStream, null);
            return stream;
        }

        public bool Exist(string filePath)
        {
            var file = new Java.IO.File(filePath);
            return file.Exists();
        }

        public string GetMimeType(string filePath)
        {
            var mt = URLConnection.FileNameMap.GetContentTypeFor(filePath);
            if (string.IsNullOrEmpty(mt))
            {
                var index = filePath.LastIndexOf('.');
                if (index > -1)
                {
                    var extension = filePath.Substring(index);
                    mt = MimeTypeHelper.GetMimeType(extension);
                }
            }
            return mt;
        }
    }
}