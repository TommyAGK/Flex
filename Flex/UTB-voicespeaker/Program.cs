using System;
using UTB_voicespeaker.DataManager;
using UTB_voicespeaker.RFid;

namespace UTB_voicespeaker
{
	internal class Program
			{
				public static string[] Data { get; private set; }
				static readonly Reader RdReader = new Reader();
				static readonly DatabaseManager Mngr = new DatabaseManager();
				public static XmlDataReader DataReader;
				public static XmlDataReader DataReaderLang;
				public static string[] Configure;
				public static string[,] Langs;// = new string[1,1];
				// Bool's for app wide access
				public static bool EasterEgg;
				public static bool Testing;
				public static bool Logging;
				public static bool InvalidValidTag;

				public static bool IsLunch; //tidsvariabel
				public static bool WentHome; //tid / db
				public static bool HasLoggedIn; // dbsjekk
				public static bool isLoggedIn; // ^
				public static bool CustomProfile; //xml ting
				public static Logger LogThings = new Logger();
				// ReSharper disable once UnusedParameter.Local
			private static void Main(string[] args)
				{

					LogThings.PurgeDebugLog(true);
					//Console.CursorVisible = false;
					LogThings.Report(@"Program.cs", @"Program started");
					if (args.Length != 0)
						{
							if (args[0].ToUpper().Equals(@"-EGG"))
								{
									EasterEgg = true;
								}
							if (args[0].ToUpper().Equals(@"-TESTING"))
								{
									Testing = true;
									Logging = true;
								}
							if (args[0].ToUpper().Equals(@"-HELP"))
								{
									Console.Clear();
									DrawHelperDragon();
									return;
								}
							if (args[0].ToUpper().Equals("-LOGGING"))
								{
									Logging = true;
								}
							LogThings.Report(@"Program.cs", @"Called with argument: " + args[0]);
						}
					SelvSkryt();
					LogThings.Report(@"Program.cs", @"Starting DataReader");
					DataReader = new XmlDataReader(XmlDataReader.ConfigType.Students, RdReader);
					DataReaderLang = new XmlDataReader();
					LogThings.Report(@"Program.cs", @"Reading Languages from XML based on DataReader");
					Langs = DataReaderLang.GetLangs(); // Read all lang words.

					while (Configure == null)
						{
						LogThings.Report(@"Program.cs", @"Loading config in relation to databases");
							Configure = DataReader.GetConfig();
						}
					LogThings.Report(@"Program.cs", @"Main loop begins");
					while (true)
						{
							//RdReader.DetectComPort();
							if (RdReader.GetData() != null)
								{
									break;
								}

							Console.ForegroundColor = ConsoleColor.Black; // mehehehe
							// ReSharper disable once ConditionIsAlwaysTrueOrFalse
							if (Console.ReadKey().Key == ConsoleKey.Escape)
								{
								LogThings.Report(@"Program.cs", @"Escape captured, ending program");
									Console.ResetColor();
									return;
								}
						}
					LogThings.Report(@"Program.cs", @"Reading data from database");
					Data = Mngr.GetData();
				}

			private static void DrawHelperDragon()
				{
				Console.ForegroundColor = ConsoleColor.Cyan;
				string tabs = "\t";
				Console.WriteLine(Environment.NewLine + Environment.NewLine);
				Console.WriteLine(tabs + @"	               ,     \    /      ,");
				Console.WriteLine(tabs + @"                      / \    )\__/(     / \");
				Console.WriteLine(tabs + @"                     /   \  (_\  /_)   /   \");
				Console.WriteLine(tabs + @"                    /     \  \@  @/   /     \");
				Console.WriteLine(tabs + @"  ╔══════════════════════════|\../|══════════════════════════╗");
				Console.WriteLine(tabs + @"  ║                           \VV/                           ║");
				Console.WriteLine(tabs + @"  ║                          Percy                           ║");
				Console.WriteLine(tabs + @"  ║                      Helping dragon                      ║");
				Console.WriteLine(tabs + @"  ║ Flex 2.0.exe -testing <- Enables testmode,local database ║");
				Console.WriteLine(tabs + @"  ║ Flex 2.0.exe -egg <- Enables ... interesting things      ║");
				Console.WriteLine(tabs + @"  ║ Flex 2.0.exe -help <- Who you gonna call? HELP FUNCTION  ║");
				Console.WriteLine(tabs + @"  ║                   JUNE 2015 @ UTB Nord                   ║");
				Console.WriteLine(tabs + @"  ║                                                          ║");
				Console.WriteLine(tabs + @"  ╚══════════════════════════════════════════════════════════╝");
				Console.WriteLine(tabs + @"                |    /\ /      \\       \ /\    |");
				Console.WriteLine(tabs + @"                |  /   V        ))       V   \  |");
				Console.WriteLine(tabs + @"                |/     `       //        '     \|");
				Console.WriteLine(tabs + @"                `              V                '");
					while (true)
						{
						if (Console.ReadKey() != null)
							{
								return;
							}
						}
				}

			private static void SelvSkryt()
				{
					Console.CursorVisible = false;
					Console.ForegroundColor = ConsoleColor.Cyan;
					
					Console.Title = "Flex 2.0 by Kim & Tommy";
					string tabs = "\t";
					Console.WriteLine(Environment.NewLine + Environment.NewLine);
					Console.WriteLine(tabs + @"	               ,     \    /      ,");
					Console.WriteLine(tabs + @"                      / \    )\__/(     / \");
					Console.WriteLine(tabs + @"                     /   \  (_\  /_)   /   \");
					Console.WriteLine(tabs + @"                    /     \  \@  @/   /     \");
					Console.WriteLine(tabs + @"  ╔══════════════════════════|\../|══════════════════════════╗");
					Console.WriteLine(tabs + @"  ║                           \VV/                           ║");
					Console.WriteLine(tabs + @"  ║                                                          ║");
					Console.WriteLine(tabs + @"  ║                  FLEX 2.0 - Written by                   ║");
					Console.WriteLine(tabs + @"  ║    Kim Einar Larsen & Tommy Asmund Gunnar Kristiansen    ║");
					Console.WriteLine(tabs + @"  ║                                                          ║");
					Console.WriteLine(tabs + @"  ║                   JUNE 2015 @ UTB Nord                   ║");
					Console.WriteLine(tabs + @"  ║                                                          ║");
					Console.WriteLine(tabs + @"  ║                                                          ║");
					Console.WriteLine(tabs + @"  ╚══════════════════════════════════════════════════════════╝");
					Console.WriteLine(tabs + @"                |    /\ /      \\       \ /\    |");
					Console.WriteLine(tabs + @"                |  /   V        ))       V   \  |");
					Console.WriteLine(tabs + @"                |/     `       //        '     \|");
					Console.WriteLine(tabs + @"                `              V                '");
					if (EasterEgg)
						{
							Mario();
						}



				}

			private static void Mario()
				{
				Console.Beep(659, 250);
				Console.Beep(659, 250);
				Console.Beep(659, 300);
				Console.Beep(523, 250);
				Console.Beep(659, 250);
				Console.Beep(784, 300);
				Console.Beep(392, 300);
				Console.Beep(523, 275);
				Console.Beep(392, 275);
				Console.Beep(330, 275);
				Console.Beep(440, 250);
				Console.Beep(494, 250);
				Console.Beep(466, 275);
				Console.Beep(440, 275);
				Console.Beep(392, 275);
				Console.Beep(659, 250);
				Console.Beep(784, 250);
				Console.Beep(880, 275);
				Console.Beep(698, 275);
				Console.Beep(784, 225);
				Console.Beep(659, 250);
				Console.Beep(523, 250);
				Console.Beep(587, 225);
				Console.Beep(494, 225);
				}
			}
	}
