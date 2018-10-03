using System.IO;

namespace Steepshot.Core.Interfaces
{
    public interface IFileProvider
    {
        Stream GetFileStream(string filePath);

        bool Exist(string filePath);

        string GetMimeType(string filePath);
    }
}