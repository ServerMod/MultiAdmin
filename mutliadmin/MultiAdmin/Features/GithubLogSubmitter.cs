using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace MultiAdmin.MultiAdmin.Features
{
	[Feature]
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
			if (Server.MultiAdminCfg.config.GetBool("submit_errors", false))
			{
				// init happens befoe game start, so take the directory and find the latest SPC log
				var files = Directory.GetFiles(Server.LogFolder, "*SCP*").ToList();

				files.Sort();
				files.Reverse();

				if (files.Count == 0) return;
				string lastGameLog = files[0];

				try
				{
					IEnumerable<string> lines = File.ReadLines(lastGameLog);
					List<ExceptionDetails> details = GetExceptions(lines);
					string submitted = lines.Last();

					if (!submitted.Equals("STACKTRACK SUBMITTED"))
					{
						Server.Write("Submitting " + details.Count + " game exceptions/errors to MultiAdmin github");
						submitThread = new Thread(new ThreadStart(() => SubmitIssues(details)));
						submitThread.Start();
						using (StreamWriter sw = File.AppendText(lastGameLog))
						{
							sw.WriteLine("STACKTRACK SUBMITTED");
						}
					}

				}

				catch
				{
					Server.Write("Failed to open log for github error submission, the SCPSL exe for that session is still shutting down.");
					//Console.WriteLine(e.Message);
					// not a big deal if we dont get the exception logged.
				}




			}
		}

		private void SubmitIssues(List<ExceptionDetails> details)
		{
			try
			{
				foreach (ExceptionDetails detail in details)
				{

					var postData = "identifier={0}&stacktrace={1}&seen={2}&labels={3}";
					var query = string.Format(postData, WebUtility.UrlEncode(detail.id), WebUtility.UrlEncode(detail.stacktrace), detail.seen.ToString(), string.Join(",", detail.tags));
					var url = "http://stracktrack.may.mx:8000/exception?" + query;
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
					request.Method = "POST";
					request.Proxy = null;
					var response = request.GetResponse();
				}
			}
			catch
			{
			}



		}


		private List<ExceptionDetails> GetExceptions(IEnumerable<string> lines)
		{
			List<ExceptionDetails> excps = new List<ExceptionDetails>();
			ExceptionDetails details = null;
			bool firstExpLine = false;
			foreach (string line in lines)
			{
				if (line.Contains("Exception"))
				{
					if (details != null)
					{
						AddException(excps, details);
					}
					details = new ExceptionDetails();
					details.id += line;
					firstExpLine = true;
					details.tags.Add("Autosubmission");
					details.tags.Add("Gameissue");
				}

				if (details != null)
				{
					details.stacktrace += line + "\n";
					if (firstExpLine)
					{
						details.id += line;
						firstExpLine = false;
					}

					if (line.Equals(" "))
					{
						details.stacktrace.Trim();
						AddException(excps, details);
						details = null;
					}
				}
			}

			return excps;
		}


		private void AddException(List<ExceptionDetails> list, ExceptionDetails details)
		{
			bool add = true;
			foreach (ExceptionDetails existing in list)
			{
				if (existing.stacktrace.Equals(details.stacktrace))
				{
					existing.seen += 1;
					add = false;
				}
			}

			if (add)
			{
				list.Add(details);
				details.seen = 1;
			}

		}

		public override void OnConfigReload()
		{
		}

		internal class ExceptionDetails
		{
			public string id = string.Empty;
			public string stacktrace = string.Empty;
			public int seen = 0;
			public List<string> tags = new List<string>();
		}
	}
}
