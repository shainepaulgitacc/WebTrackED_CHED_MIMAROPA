using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Drawing;

namespace WebTrackED_CHED_MIMAROPA.Model.Service
{
    public class FileUploader
    {
        private readonly IWebHostEnvironment _env;
        public FileUploader(IWebHostEnvironment env)
        {
            _env = env;
        }
        public async Task<string> Uploadfile(IFormFile? file,string? folderName)
        {
            string fileName = null;
            if (file != null)
            {
                string uploadDr = Path.Combine(_env.WebRootPath, folderName);
                fileName = Guid.NewGuid().ToString() + "=" + file.FileName;
                string filePath = Path.Combine(uploadDr, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

            }
            return fileName;
        }
        public async Task<byte[]> DownloadFile(string fileName)
        {
            const string folderName = "Documents";
            string filePath = Path.Combine(_env.WebRootPath, folderName, fileName);
            if (File.Exists(filePath))
            {
                return await File.ReadAllBytesAsync(filePath);
            }
            else
            {
                throw new FileNotFoundException("The specified file was not found.");
            }
        }

        #region --view file function--
        public async Task<(string,string)> ViewFile(string fileName)
        {
            const string folderName = "Documents";
            string filePath = Path.Combine(_env.WebRootPath, folderName, fileName);
            if (File.Exists(filePath))
            {
                string contentType = GetContentType(Path.GetExtension(filePath));
                return (filePath, contentType);
            }
            else
            {
                throw new FileNotFoundException("The specified file was not found.");
            }

        }
        private string GetContentType(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xls":
                    return "application/vnd.ms-excel";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".ppt":
                    return "application/vnd.ms-powerpoint";
                case ".pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                default:
                    return "application/octet-stream"; // Default MIME type for unknown file types
            }
        }
        #endregion
    }
}
