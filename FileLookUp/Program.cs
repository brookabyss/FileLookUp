using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CheckForWordUsage;
using HtmlAgilityPack;

namespace FileLookUp
{
    class Program
    {

        static void Main(string[] args)
        {
            //MainAsync(args).GetAwaiter().GetResult();
            LookUpHelper();
        }


        static async Task MainAsync(string[] args)
        {
            await SetUpDownload();
           // await downloadUKFile();
            
        }

        static void LookUpHelper()
        {
            List<string> searchTerms = new List<string>() { @"inequality", @"equality", @"article 8", @"article 9", @"section 8", @"section 9", @"non-discrimination", @"discrimination" };
            //List<string> searchTerms = new List<string>() { @"equality", @"article 8(?![0-9])", @"article 9(?![0-9])", @"section 8(?![0-9])", @"section 9(?![0-9])", @"non-discrimination", @"discrimination" };
            string lookUpFolder = "/Users/brookkebede/Helu";
            List<string> Headers = new List<string>()
            {
                "Source site", "Search Term", "File path"
            };
            List<string> matchingFiles = new List<string>();
            Dictionary<string, string> matchedPathsTerm = new Dictionary<string, string>();
            foreach (string searchTerm in searchTerms)
            {
                matchingFiles = QueryContents.LookUp(lookUpFolder, searchTerm, "www.saflii.org", matchingFiles);
            }
            string csvPath = "LookUp" + "_" + DateTime.Now.ToString("dd_MMM_hh_mm_ss_tt") + ".csv";
            foreach (string m in matchingFiles)
            {
                string[] values = m.Split(",");
                string urlPathForCase = values[2];
                string sT = values[1];
                if (matchedPathsTerm.ContainsKey(urlPathForCase))
                {
                    matchedPathsTerm[urlPathForCase] = matchedPathsTerm[urlPathForCase] + "_" + sT;
                }
                else
                {
                    matchedPathsTerm[urlPathForCase] = sT;
                }
            }
            List<string> newMatchingFiles = new List<string>();

            foreach (KeyValuePair<string, string> acct in matchedPathsTerm)
            {
                newMatchingFiles.Add("www.saflii.org" + "," + acct.Value + "," + acct.Key);
            }
            CSVHelper.CreateCSVFile(newMatchingFiles, csvPath, string.Join(",", Headers));
        }

        static async Task SetUpDownload()
        {
            string baseYearsListUrl = @"http://www.saflii.org/za/cases/ZACC/";
            List<string> years = new List<string>();
            for (int i = 2010; i < 2011; i++)
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


        static async Task downloadUKFile()
        {
            try
            {
                // string ukUrlBase = "https://www.supremecourt.uk";
                List<string> years = new List<string>() { "index.html" };
                for(int i=2019; i >2008; i--)
                {
                    years.Add(i.ToString() + ".html");
                }
                string baseYear = "https://www.supremecourt.uk/decided-cases/";
                foreach(string y in years)
                {
                    string yUrl = baseYear + y;
                    UriBuilder builder = new UriBuilder(yUrl);
                    using (HttpClient client = new HttpClient())
                    {
                        using (var response = await client.GetAsync(builder.Uri, HttpCompletionOption.ResponseHeadersRead))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                System.IO.Directory.CreateDirectory(@"/Users/brookkebede/Helu/UK");
                                using (var stream = await response.Content.ReadAsStreamAsync())
                                using (var streamReader = new StreamReader(stream))
                                //using (FileStream file = new FileStream(@"/Users/brookkebede/Helu/" + fileName, FileMode.Create, FileAccess.Write))
                                //{
                                using (Stream streamToWriteTo = File.Open(@"/Users/brookkebede/Helu/UK/"+ y, FileMode.Create))
                                {
                                    await stream.CopyToAsync(streamToWriteTo);
                                }
                            }
                        }
                    }
                }

                Dictionary<string, string> casesPerYear = new Dictionary<string, string>();
                casesPerYear = QueryContents.UKLookUp("/Users/brookkebede/Helu/UK", casesPerYear);

                foreach(KeyValuePair<string,string> yearCase in casesPerYear)
                {
                        // string ukUrlBase = "https://www.supremecourt.uk";
                        List<string> cases = yearCase.Value.Split(",").ToList();
                        List<string> pdfPaths = new List<string>();
                        string baseCaseUrl= "https://www.supremecourt.uk";
                        foreach (string c in cases)
                        {
                            string yUrl = baseCaseUrl + c;
                            UriBuilder builder = new UriBuilder(yUrl);
                            using (HttpClient client = new HttpClient())
                            {
                            var html = await client.GetStringAsync(builder.Uri);
                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(html);
                            HtmlNode node = doc.DocumentNode.Descendants("a").Single(n =>
                            {
                                if (n == null)
                                {
                                    Console.WriteLine("No pdf link found for " + yearCase.Key);
                                }

                                return n.GetAttributeValue("href", "").IndexOf("judgment.pdf", StringComparison.OrdinalIgnoreCase) >= 0;
                            });
                            if (node != null)
                            {
                                pdfPaths.Add(node.GetAttributeValue("href", ""));
                            }
                        }
                            foreach(string p in pdfPaths)
                            {
                                    
                                  using (WebClient client = new WebClient())
                                  {
                                    System.IO.Directory.CreateDirectory(@"/Users/brookkebede/Helu/UK/web/" + yearCase.Key);
                                    string pUrl = baseCaseUrl + "/cases/" + p;
                                    UriBuilder pdfPathbuilder = new UriBuilder(pUrl);
                                    client.DownloadFile(pdfPathbuilder.Uri, @"/Users/brookkebede/Helu/UK/web/" + yearCase.Key+"/"+p.Split("/")[1]);
                                             
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
