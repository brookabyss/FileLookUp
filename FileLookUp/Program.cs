using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CheckForWordUsage;

namespace FileLookUp
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }


        static async Task MainAsync(string[] args)
        {
            await SetUpDownload();

            List<string> searchTerms = new List<string>() { "equality", "article 8" };
            string lookUpFolder = "/Users/brookkebede/Helu";
            List<string> Headers = new List<string>()
            {
                "Source site", "Search Term", "File path"
            };
            List<string> matchingFiles = new List<string>();
            foreach (string searchTerm in searchTerms)
            {
                matchingFiles = QueryContents.LookUp(lookUpFolder, searchTerm, "www.saflii.org", matchingFiles);
            }
            string csvPath = "LookUp" + "_" + DateTime.Now.ToString("dd_MMM_hh_mm_ss_tt") + ".csv";
            CSVHelper.CreateCSVFile(matchingFiles, csvPath, string.Join(",", Headers));
        }

        static async Task SetUpDownload()
        {
            string baseYearsListUrl = @"http://www.saflii.org/za/cases/ZACC/";
            List<string> years = new List<string>();
            for (int i = 2011; i < 2021; i++)
            {
                years.Add(i.ToString());
            }

            List<string> caseNumbers = new List<string>();

            string fileExtension = ".rtf";
            for (int i = 1; i < 30; i++)
            {
                caseNumbers.Add(i.ToString());
            }

            foreach (string year in years)
            {
                foreach (string caseNumber in caseNumbers)
                {
                    string fileName = caseNumber + fileExtension;
                    string yearUrl = baseYearsListUrl + year + "/" + fileName;
                    await downloadFile(yearUrl, year, fileName);

                }
            }
        }

        static async Task downloadFile(string path,string year, string fileName)
        {
            try
            {
                UriBuilder builder = new UriBuilder(path);
                using (HttpClient client = new HttpClient())
                {
                    using (var response = await client.GetAsync(builder.Uri, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            System.IO.Directory.CreateDirectory(@"/Users/brookkebede/Helu/" + year);
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            using (var streamReader = new StreamReader(stream)) 
                            //using (FileStream file = new FileStream(@"/Users/brookkebede/Helu/" + fileName, FileMode.Create, FileAccess.Write))
                            //{
                            using (Stream streamToWriteTo = File.Open(@"/Users/brookkebede/Helu/" + year + "/" + fileName, FileMode.Create))
                            {
                                await stream.CopyToAsync(streamToWriteTo);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw; //throws 'TypeError: Failed to fetch'
            }
        }
    }
}
