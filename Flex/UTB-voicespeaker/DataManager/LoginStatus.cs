using System;
using System.ComponentModel;
using UTB_voicespeaker.Synthesizer;

namespace UTB_voicespeaker.DataManager
	{
		internal class LoginStatus
			{
				public enum Status
					{
						[Description("Inn for dagen")] Login = 1, // inn for dagen
						[Description("Ut til lunsj")] ToLunch, // ut til lunsj
						[Description("Tilbake fra lunsj")] FromLunch, // tilbake fra lunsj
						[Description("Utlogging")] GoHome, // ...
					}
				public enum ErrorTypes
					{
						DoubleLogin,
						BufferTime, // tried to log out between 10 and lunch.
						UnExpectedError9000,
						TooSoon,
						NoEntryThisHour,
						DoubleLunch
					}
				private string[] _result;
				private string CustomLanguage = null;
				private DateTime _currenTime;
				private TimeSpan LunchTimeStart = new TimeSpan(10, 30, 0); //10:30
				private TimeSpan LunchTimeStop = new TimeSpan(15, 0, 0); // 15:00
				private TimeSpan BrunchEndFriday = new TimeSpan(11, 30 ,0); // 11:30 
			
				private TimeSpan FridayHome = new TimeSpan(13, 0, 0); // KLOKKA ETT

				private TimeSpan GoHomeTime = new TimeSpan(14, 30, 0, 0); // 15:30
				private TimeSpan LoginWindowMorning = new TimeSpan(10, 0, 0); // 10:00
				private TimeSpan BufferTime = new TimeSpan(10,29,00);


				//private XmlDataReader.LangType curLang;
						XmlDataReader.LangWord message = XmlDataReader.LangWord.DayEnd;
				private XmlDataReader dr;


				private SpeechMacroProcessor smp = new SpeechMacroProcessor();
						DatabaseManager dbManager = new DatabaseManager();
				

				Logger LogThings = new Logger();





				public bool TimeBetween(DateTime datetime, TimeSpan start, TimeSpan end)
					{
						// convert datetime to a TimeSpan
						TimeSpan now = datetime.TimeOfDay;
						// see if start comes before end
						if (start < end)
							{
								return start <= now && now <= end;
							}
						// start is after end, so do the inverse comparison
						return ! (end < now && now < start);
					}

				public LoginStatus(string[] input)
					{
					//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, Begin");
						_result = input;
						dr = new XmlDataReader(_result);
						//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, Created reader for XML files, based on _result string array");
						if (Program.Testing)
							{
							//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, app testing detected, setting datetime to manual time");
								_currenTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 12, 0, 00); // DEEERTY HACK
								//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, time = " + _currenTime);
							}
						else
							{
							//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, Setting _current time to ... current time");
								_currenTime = DateTime.Now;
								//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, current time = " + _currenTime);
							}
						//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, Calling DetermineLoginstatus");
						DetermineLoginStatus();
						//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, Starting time logic");
						if (_currenTime.DayOfWeek == DayOfWeek.Saturday || _currenTime.DayOfWeek == DayOfWeek.Sunday)
							{
								VoiceOutError(ErrorTypes.NoEntryThisHour);
							}
						if (_currenTime.DayOfWeek == DayOfWeek.Friday && TimeBetween(_currenTime, BrunchEndFriday, FridayHome))
							{ // this is to make sure you can leave early on fridays, no matter what you did before
								if (Program.isLoggedIn)
									{
										LogOutUser(Status.GoHome);
										return;
									}
								if (Program.IsLunch)
									{
										LogInUser(Status.FromLunch);
										//if (_currenTime.Hour >= FridayHome.Hours)
										//	{
										//		LogOutUser(Status.GoHome);
												
										//	}
										//TODO: make sure this works
										return;
									}
											// this gets done ONLY if you were not at lunch or logged in.
										VoiceOutError(ErrorTypes.NoEntryThisHour);
										return;
									

							}
						if (Program.isLoggedIn)
							{
							//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, Person is logged in");
								if (TimeBetween(_currenTime, LunchTimeStart, LunchTimeStop))
									// if current time is between start and stop of lunchtime
									{
										//to lunch
										// bool is on lunch?
										if (!dbManager.BeenToLunchToday(_result))
											{

											//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, Person went out to lunch");
												LogOutUser(Status.ToLunch);
											}
										else {
										//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, unexpected but still expected error...9000");
											VoiceOutError(ErrorTypes.DoubleLunch);
										}
									
										
										
									}
								else if (_currenTime.Hour >= LunchTimeStop.Hours) //Etter kl 15:00
									{
										// go home
									//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, Person went home");
										LogOutUser(Status.GoHome);
										
									}
								else if (_currenTime.Hour < LoginWindowMorning.Hours) // If logged in and attempts 2nd login before 10:00
									{
											//NO, PLS STAHP
									//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, detected double login");
										VoiceOutError(ErrorTypes.DoubleLogin); // This works
									}
									else if (TimeBetween(_currenTime,LoginWindowMorning, BufferTime))
										{
											// mellom 10 og 10:29.
										//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, detected buffered time");
											VoiceOutError(ErrorTypes.BufferTime);  // this works

										}
								else
									{
										//this is before 10:30, cannot log out, talk to teach.
										//You only end up here if pigs fly
									//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, this unexpected error is also 9000");
										VoiceOutError(ErrorTypes.UnExpectedError9000);
										//Console.WriteLine(_currenTime);


									}
							}
						else
							{
							//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, Person is NOT logged in");
								if (_currenTime.Hour < LoginWindowMorning.Hours) // før kl 10
									{
										//Console.WriteLine("Before 10 in the morning.");
									//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, Person has logged in now");
										LogInUser(Status.Login);  // works
										
									}
								else if (Program.IsLunch && (TimeBetween(_currenTime, LunchTimeStart, LunchTimeStop))) // 10:30 -> 15:00 & boolean IsLunch = true
									{
									// if you are at lunch, and are within the timescope of lunch then true
									//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, Person returned from lunch");
										LogInUser(Status.FromLunch);
										
									}
								else if (TimeBetween(_currenTime, LoginWindowMorning, BufferTime))
									{
										// Attempting to log in after 10, but before lunch. NO CAN DO, goto Teacher
									//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, Person was too hungry to wait, has to wait more");
										VoiceOutError(ErrorTypes.TooSoon);
									}
								else // if not before 10, or not on lunch. This means you are here after school hours or 
									{
									//LogThings.Report(@"LoginStatus.cs", @"LoginStatus, Person overslept, cannot log in past deadline");
										VoiceOutError(ErrorTypes.NoEntryThisHour);
										 //cannot log in after school hours, use web
									}
							}
					}

#region DataManagement of stuff

				private void VoiceOutError(ErrorTypes error)
					{
					//LogThings.Report(@"LoginStatus.cs", @"VoiceOutError, begin");
					 //prater engelsk uansett
						//Console.WriteLine("Y U NO MEK THING GOOD? {0} !!!", error.ToString()); //TODO: Prio 5
					//LogThings.Report(@"LoginStatus.cs", @"VoiceOutError, Select country called");
						
						
						CountrySelector();
						if (error.Equals(ErrorTypes.BufferTime))
							{
								message = XmlDataReader.LangWord.BufferTime;
							    _result[4] = Program.Langs[(int)dr.LangSelect, (int)XmlDataReader.LangWord.BufferTime];
							}

						else if (error.Equals(ErrorTypes.DoubleLogin)) { message = XmlDataReader.LangWord.DoubleLogin; }
						else if (error.Equals(ErrorTypes.DoubleLunch)) { message = XmlDataReader.LangWord.DoubleLunch; }
						else if (error.Equals(ErrorTypes.NoEntryThisHour))
							{
								message = XmlDataReader.LangWord.NoEntryThisHour;
								if (Program.IsLunch)
									{
										//Console.WriteLine("Logged in from lunch");
										LogInUser(Status.FromLunch);
										Program.IsLunch = false;
										return;
										//LogOutUser(Status.GoHome);
									}
							}
						else if (error.Equals(ErrorTypes.UnExpectedError9000)) { message = XmlDataReader.LangWord.UnExpectedError9000;  }
						else if (error.Equals(ErrorTypes.TooSoon)) { message = XmlDataReader.LangWord.TooSoon; }

                        if (CustomLanguage != null)
                        {
						smp.Process((Language)dr.LangSelect, Program.Langs[(int)dr.LangSelect, (int)message].Replace("@user", _result[7]), 0, CustomLanguage);
                        }
                        else
                        {
						smp.Process((Language)dr.LangSelect, Program.Langs[(int)dr.LangSelect, (int)message].Replace("@user", _result[7]), 0);
                        }
						//LogThings.Report(@"LoginStatus.cs", @"VoiceOutError, END");
					}

				private void LogOutUser(Status status)
					{
					//LogThings.Report(@"LoginStatus.cs", @"LogOutUser, begin");
						_result[3] = "Ut";
						//LogThings.Report(@"LoginStatus.cs", @"LogOutUser, called country selector");
						CountrySelector();
						
						if (status.Equals(Status.ToLunch)) {
						  // if been to lunch say "You have already been to lunch"
						
						 message = XmlDataReader.LangWord.LunchStart; 
						_result[4] = Program.Langs[(int)dr.LangSelect, (int)XmlDataReader.LangWord.LunchComment]; Program.IsLunch = true; Program.isLoggedIn = false;
						
						}
						
						else if (status.Equals(Status.GoHome)) {
							if (_currenTime.DayOfWeek == DayOfWeek.Friday) { message = XmlDataReader.LangWord.WeekEnd;}
								else { message = XmlDataReader.LangWord.DayEnd; }
							Program.isLoggedIn = false;
							_result[4] = Program.Langs[(int)dr.LangSelect, (int)XmlDataReader.LangWord.SignOutComment]; }


						
				
					smp.Process((Language)dr.LangSelect, Program.Langs[(int)dr.LangSelect, (int)message].Replace("@user", _result[7]), 0, CustomLanguage);
					//LogThings.Report(@"LoginStatus.cs", @"LogOutUser, write data to database");
					dbManager.WriteData(_result);
					//LogThings.Report(@"LoginStatus.cs", @"LogOutUser, END");
					}

				private void LogInUser(Status status)
					{
					//LogThings.Report(@"LoginStatus.cs", @"LogInUser, begin");
						Program.WentHome = false;
						Program.IsLunch = false;
						//LogThings.Report(@"LoginStatus.cs", @"LogInUser, Set bools to match logged in");
						//Console.WriteLine("Logging in user.{0}", status.ToString()); //TODO: Prio 1
						
						_result[3] = "In";
						_result[4] = null;
						//LogThings.Report(@"LoginStatus.cs", @"LogInUser, set _result vars to match logged in");
						//LogThings.Report(@"LoginStatus.cs", @"LogInUser, country selector called");
						CountrySelector();
						//LogThings.Report(@"LoginStatus.cs", @"LogInUser, determine if day start or lunch");
						if (status.Equals(Status.FromLunch)) { message = XmlDataReader.LangWord.LunchEnd; }//LogThings.Report(@"LoginStatus.cs", @"LogInUser, was lunch"); }
						else if (status.Equals(Status.Login))
							{
							//LogThings.Report(@"LoginStatus.cs", @"LogInUser, was day start");
								message = XmlDataReader.LangWord.DayStart;
							}

						smp.Process((Language)dr.LangSelect, Program.Langs[(int)dr.LangSelect, (int)message].Replace("@user", _result[7]), 0);
						Program.isLoggedIn = true;
						//LogThings.Report(@"LoginStatus.cs", @"LogInUser, writing data to database");
						dbManager.WriteData(_result);
						//LogThings.Report(@"LoginStatus.cs", @"LogInUser, END");
					}
			
				private void CountrySelector()
					{
					//LogThings.Report(@"LoginStatus.cs", @"CountrySelector, begin");
					if (dr.CustomProfileExist(_result[2]))
							{
							//LogThings.Report(@"LoginStatus.cs", @"CountrySelector, Person has custom profile");
								Program.CustomProfile = true;
								//LogThings.Report(@"LoginStatus.cs", @"CountrySelector, Setting language to custom profile");
								dr.LangSelect = XmlDataReader.LangType.CustomProfile;
								//LogThings.Report(@"LoginStatus.cs", @"CountrySelector, Calling process language");
								dr.ProcessLanguage();

								//CustomLanguage = _result[0]; // works, but defaults to database lang
								//LogThings.Report(@"LoginStatus.cs", @"CountrySelector, setting custom language to whatever the process returned");
								CustomLanguage = dr.GetLang(XmlDataReader.LangType.CustomProfile);
								//LogThings.Report(@"LoginStatus.cs", @"CountrySelector, setting custom language to data in xml file");
								CustomLanguage = CustomLanguage.Substring(0,3).ToUpper();
								//LogThings.Report(@"LoginStatus.cs", @"CountrySelector, END CUSTOM");
							}
						else
							{
							//LogThings.Report(@"LoginStatus.cs", @"CountrySelector, default profile, selecting language based on SQL data");
							if (_result[0].ToUpper().Equals("NOR")) { dr.LangSelect = XmlDataReader.LangType.Nor; }
							else if (_result[0].ToUpper().Equals("SWE")) { dr.LangSelect = XmlDataReader.LangType.Swe; }
							else if (_result[0].ToUpper().Equals("FIN")) { dr.LangSelect = XmlDataReader.LangType.Fin; }
							else if (_result[0].ToUpper().Equals("RUS")) { dr.LangSelect = XmlDataReader.LangType.Rus; }
							else { dr.LangSelect = XmlDataReader.LangType.Eng; }
							dr.ProcessLanguage();
							//LogThings.Report(@"LoginStatus.cs", @"CountrySelector, processing language");
							//LogThings.Report(@"LoginStatus.cs", String.Format("Database lang: {0}, Enum lang: {1}",_result[0],dr.LangSelect));
							}
						//LogThings.Report(@"LoginStatus.cs", @"CountrySelector, END");
					}




#endregion
				private void DetermineLoginStatus()
					{
					//LogThings.Report(@"LoginStatus.cs", @"DetermineLoginStatus, Begin");
					//LogThings.Report(@"LoginStatus.cs", @"DetermineLoginStatus, calling resolveComments");
						if (_result[1] == null)
							{
								return;
							}

						if (_result[4] != null)
							{
							ResolveComments(_result[4]); //get the comment field from database string array
								
							}
						if (_result[3] != null)
							{
							if (_result[3].Equals("In"))
								{
								//LogThings.Report(@"LoginStatus.cs", @"DetermineLoginStatus, Person has been determined as already logged in");
								Program.isLoggedIn = true;
								//LogThings.Report(@"LoginStatus.cs", @"DetermineLoginStatus, set bool Program.isLoggedIn = true");
									//Console.WriteLine("Set login to true");
								}
							}

						//LogThings.Report(@"LoginStatus.cs", @"DetermineLoginStatus, End");
					}

				private void ResolveComments(string s) // check string, set bools
					{
					
					//LogThings.Report(@"LoginStatus.cs", @"ResolveComments, begin");
						if (s.Equals("Lounas") | s.Equals("Lunch") | s.Equals("Lunsj") | s.Equals("обед"))
							{
							//LogThings.Report(@"LoginStatus.cs", @"ResolveComments, some sort of lunch word detected");
								Program.IsLunch = true;
								Program.isLoggedIn = false;
								//LogThings.Report(@"LoginStatus.cs", @"ResolveComments, Program.islunch = true, and Program.isloggedin = false");
							}
						else if (s.Equals("Lähtenyt tältäpäivältä") | s.Equals("Gått för dagen") | s.Equals("Gått for dagen") | s.Equals("Прошли за день"))
							{
							//LogThings.Report(@"LoginStatus.cs", @"ResolveComments, left for the day detected");
								Program.WentHome = true;
								Program.isLoggedIn = false;
								//LogThings.Report(@"LoginStatus.cs", @"ResolveComments, Program.isloggedin = false, Program.WentHome = true");
							}
						//LogThings.Report(@"LoginStatus.cs", @"ResolveComments, END");
					}
			}
	}
