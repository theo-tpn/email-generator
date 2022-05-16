using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;

namespace Noctus.Application.Helpers
{
    public static class FileHelper
    {
        public static string GetFileFriendlyName(string fileName)
        {
            return fileName.Split('.').First();
        }

        public static Result DeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
                return Result.Ok();
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }
        }
        public static Result SaveFile(string filePath, string fileContent)
        {
            try
            {
                File.WriteAllText(filePath, string.Empty);
                using var sw = new StreamWriter(filePath);
                sw.WriteLine(fileContent);
                sw.Dispose();
                return Result.Ok();
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }

        }

        public static Result RenameFile(string oldPath, string newPath)
        {
            try
            {
                var fileInfo = new FileInfo(oldPath);
                fileInfo.MoveTo(newPath);
                return Result.Ok();
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }

        }
    }
}
