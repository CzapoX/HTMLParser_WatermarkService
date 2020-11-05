using System.IO;
using System.Threading.Tasks;

namespace Limalima.Backend.Azure
{
    public interface IAzureImageUploadComponent
    {
        Task<bool> UploadFileToStorage(string localFilePath, string fileName);
    }
}