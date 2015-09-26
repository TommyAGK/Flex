using System;
using System.IO;
using System.Xml;
using UTB_voicespeaker.RFid;
using UTB_voicespeaker.Synthesizer;

// ReSharper disable PossibleNullReferenceException

namespace UTB_voicespeaker.DataManager
	{

		internal class XmlDataReader
			{
				private string _path;
				public bool DiagXML;
				private Reader readerXML;
				public enum ConfigType
					{
						Students = 0,
						Teachers,
						Eng,
						Fin,
						Nor,
						Swe,
						Rus
					}

				private string[] _dataStrings;

				public enum LangType
				{
					Eng = 0,
					Fin,
					Nor,
					Rus,
					Swe,
					CustomProfile // 5
				}
				public LangType LangSelect { get; set; }

				public enum LangWord
				{
					DayStart	= 0,
					LunchStart	= 1,
					LunchEnd	= 2,
					DayEnd		= 3,
					WeekEnd		= 4,
					ErrorFlex	= 5,
					LunchComment = 6,
					SignOutComment =7,
					BufferTime,
					DoubleLogin,
					DoubleLunch,
					TooSoon,
					NoEntryThisHour,
					UnExpectedError9000,
					CustLang
				}

				private static int numOfxmlNodes = 15;
				private static int numOfLangs = 6;
				public string GetLang(LangType lang)
					{
						//get { return LangStrings[numOfLangs, numOfxmlNodes ] ;}
						
						return LangStrings[(int)lang,0]; //defaults to last index of selected language
					}

				public static string[] ConfStrings = new string[5];
				public static string[,] LangStrings = new string[numOfLangs,numOfxmlNodes]; // custom = [5,X]

				
				
	
				
				/// <summary>
				/// Constructor for language based XMLreading
				/// </summary>
				/// <param name="target"></param>
				/// <param name="reader"></param>
				/// <param name="diagnostic"></param>
				public XmlDataReader(Enum target, Reader reader,  bool diagnostic = false ) // language constructor
					{ //select what to run based on enum cast.
						DiagXML = diagnostic; //default diagnostic to false.
						readerXML = reader;
						switch ((ConfigType)target)
							{
							case ConfigType.Students:
							ReadConfig(ConfigType.Students);
								break;
							case ConfigType.Teachers:
							ReadConfig(ConfigType.Teachers);
								break;
							/*
							case ConfigType.Nor:
								ReadLanguage(ConfigType.Nor);
								break;
							case ConfigType.Fin:
								ReadLanguage(ConfigType.Fin);
								break;
							case ConfigType.Eng:
								ReadLanguage(ConfigType.Eng);
								break;
							case ConfigType.Rus:
								ReadLanguage(ConfigType.Rus);
								break;
							case ConfigType.Swe:
								ReadLanguage(ConfigType.Swe);
								break;
							*/
							}
					}

				public XmlDataReader( string[] inputData = null)
					{
						_dataStrings = inputData; // null or not null.. we set it.
						//ReadLanguage(LangSelect);
					}

				public void ProcessLanguage()
					{
						ReadLanguage(LangSelect);
					}

				private void ReadWords(ref XmlNodeList node, LangType lang, int XmlLangIndex)
				{
						try
							{
							LangStrings[(int)lang, (int)LangWord.CustLang] = node[XmlLangIndex].SelectSingleNode("Lang").InnerText;
							LangStrings[(int)lang, (int)LangWord.DayStart] = node[XmlLangIndex].SelectSingleNode("DayStart").InnerText;
							LangStrings[(int)lang, (int)LangWord.LunchStart] = node[XmlLangIndex].SelectSingleNode("LunchStart").InnerText;
							LangStrings[(int)lang, (int)LangWord.LunchEnd] = node[XmlLangIndex].SelectSingleNode("LunchEnd").InnerText;
							LangStrings[(int)lang, (int)LangWord.DayEnd] = node[XmlLangIndex].SelectSingleNode("DayEnd").InnerText;
							LangStrings[(int)lang, (int)LangWord.WeekEnd] = node[XmlLangIndex].SelectSingleNode("WeekEnd").InnerText;
							LangStrings[(int)lang, (int)LangWord.ErrorFlex] = node[XmlLangIndex].SelectSingleNode("ErrorFlex").InnerText;
							LangStrings[(int)lang, (int)LangWord.LunchComment] = node[XmlLangIndex].SelectSingleNode("LunchComment").InnerText;
							LangStrings[(int)lang, (int)LangWord.SignOutComment] = node[XmlLangIndex].SelectSingleNode("SignOutComment").InnerText;
							LangStrings[(int)lang, (int)LangWord.BufferTime] = node[XmlLangIndex].SelectSingleNode("BufferTime").InnerText;
							LangStrings[(int)lang, (int)LangWord.DoubleLogin] = node[XmlLangIndex].SelectSingleNode("DoubleLogin").InnerText;
							LangStrings[(int)lang, (int)LangWord.DoubleLunch] = node[XmlLangIndex].SelectSingleNode("DoubleLunch").InnerText;
							LangStrings[(int)lang, (int)LangWord.TooSoon] = node[XmlLangIndex].SelectSingleNode("TooSoon").InnerText;
							LangStrings[(int)lang, (int)LangWord.NoEntryThisHour] = node[XmlLangIndex].SelectSingleNode("NoEntryThisHour").InnerText;
							LangStrings[(int)lang, (int)LangWord.UnExpectedError9000] = node[XmlLangIndex].SelectSingleNode("UnExpectedError9000").InnerText;
						
							}
						catch (Exception)
							{
								Speaker sp = new Speaker();
								sp.Speak("Error Reading XML language file");
								
							}
				}

				// ReSharper disable once UnusedParameter.Local
				private void ReadLanguage(LangType lang)
							{
								// Set path to Language.xml
						if (lang.Equals(LangType.CustomProfile))
							{
								_path = Directory.GetCurrentDirectory() + String.Format(@"\Dependency\Profile\{0}.xml", _dataStrings[2]);
							}
						else
							{
								_path = Directory.GetCurrentDirectory() + @"\Dependency\Config\Language.xml";
							}

								// Create and load XML document
								XmlDocument xmldoc = new XmlDocument();
								xmldoc.Load(new StreamReader(_path));
								if (xmldoc.DocumentElement == null)
									return;

								// Find languages tags
								var nodeList = xmldoc.DocumentElement.SelectNodes("Language");
				
								// Populate lang words
								// TODO: nodeList[x] is hard coded for now. Should find current index for lang instead.
								switch (lang)
								{
									case LangType.Fin:
										ReadWords(ref nodeList, LangType.Fin, 1);
										break;
									case LangType.Nor:
										ReadWords(ref nodeList, LangType.Nor, 2);
										break;
									case LangType.Rus:
										ReadWords(ref nodeList, LangType.Rus, 3);
										break;
									case LangType.Swe:
										ReadWords(ref nodeList, LangType.Swe, 4);
										break;
									case LangType.CustomProfile:
										ReadWords(ref nodeList, LangType.CustomProfile, 0);
										break;
									case LangType.Eng:
									default:
										ReadWords(ref nodeList, LangType.Eng, 0);
										break;
								}

							}

				

				private void ReadConfig(Enum choice)
							{
									// read the XML file from                       v here
								_path = Directory.GetCurrentDirectory() + @"\Dependency\Config\Config.xml";
								XmlDocument xmldoc = new XmlDocument();
								xmldoc.Load(new StreamReader(_path));
								if (xmldoc.DocumentElement == null)
									{
										return;
									}
								var nodeList = xmldoc.DocumentElement.SelectNodes("Server"); // <- This gets the stuff related to servers, all of it


								//Console.WriteLine(nodeList[0].NextSibling.SelectSingleNode("table").InnerText);

								try
									{
										if (choice.Equals(ConfigType.Students)) // if you want to find a students RF data, do this
											{

												ConfStrings[0] = nodeList[0].SelectSingleNode("address").InnerText;
												ConfStrings[1] = nodeList[0].SelectSingleNode("sqldirectory").InnerText;
												ConfStrings[2] = nodeList[0].SelectSingleNode("table").InnerText;
												ConfStrings[3] = nodeList[0].SelectSingleNode("dbuser").InnerText;
												ConfStrings[4] = nodeList[0].SelectSingleNode("dbpass").InnerText;

											}
										else
											{
												// this is teacher data
												ConfStrings[0] = nodeList[0].NextSibling.SelectSingleNode("address").InnerText;
												ConfStrings[1] = nodeList[0].NextSibling.SelectSingleNode("sqldirectory").InnerText;
												ConfStrings[2] = nodeList[0].NextSibling.SelectSingleNode("table").InnerText;
												ConfStrings[3] = nodeList[0].NextSibling.SelectSingleNode("dbuser").InnerText;
												ConfStrings[4] = nodeList[0].NextSibling.SelectSingleNode("dbpass").InnerText;
											}
										if (DiagXML)
											{
												Speaker sp = new Speaker(Language.Norwegian);
												var str = "XML status: filtilgang og lesning OK";
												sp.Speak(str, 0);
											
											}
									}
								catch (Exception e)
									{
										Console.WriteLine(e); // you done fucked up.
									}
							}

				public string[] GetConfig()
							{
								return ConfStrings; // Returns the XML data array of strings.
							}

				public string[,] GetLangs()
				{
					return LangStrings; // Returns all words from XML lang.
				}

				public bool CustomProfileExist(string username)
					{
						return File.Exists(Directory.GetCurrentDirectory() + String.Format(@"\Dependency\Profile\{0}.xml",username)); 
					}

	}
}
