namespace JWTUAuthLogin.Services
{
    public interface IFileValidationService
    {
        bool IsAllowedFileType(string fileName, byte[] fileContent, out string validatedExtension);
        bool IsValidImageFile(byte[] fileContent);
        bool IsValidPdfFile(byte[] fileContent);

        bool IsValidOfficeFile(string fileName,byte[] fileContent);
    }
    public class FileValidationService: IFileValidationService
    {
        // Strict allowlist of permitted extensions
        private static readonly string[] AllowedExtensions = new[]
       {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp",  // Images
            ".pdf",                                     // Documents
            ".doc", ".docx", ".xls", ".xlsx",          // Office
            ".txt", ".csv"                              // Text files
        };
        // Blocked dangerous extensions (defense in depth)
        private static readonly string[] BlockedExtensions = new[]
        {
            ".html", ".htm", ".js", ".svg", ".svgz",   // Web/Script files
            ".exe", ".bat", ".cmd", ".com", ".sh",     // Executables
            ".php", ".asp", ".aspx", ".jsp",           // Server-side scripts
            ".vbs", ".ps1", ".psm1"                    // Scripting
        };
        // Magic numbers (file signatures) for validation
        private static readonly Dictionary<string, byte[][]> FileSignatures = new()
        {
            // JPEG
            { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },

            // PNG
            { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },

            // GIF
            { ".gif", new[] {
                new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 },  // GIF87a
                new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }   // GIF89a
            } },

            // BMP
            { ".bmp", new[] { new byte[] { 0x42, 0x4D } } },  // BM

            // PDF
            { ".pdf", new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },  // %PDF

            // MS Office (DOCX, XLSX - ZIP format)
            { ".docx", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },  // PK..
            { ".xlsx", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },  // PK..

            // MS Office (DOC, XLS - OLE format)
            { ".doc", new[] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
            { ".xls", new[] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } }
        };

        public bool IsAllowedFileType(string fileName, byte[] fileContent, out string validatedExtension)
        {
            validatedExtension = string.Empty;

            // Validate file content exists
            if (fileContent == null || fileContent.Length == 0)
            {
                return false;
            }

            // Get extension and normalize
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }

            // Step 1: Check for double extensions (e.g., malicious.html.jpg)
            var fileNameLower = fileName.ToLowerInvariant();
            foreach (var blockedExt in BlockedExtensions)
            {
                if (fileNameLower.Contains(blockedExt + "."))
                {
                    // File has blocked extension before the final extension
                    return false;
                }
            }

            // Step 2: Explicitly block dangerous extensions
            if (BlockedExtensions.Contains(extension))
            {
                return false;
            }

            // Step 3: Validate extension is in allowlist
            if (!AllowedExtensions.Contains(extension))
            {
                return false;
            }

            // Step 4: Validate file signature (magic numbers)
            if (!ValidateFileSignature(extension, fileContent))
            {
                return false;
            }

            // Step 5: Additional validation for specific file types
            if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" ||
                extension == ".gif" || extension == ".bmp")
            {
                if (!IsValidImageFile(fileContent))
                {
                    return false;
                }
            }
            else if (extension == ".pdf")
            {
                if (!IsValidPdfFile(fileContent))
                {
                    return false;
                }
            }
            else if (extension == ".doc" || extension == ".docx" ||
                     extension == ".xls" || extension == ".xlsx")
            {
                if (!IsValidOfficeFile(fileName, fileContent))
                {
                    return false;
                }
            }

            validatedExtension = extension;
            return true;
        }

        private bool ValidateFileSignature(string extension, byte[] fileContent)
        {
            if (!FileSignatures.ContainsKey(extension))
            {
                // For file types without signature validation (txt, csv),
                // only allow if in allowlist and not in blocklist
                return AllowedExtensions.Contains(extension) && !BlockedExtensions.Contains(extension);
            }

            var signatures = FileSignatures[extension];
            return signatures.Any(signature =>
            {
                if (fileContent.Length < signature.Length)
                {
                    return false;
                }
                return fileContent.Take(signature.Length).SequenceEqual(signature);
            });
        }

        public bool IsValidImageFile(byte[] fileContent)
        {
            // Additional image validation
            // Check minimum file size (images should be > 100 bytes)
            if (fileContent.Length < 100)
            {
                return false;
            }

            // UPDATED 27-Dec-2025: Increased limit for mobile camera photos
            // Check maximum file size (10MB for images - allows high-quality camera photos)
            if (fileContent.Length > 10 * 1024 * 1024)
            {
                return false;
            }

            // Additional checks could be added here:
            // - Try to decode with System.Drawing.Image
            // - Check for embedded scripts in metadata

            return true;
        }

        public bool IsValidPdfFile(byte[] fileContent)
        {
            // PDF validation
            if (fileContent.Length < 1024)  // PDFs should be reasonably sized
            {
                return false;
            }

            // Check for PDF header
            if (fileContent.Length >= 5)
            {
                var header = System.Text.Encoding.ASCII.GetString(fileContent.Take(5).ToArray());
                if (header != "%PDF-")
                {
                    return false;
                }
            }

            // Check maximum file size (2MB for documents)
            if (fileContent.Length > 2 * 1024 * 1024)
            {
                return false;
            }

            return true;
        }

        public bool IsValidOfficeFile(string fileName, byte[] fileContent)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

            // Minimum size check
            if (fileContent.Length < 512)
            {
                return false;
            }

            // Check maximum file size (2MB for documents)
            if (fileContent.Length > 2 * 1024 * 1024)
            {
                return false;
            }

            // Validate signature
            return ValidateFileSignature(extension, fileContent);
        }
    }
}
