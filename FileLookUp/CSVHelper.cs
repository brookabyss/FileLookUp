using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CheckForWordUsage
{
    static class CSVHelper
    {
        private static string TempFolderName = "LookUpCSVFiles";

        private static string BasePath = @"/Users/brookkebede/Helu/";
        public static void CreateCSVFile(List<string> allLines, string csvPath, string headers)
        {
            try
            {
                string folderPath = CreateSupportTempFolder();
                if (folderPath != null)
                {
                    string filePath = folderPath + "//" + csvPath;

                    var csv = new StringBuilder();
                    csv.AppendLine(headers);
                    allLines.ForEach(lines =>
                    {
                        csv.AppendLine(string.Join(",", lines));
                    });

                    if (!File.Exists(filePath))
                    {
                        File.WriteAllText(filePath, csv.ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Create a temporary folder for storing downloaded attachments
        /// </summary>
        /// <returns>The path to the temporary directory</returns>
        public static string CreateSupportTempFolder()
        {
            try
            {
                string attachmentFolderPath = Path.Combine(BasePath, TempFolderName);
                if (!Directory.Exists(attachmentFolderPath))
                {
                    Directory.CreateDirectory(attachmentFolderPath);
                }
                return attachmentFolderPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}