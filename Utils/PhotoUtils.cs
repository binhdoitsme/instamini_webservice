using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace InstaminiWebService.Utils
{
    public static class PhotoUtils
    {
        private static bool ValidatePhotoUpload(IFormFile uploadedPhoto)
        {
            if (uploadedPhoto is null || uploadedPhoto.Length == 0)
            {
                return false;
            }
            try
            {
                var img = Image.FromStream(uploadedPhoto.OpenReadStream());
                return true;
            }
            catch
            {
                // bad image
                return false;
            }
        }

        /// <summary>
        /// Upload a photo asynchronously then return the file name.
        /// </summary>
        /// <param name="uploadedPhoto">The photo to be uploaded</param>
        /// <param name="targetDir">The path to save the newly uploaded file</param>
        /// <returns></returns>
        public static async Task<string> UploadPhotoAsync(IFormFile uploadedPhoto, string targetDir)
        {
            if (!ValidatePhotoUpload(uploadedPhoto))
            {
                throw new InvalidDataException();
            }
            var fileName = $"{Guid.NewGuid()}_{uploadedPhoto.FileName}";
            var targetFile = $"{targetDir}/{fileName}";
            using (var fileSteam = new FileStream(targetFile, FileMode.Create))
            {
                await uploadedPhoto.CopyToAsync(fileSteam);
            }
            return fileName;
        }
    }
}
