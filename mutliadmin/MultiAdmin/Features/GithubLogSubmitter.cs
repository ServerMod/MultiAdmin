using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace MultiAdmin.MultiAdmin.Features
{
    class GithubLogSubmitter : Feature
    {
        private Thread submitThread;
        public GithubLogSubmitter(Server server) : base(server)
        {
        }

        public override string GetFeatureDescription()
        {
            return "Goes through the last log file and submits any stacktraces";
        }

        public override string GetFeatureName()
        {
            return "GitHub log submitted";
        }

        public override void Init()
        {
            if (Server.MultiAdminCfg.GetBoolean("submit_errors", true))
            {
                // init happens befoe game start, so take the directory and find the latest SPC log
                var files = Directory.GetFiles(Server.logFolder, "*SPC*").ToList();
                files.Sort();
                files.Reverse();

                if (files.Count == 0) return;
                String lastGameLog = files[0];

                IEnumerable<String> lines = File.ReadLines(lastGameLog);
                List<ExceptionDetails> details = GetExceptions(lines);
                String submitted = lines.Last();

                if (!submitted.Equals("STACKTRACK SUBMITTED"))
                {
                    Server.Write("Submitting " + details.Count + " game exceptions/errors to MultiAdmin github");
                    try
                    {
                        submitThread = new Thread(new ThreadStart(() => SubmitIssues(details)));
                        submitThread.Start();
                        using (StreamWriter sw = File.AppendText(lastGameLog))
                        {
                            sw.WriteLine("STACKTRACK SUBMITTED");
                        }
                    }
                    catch (Exception e)
                    {
                        // not a big deal if we dont get the exception logged.
                    }
                }


                

            }
        }

        private void SubmitIssues(List<ExceptionDetails> details)
        {
            foreach (ExceptionDetails detail in details)
            {
                
                var postData = "identifier={0}&stacktrace={1}&labels={2}";
                var query = String.Format(postData, WebUtility.UrlEncode(detail.id), WebUtility.UrlEncode(detail.stacktrace), String.Join(",", detail.tags));
                var url = "http://stacktrack.may.mx:8000/exception?" + query;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Proxy = null;
                var response = request.GetResponse();

            }


        }


        private List<ExceptionDetails> GetExceptions(IEnumerable<String> lines)
        {
            List<ExceptionDetails> excps = new List<ExceptionDetails>();
            ExceptionDetails details = null;
            Boolean firstExpLine = false;
            foreach(String line in lines)
            {
                if (line.Contains("Exception"))
                {
                    if (details != null)
                    {
                        excps.Add(details);
                    }
                    details = new ExceptionDetails();
                    details.stacktrace += line + "\n";
                    details.id += line;
                    firstExpLine = true;
                    details.tags.Add("Autosubmission");
                    details.tags.Add("Gameissue");
                }

                if (details != null)
                {
                    if (line.Length > 2 && line.Substring(0, 2).Equals("  "))
                    {
                        details.stacktrace += line + "\n";
                        if (firstExpLine)
                        {
                            details.id += line;
                            firstExpLine = false;
                        }
                    }

                    if (line.Equals(" "))
                    {
                        excps.Add(details);
                        details = null;
                    }
                }
            }

            return excps;
        }

        public override void OnConfigReload()
        {
        }

        internal class ExceptionDetails
        {
            public String id = "";
            public String stacktrace = "";
            public List<String> tags = new List<String>();
        }
    }
}
