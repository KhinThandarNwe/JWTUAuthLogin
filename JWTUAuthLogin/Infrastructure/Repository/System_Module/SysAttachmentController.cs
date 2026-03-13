using JWTUAuthLogin.DBModel;
using JWTUAuthLogin.DBModels.DB_UnitOfWork;
using JWTUAuthLogin.DTO.System_Module;
using JWTUAuthLogin.Infrastructure.Repository.Token_Module;
using JWTUAuthLogin.Services;
using JWTUAuthLogin.Shared.Common;
using JWTUAuthLogin.Shared.Enums;
using JWTUAuthLogin.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace JWTUAuthLogin.Infrastructure.Repository.System_Module
{
    public interface ISysAttachmentController
    {
        Task<ServiceActionResult> UploadFile(
            MultipartReader reader,
            MultipartSection section,
            string recordType,
            string recordId,
            string photoCategory
            );
        Task<ServiceActionResult> UploadBase64FileAsync(AttachmentDTO attachment);
        ServiceActionResult Remove(string recorderId);
    }
    public class SysAttachmentController : ISysAttachmentController
    {
        private string ControllerLogLable = "FileUploadController";
        private MBDatabaseContext _mb;
        private readonly ITokenManager _tokenManager;
        private string requestId;
        private string licenseId;
        //private AppSetting _appSetting;
        //private ISysLogController _logControl;
        private readonly IFileValidationService _fileValidator;

        public SysAttachmentController(
            IUnitOfWork unitOfWork,
            ITokenManager tokenManager
            )
        {
            _mb = unitOfWork.DBContext;
            _tokenManager = tokenManager;
            _fileValidator = new FileValidationService();
        }
        public async Task<ServiceActionResult> UploadFile(
            MultipartReader reader,
            MultipartSection? section,
            string recordType,
            string recordId,
            string photoCategory
            )
        {
            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (hasContentDispositionHeader)
                {
                    string fileNameWithoutDoubleQuote = contentDisposition.FileName.Value.Replace("\"", "");

                    if (contentDisposition.DispositionType.Equals("form-data") && !string.IsNullOrEmpty(fileNameWithoutDoubleQuote) || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value))
                    {
                        string filePath = Path.GetFullPath(Path.Combine(
                            Environment.CurrentDirectory,
                            "UploadedFiles/" + recordType + "/" + photoCategory));

                        byte[] fileArray;
                        using (var memoryStream = new MemoryStream())
                        {
                            await section.Body.CopyToAsync(memoryStream);
                            fileArray = memoryStream.ToArray();
                        }
                        //Validate file type using magic numbers and extension checks
                        if (!_fileValidator.IsAllowedFileType(fileNameWithoutDoubleQuote, fileArray, out string validatedExtension))
                        {
                            return new ServiceActionResult(
                                Shared.Enums.ReturnStatus.BadRequest,
                                "File upload rejected: Invalid file type or file content does not match extension", "");
                        }
                        if (Directory.Exists(filePath) == false)
                        {
                            Directory.CreateDirectory(filePath);
                        }
                        string fileName = Guid.NewGuid().ToString() + validatedExtension;
                        string fullFilePath = Path.Combine(filePath, fileName);
                        string returnImageURL = recordType + "/" + photoCategory + "/" + fileName;
                        decimal fileSize = Convert.ToDecimal(fileArray.Length) / (1024.0m * 1024.0m); // Convert to MB
                        string originalFileName = fileNameWithoutDoubleQuote.ToString().Replace(" ", "");

                        SaveToDatabase(
                             recordType,
                            recordId,
                            photoCategory,
                            "content/" + returnImageURL,
                            originalFileName,
                            fileSize,
                            fileName
                            );
                        using (var fileStream = File.Create(fullFilePath))
                        {
                            await fileStream.WriteAsync(fileArray, 0, fileArray.Length);
                        }
                    }
                    section = await reader.ReadNextSectionAsync();
                }

            }
            return new ServiceActionResult(
                   Shared.Enums.ReturnStatus.success,
                   "File uploaded successfully",
                   "");

        }

        public async Task<ServiceActionResult> UploadBase64FileAsync(AttachmentDTO attachment)
        {
            try
            {
                // Extract file extension from the fileName
                var fileExtension = Path.GetExtension(attachment.FileName);
                if (string.IsNullOrEmpty(fileExtension))
                {
                    return new ServiceActionResult(
                        Shared.Enums.ReturnStatus.BadRequest,
                        "File upload rejected: Missing file extension",
                        "");
                }
                //Decode base64 to byte arrat
                byte[] fileBytes;
                try
                {
                    string base64Data = attachment.ImageData;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }
                    fileBytes = Convert.FromBase64String(base64Data);
                }
                catch (FormatException)
                {
                    return new ServiceActionResult(
                        Shared.Enums.ReturnStatus.BadRequest,
                        "File upload rejected: Invalid base64 string",
                        "");
                }
                if (!_fileValidator.IsAllowedFileType(attachment.FileName, fileBytes, out string validatedExtension))
                {
                    return new ServiceActionResult(
                        Shared.Enums.ReturnStatus.BadRequest,
                        "File upload rejected: Invalid file type or file content does not match extension",
                        "");
                }
                string safeFileName = Guid.NewGuid().ToString() + validatedExtension;
                string folderPath = Path.Combine(Environment.CurrentDirectory, "UploadedFiles", attachment.RefType, attachment.RefCategory);

                string fullFilePath = Path.Combine(folderPath, safeFileName);

                //Ensure directory exits
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                // Save file to disk
                await File.WriteAllBytesAsync(fullFilePath, fileBytes);

                //Generate file size in MB
                decimal fileSizeMB = Convert.ToDecimal(fileBytes.Length) / (1024.0m * 1024.0m);

                // Build return URL
                string returnImageURL = $"{attachment.RefType}/{attachment.RefCategory}/{safeFileName}";
                string fullUrl = $"content/{returnImageURL}";

                //Save metadata to database
                SaveToDatabase(
                     attachment.RefType,
                    attachment.RefID,
                    attachment.RefCategory,
                    fullUrl,
                    attachment.FileName.Replace(" ", ""),
                    fileSizeMB,
                    safeFileName
                    );
                return new ServiceActionResult(Shared.Enums.ReturnStatus.success, "File uploaded successfully", "");
            }
            catch (Exception ex)
            {
                return new ServiceActionResult(
                    Shared.Enums.ReturnStatus.InternalServerError,
                    $"An error occurred: {ex.Message}",
                    "");
            }
        }
        private ServiceActionResult SaveToDatabase(
          string recordType,
          string recordId,
          string photoCategory,
          string photURL,
          string originalFileName,
          decimal fileSize,
          string fileName
      )
        {
            AuthorizedUser au = _tokenManager.GetAuthorizedUser();
            string requestid = au.userId;
            string licenseid = au.licenseId;
            try
            {
                if (recordType == "emp-profile")
                {
                    SysAttachment? old_record = _mb.SysAttachments.FirstOrDefault(c =>
                        c.ReferenceId == recordId && c.Active == true && c.Sector == photoCategory
                    );

                    if (old_record != null)
                    {
                        old_record.Active = false;
                        old_record.ModifiedOn = DateTime.Now;
                        old_record.ModifiedBy = requestid;
                        _mb.SaveChanges();
                    }
                }
                // if (photoCategory == "cover")
                // {
                //     SysAttachment? old_record = _context.SysAttachments.FirstOrDefault(c =>
                //         c.ReferenceId == recordId && c.Active == true && c.Sector == photoCategory
                //     );

                //     if (old_record != null)
                //     {
                //         old_record.Active = false;
                //         old_record.ModifiedOn = DateTime.Now;
                //         old_record.ModifiedBy = requestid;
                //         _context.SaveChanges();
                //     }
                // }

                // else if (
                //     recordType == "brand"
                //     || recordType == "category"
                //     || recordType == "SKUPrice"
                //     || recordType == "group"
                //     || recordType == "companyProfile"
                // )
                // {
                //     SysAttachment? old_record = _context.SysAttachments.FirstOrDefault(c =>
                //         c.ReferenceId == recordId && c.Active == true && c.Sector == photoCategory
                //     );

                //     if (old_record != null)
                //     {
                //         old_record.Active = false;
                //         old_record.ModifiedOn = DateTime.Now;
                //         old_record.ModifiedBy = requestid;
                //         _context.SaveChanges();
                //     }
                // }
                SysAttachment dataRecord = new SysAttachment();
                dataRecord.AttachId= Guid.NewGuid().ToString();
                dataRecord.ReferenceId = recordId;
                dataRecord.ReferenceType = recordType;
                dataRecord.Sector = photoCategory;
                dataRecord.Remark = "";
                dataRecord.Active = true;
                dataRecord.IsMobileAtt = true;

                dataRecord.FileSize = Math.Round(fileSize, 3);

                dataRecord.AttachUrl = photURL;
                dataRecord.AttachFileName = fileName;
                dataRecord.AttachFilePath = fileName;
                dataRecord.AttachOn = DateTime.Now;
                dataRecord.OriginalFileName = originalFileName;
                dataRecord.ModifiedOn = DateTime.Now;
                dataRecord.CreatedOn = DateTime.Now;
                dataRecord.ModifiedBy = requestid;
                dataRecord.CraatedBy = requestid;
                dataRecord.LicenseId = licenseid;
                dataRecord.LastAction = Guid.NewGuid().ToString();

                _mb.SysAttachments.Add(dataRecord);

                _mb.SaveChanges();
                return new ServiceActionResult(ReturnStatus.success, "", "");
            }
            catch (Exception ex)
            {
                return Common_Methods.doErrorLog(ControllerLogLable, ex.Message, ex, requestid);
            }
        }
        public ServiceActionResult Remove(string recordid)
        {
            AuthorizedUser au = _tokenManager.GetAuthorizedUser();
            string requestid = au.userId;
            string licenseid = au.licenseId;
            try
            {
                var dataRecord = _mb.SysAttachments.FirstOrDefault(c =>
                    c.AttachId.ToString() == recordid
                );
                if (dataRecord != null)
                {
                    dataRecord.Active = false;
                    dataRecord.LastAction = Guid.NewGuid().ToString();
                    dataRecord.ModifiedOn = DateTime.Now;

                    //Write System Log Main Record
                    //_logControl.writeLog(
                    //    _mb,
                    //    dataRecord,
                    //    "SysAttachment",
                    //    dataRecord.OriginalFileName,
                    //    ControllerLogLable,
                    //    dataRecord.AttachId,
                    //    requestid
                    //);

                    _mb.SaveChanges();

                    //content/products/attachment/NAWWINSANDARPHYOMS_1.pdf
                    string filePath = Path.GetFullPath(
                        Path.Combine(
                            Environment.CurrentDirectory,
                            dataRecord.AttachUrl.Replace("content", "UploadedFiles")
                        )
                    );
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    return new ServiceActionResult(ReturnStatus.success);
                }

                return new ServiceActionResult(ReturnStatus.NotFound);
            }
            catch (Exception ex)
            {
                return Common_Methods.doErrorLog(ControllerLogLable, ex.Message, ex, requestid);
            }
        }
    }
}