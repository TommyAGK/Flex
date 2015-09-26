using System;
using System.IO;

namespace UTB_voicespeaker
	{
	public class Logger
		{
			private static string _filePath;
			private DateTime time = DateTime.Now;
			

			public Logger()
				{
					_filePath = AppDomain.CurrentDomain.BaseDirectory + @"\debugtext.txt";
				}
			public  void Report(string className, string data)
				{
					if (! Program.Logging)
						{
							return;
						}
					var timeStamp = @"[" + time + @"] ";
					File.AppendAllText(_filePath, timeStamp + className +"\t\t" +  data + Environment.NewLine);
				}

			public static void Report( string className ,string data, string irrelevantfluff = null)
				{
					if (! Program.Logging)
						{
							return;
						}
					var time = DateTime.Now;
					var timeStamp = @"[" + time + @"] ";
					File.AppendAllText(_filePath, timeStamp + className + "\t\t" + data + Environment.NewLine);
				}

			public void PurgeDebugLog(bool AreYouEvenSure = false)
				{
					if (! Program.Logging)
						{
							return;
						}
					if (! AreYouEvenSure)
						{
							return;
						}
					if (File.Exists(_filePath))
						{
							File.Delete(_filePath);
						}
				}
		}
		
	}
