using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

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
                where file.Extension == ".rtf" && file.FullName.Split("Helu/")[1].IndexOf("/", StringComparison.OrdinalIgnoreCase) >= 0 && Int32.Parse(file.FullName.Split("Helu/")[1].Split("/")[0]) % 2 == 0
                let fileText = GetFileText(file.FullName)
                //let fileText = GetFileText(@"C:\Users\brkebede\Documents\configrussellsPR.ts")
                // where fileText.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0
                where  Regex.IsMatch(fileText, searchTerm, RegexOptions.IgnoreCase)
                //where new Regex(searchTerm, RegexOptions.IgnoreCase).Match(fileText) != null
                select repoName + "," + searchTerm + "," + @"http://www.saflii.org/za/cases/ZACC/"+file.FullName.Split("Helu/")[1].Replace(".rtf",".pdf");
            List<List<string>> lookUps = new List<List<string>>();
            // Execute the query.  
            Console.WriteLine("The term \"{0}\" was found in:", searchTerm);
            foreach (string filename in queryMatchingFiles)
            {
                Console.WriteLine(filename);
            }

            // Keep the console window open in debug mode.  
            //Console.WriteLine("Press any key to exit");
            //Console.ReadKey()
            matchingFiles = matchingFiles.Concat(queryMatchingFiles.ToList()).ToList();
            return matchingFiles;
        }

        public static Dictionary<string, string> UKLookUp(string startFolder, Dictionary<string, string> casesPerYear)
        {
            // Take a snapshot of the file system.  
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(startFolder);

            // This method assumes that the application has discovery permissions  
            // for all folders under the specified path.  
            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            
            foreach (System.IO.FileInfo file in fileList)
            {
                if(file.Extension == ".html")
                {
                    HtmlDocument doc = new HtmlDocument();
                    doc.Load(file.FullName);
                    string year = file.Name.Split(".")[0];
                    year = year == "index" ? "2020" : year;
                    casesPerYear.Add(year, "");
                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        HtmlAttribute att = link.Attributes["href"];
                        if (att.Value != null && att.Value.IndexOf("uksc-", StringComparison.OrdinalIgnoreCase) >=0){
                            if (string.IsNullOrEmpty(casesPerYear[year]))
                            {
                                casesPerYear[year] = att.Value;
                            }
                            else
                            {
                                casesPerYear[year] = casesPerYear[year] + "," + att.Value;
                            }
                        }
                    }
                }
            }

            /**

            // Search the contents of each file.  
            // A regular expression created with the RegEx class  
            // could be used instead of the Contains method.  
            // queryMatchingFiles is an IEnumerable<string>.  
            var queryMatchingFiles =
                from file in fileList
                where file.Extension == ".html"
                let fileText = GetFileText(file.FullName)
                //let fileText = GetFileText(@"C:\Users\brkebede\Documents\configrussellsPR.ts")
                // where fileText.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0
                where new Regex(searchTerm, RegexOptions.IgnoreCase).Match(fileText) != null
                select repoName + "," + searchTerm + "," + @"https://www.supremecourt.uk/cases/" + file.FullName.Split("Helu/")[1].Replace(".rtf", ".pdf");
            List<List<string>> lookUps = new List<List<string>>();
            // Execute the query.  
            Console.WriteLine("The term \"{0}\" was found in:", searchTerm);
            foreach (string filename in queryMatchingFiles)
            {
                Console.WriteLine(filename);
            }

            // Keep the console window open in debug mode.  
            //Console.WriteLine("Press any key to exit");
            //Console.ReadKey()
            matchingFiles = matchingFiles.Concat(queryMatchingFiles.ToList()).ToList();

            **/
            return casesPerYear;
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