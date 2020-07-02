using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CheckForWordUsage
{
    static class QueryContents
    {
        public static List<string> LookUp(string startFolder, string searchTerm, string repoName, List<string> matchingFiles)
        {
            // Take a snapshot of the file system.  
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(startFolder);

            // This method assumes that the application has discovery permissions  
            // for all folders under the specified path.  
            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            // Search the contents of each file.  
            // A regular expression created with the RegEx class  
            // could be used instead of the Contains method.  
            // queryMatchingFiles is an IEnumerable<string>.  
            var queryMatchingFiles =
                from file in fileList
                    where file.Extension == ".rtf"
                    let fileText = GetFileText(file.FullName)
                //let fileText = GetFileText(@"C:\Users\brkebede\Documents\configrussellsPR.ts")
                where fileText.Contains(searchTerm)
                select repoName + "," + searchTerm + "," + file.FullName;
            List<List<string>> lookUps = new List<List<string>>();
            // Execute the query.  
            Console.WriteLine("The term \"{0}\" was found in:", searchTerm);
            foreach (string filename in queryMatchingFiles)
            {
                Console.WriteLine(filename);
            }

            // Keep the console window open in debug mode.  
            //Console.WriteLine("Press any key to exit");
            //Console.ReadKey();
            matchingFiles = matchingFiles.Concat(queryMatchingFiles.ToList()).ToList();
            return matchingFiles;
        }

        // Read the contents of the file.  
        static string GetFileText(string name)
        {
            string fileContents = String.Empty;

            // If the file has been deleted since we took
            // the snapshot, ignore it and return the empty string.  
            if (System.IO.File.Exists(name))
            {
                fileContents = System.IO.File.ReadAllText(name);
            }
            return fileContents;
        }
    }
}