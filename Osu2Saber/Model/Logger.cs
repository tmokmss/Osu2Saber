using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu2Saber.Model
{
    class Logger
    {
        List<ErrorInfo> errors;
        string fileName;

        public Logger()
        {
            errors = new List<ErrorInfo>();
            fileName = $"Log_{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
        }

        public void AddException(Exception e, string filePath)
        {
            lock (errors)
            {
                var error = new ErrorInfo { Exception = e, ArchiveName = Path.GetFileName(filePath) };
                errors.Add(error);
            }
        }

        public void Write()
        {
            lock (errors)
            {
                using (var writer = new StreamWriter(fileName, true))
                {
                    writer.Write(BuildErrorString());
                }
                errors.Clear();
            }
        }

        string BuildErrorString()
        {
            var sb = new StringBuilder();
            foreach (var error in errors)
            {
                sb.Append($"Error while converting - {error.ArchiveName}\n");
                sb.Append(error.Exception.ToString());
                sb.Append("\n\n");
            }
            return sb.ToString();
        }

        class ErrorInfo
        {
            public Exception Exception;
            public string ArchiveName;
        }
    }
}
