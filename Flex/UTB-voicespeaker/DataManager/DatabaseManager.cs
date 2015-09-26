

using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using UTB_voicespeaker.RFid;
using UTB_voicespeaker.Synthesizer;

namespace UTB_voicespeaker.DataManager
	{
	class DatabaseManager
		{

		
			private string _query;
			private string _rfid;
			private SqlConnection _cnn;
			private static int _arrsize = 8;
			private string[] _result = new string[_arrsize];
			public XmlDataReader.ConfigType Target { get; internal set; }
			private bool _diagnostic;
			private bool tinyBool = false;
			private Reader _reader;
			Speaker sp = new Speaker(Language.Norwegian);
			Logger LogThings = new Logger();


			/// <summary>
			/// Generate data based on RFID 
			/// </summary>
			/// <param name="input">This is the RFID!</param>
			/// <returns></returns>
			public string[] GenerateData(string input, Reader reader, bool diag = false)
				{
					LogThings.Report(@"DatabaseManager.cs", @"GenerateData, Begin");
					_reader = reader;
					_diagnostic = diag;
					_rfid = input; // store RFID
					LogThings.Report(@"DatabaseManager.cs", @"GenerateData, Vars set, calling DataCon with RFID passed as string");
					DataCon(input);
					LogThings.Report(@"DatabaseManager.cs", @"GenerateData, Calling ConnectToDatabase");
					ConnectToDataBase();
					LogThings.Report(@"DatabaseManager.cs", @"GenerateData, Returning Result string array");
					LogThings.Report(@"DatabaseManager.cs", @"GenerateData, END");
					//_result[7] = SanitizeName(_result[7]);
					return _result; // this contains most recent result
				}

			/// <summary>
			/// Returns the data, and if you have not created it yet, it will.
			/// </summary>
			/// <returns></returns>
			public string[] GetData()
				{
					if (_result != null)
						{
							return _result;
						}
					var backup = new Reader();
					return GenerateData(backup.GetBackup(),backup);
				}

			public void FeedBack(string message, int speed = 0)
				{
					Speaker spkr = new Speaker(Language.Norwegian);
					spkr.Speak(message, speed);
				}

			public void WriteData(string[] input)
				{
					_result = input;
					DataCon(null, false);
					ConnectToDataBase(true); //write data
				}

			private void DataCon(string rfid, bool read = true) // connect to your database
				{
				LogThings.Report(@"DatabaseManager.cs", @"DataCon, Begin");
					string connetionString = "";
					if (rfid == "")
						{
						LogThings.Report(@"DatabaseManager.cs", @"DataCon, RFID was null, exiting loop (end)");
							return;
						}
					var config = Program.Configure;
					LogThings.Report(@"DatabaseManager.cs", @"DataCon, Loading xml configure by calling for main program var Configure");
					
					if (config != null)
						{
							if (Program.Testing)
								{
								LogThings.Report(@"DatabaseManager.cs", @"DataCon, System is in Testing mode, using hardcoded database connection string");
								 //Currently "broken" on purpose for proof of concept testing system.
								 //TODO swap commented strings
									connetionString = String.Format("Data Source={0};Initial Catalog={1}; Integrated security = true;", "Kingen" + @"\sql2014", "Students");
									//connetionString = String.Format("Data Source={0};Initial Catalog={1}; Integrated security = true;",config[0] + @"\sql2014", config[2]);
									//connetionString = String.Format("Data Source={0};Initial Catalog={1}; Integrated security = false;", config[0] + @"\sql2014", config[2]);
									
								}
							else
								{
								LogThings.Report(@"DatabaseManager.cs", @"DataCon, Parsing database connectionstring based on XML source");
								connetionString = String.Format("Data Source={0}\\{1};Initial Catalog={2}; User Id= {3}; Password={4};",
									config[0], config[1], config[2], config[3], config[4]);
								//LogThings.Report(@"DatabaseManager.cs", @"DataCon, connectionstring = " + connetionString);
								}
						}
					else
						{
						LogThings.Report(@"DatabaseManager.cs", @"DataCon, fucked up, voicing error");
							sp.Speak("Please check your XML configuration, could not locate correct path to database server.", 0);
						}
					LogThings.Report(@"DatabaseManager.cs", @"DataCon, creating connection object with connection string");
					_cnn = new SqlConnection(connetionString);
					if (read)
						{
						LogThings.Report(@"DatabaseManager.cs", @"DataCon, Detected READ request on bool, setting query to read data");
						_query ="SELECT TOP 1 land, rfid, tblPersoner.UserNamn, tidKort.aktivitet, tidKort.kommentar, tidKort.Datum, tidKort.tidStempel, tblPersoner.Forname " +
                                "FROM [tblPersoner] join [tidKort] on tblPersoner.UserNamn = tidKort.UserNamn WHERE (rfid = '" + rfid 
								+ "' and tidKort.Datum = Convert(varchar(10),GETDATE(),121)  ) order by ID DESC";
						LogThings.Report(@"DatabaseManager.cs", @"DataCon, Query : " + _query);
						}
					else if (! read)
						{
						LogThings.Report(@"DatabaseManager.cs", @"DataCon, Detected WRITE request on bool");		
							var inputData = DateTime.Now; 
							if (_result[4] == null)
								{
								LogThings.Report(@"DatabaseManager.cs", @"DataCon, Write SQL query created for status table because previous result had no 'comment' in it.");
									_query = String.Format(
										"Insert into tidKort (UserNamn, Datum, tidStempel, aktivitet) " + " Values('{0}','{1}','{2}','{3}');" + 
										" Update tblStatus set startDatum = '{1}', inne = '{4}', Lunch = '{5}', Sjuk = 'False' where UserNamn = '{0}' " ,
										_result[2],inputData.ToString("d", CultureInfo.CreateSpecificCulture("sv-SE")), inputData.ToString("HH:mm"), _result[3],Program.isLoggedIn, Program.IsLunch);
									//LogThings.Report(@"DatabaseManager.cs", @"DataCon, Query: " + _query);
										// reset _ALL_ the bools, because reasons
									Program.IsLunch = false;
									Program.isLoggedIn = false;
									Program.HasLoggedIn = false;
									Program.WentHome = false;
									LogThings.Report(@"DatabaseManager.cs", @"DataCon, Set IsLunch, IsLoggedIn, HasLoggedIn and WentHome to false");
								}
							else
								{
								LogThings.Report(@"DatabaseManager.cs", @"DataCon, Write SQL query created for either leaving or lunch");
								_query =String.Format("Insert into tidKort (UserNamn, Datum, tidStempel, aktivitet, kommentar)" +
										" Values ('{0}','{1}','{2}','{3}','{4}'); Update tblStatus set startDatum = '{1}', inne = '{5}', Lunch = '{6}', Sjuk = 'False' where UserNamn = '{0}' ", _result[2], inputData.ToString("d", CultureInfo.CreateSpecificCulture("sv-SE")), inputData.ToString("HH:mm"), _result[3], _result[4], Program.isLoggedIn, Program.IsLunch);
								LogThings.Report(@"DatabaseManager.cs", @"DataCon, Query: " + _query);
									Program.IsLunch = false;
									Program.isLoggedIn = false;
									Program.HasLoggedIn = false;
									Program.WentHome = false;
									LogThings.Report(@"DatabaseManager.cs", @"DataCon, Bool reset again");
								}
						}
					LogThings.Report(@"DatabaseManager.cs", @"DataCon, END");
                }

			private void ConnectToDataBase(bool write = false)
				{
					LogThings.Report(@"DatabaseManager.cs", @"ConnectToDataBase, Begin, write flag set to " + write);

					if (_rfid == "")
						{
						LogThings.Report(@"DatabaseManager.cs", @"ConnectToDataBase, Storage was empty, returning (end)");
							return;
						}
					try
						{
							if (_diagnostic)
								{
								LogThings.Report(@"DatabaseManager.cs", @"ConnectToDataBase, Diagnostic detected");
									try
										{
											_cnn.Open();
											var str = "Database status: Tilkobling OK";
											FeedBack(str, 0);
											_cnn.Close();
										}
									catch (Exception)
										{
											var str = "Database status: Tilkobling feilet";
											FeedBack(str, 0);
										}
								}
							else
								{
								LogThings.Report(@"DatabaseManager.cs", @"ConnectToDataBase, Attempting to open database connection");
									_cnn.Open();
								LogThings.Report(@"DatabaseManager.cs", @"ConnectToDataBase, _cnn.Open();");
									if (! write)
										{
										LogThings.Report(@"DatabaseManager.cs", @"ConnectToDataBase, Reading data");
											ReadData(_query);
											if ( _result[1] == null)
												{
													tinyBool = true;

												var query = String.Format("SELECT UserNamn, Land From tblPersoner where rfid = '{0}';", _rfid);
												//ny query

												ReadData(query);
												}
										}
									else
										{
										LogThings.Report(@"DatabaseManager.cs", @"ConnectToDataBase, Writing Data");
											InsertData(_query);
										}
									//Console.WriteLine("Is closed");
									LogThings.Report(@"DatabaseManager.cs", @"ConnectToDataBase, _cnn.close();");
									_cnn.Close();
									LogThings.Report(@"DatabaseManager.cs", @"ConnectToDataBase, close ok!");
								}
						}
					catch (SqlException)
						{
						LogThings.Report(@"DatabaseManager.cs", @"ConnectToDataBase, FUKED UP");
							FeedBack("Kan ikke koble til database, sjekk konfigurasjon");
						}
					LogThings.Report(@"DatabaseManager.cs", @"ConnectToDataBase, END");
				}

			private void InsertData(string sql)
				{
				
					try
						{
							using (var command = new SqlCommand(sql, _cnn))
								{
									command.ExecuteNonQuery();
								}
						}
					catch (Exception e)
						{
						LogThings.Report(@"DatabaseManager.cs", @"InsertData, EXCEPTION");
							FeedBack("Kunne ikke skrive data til database, noe gikk galt", 0);
						}
				}

			private void ReadData(string sql)
				{
					
				LogThings.Report(@"DatabaseManager.cs", @"ReadData, Begin");
					_result = new string[_arrsize];
					try
						{
							using (var command = new SqlCommand(sql, _cnn))
								{
								LogThings.Report(@"DatabaseManager.cs", @"ReadData, created sql command");
							
									using (var reader = command.ExecuteReader())
										{
										LogThings.Report(@"DatabaseManager.cs", @"ReadData, executed command as reader");
											Program.InvalidValidTag = false;
											if (!reader.HasRows) //validates as false
												{
													LogThings.Report(@"DatabaseManager.cs", @"ReadData, no rows recieved from SQL query");
													//sp = new Speaker(Language.English);
													//sp.Speak("ATTENTION! ERROR! This rfid tag is not registered, please contact your teacher");
													Program.InvalidValidTag = true;
													tinyBool = true;
													return;
												}
											if (_result == null)
												{
												LogThings.Report(@"DatabaseManager.cs", @"ReadData, _Result is null, returning");
													return;
												}
											while (reader.Read())
												{
												LogThings.Report(@"DatabaseManager.cs", @"ReadData, Writing data to _result array");
													if (! tinyBool)
														{
															_result[0] = reader[0].ToString(); // Land
															_result[1] = reader[1].ToString(); // rfid
															_result[2] = reader[2].ToString(); // Username
															_result[3] = reader[3].ToString(); // Aktivitet
															_result[4] = reader[4].ToString(); // Kommentar
															_result[5] = reader[5].ToString(); // Datum
															_result[6] = reader[6].ToString(); // tidstempel
															_result[7] = reader[7].ToString(); // Forname
															_result[7] = SanitizeName(_result[7]);
														}
													else
														{
															_result[2] = reader[0].ToString(); // username
															_result[0] = reader[1].ToString(); // land
															_result[1] = _rfid;
														}
													LogThings.Report(@"DatabaseManager.cs", @"ReadData, Calling loginstatus class, aka loff, passing _result as parameter");
													
													var lgStatus = new LoginStatus(_result );
													LogThings.Report(@"DatabaseManager.cs", @"ReadData, no longer reading");
												}
										}
								}
						}
					catch (ArgumentException)
						{
						LogThings.Report(@"DatabaseManager.cs", @"ReadData, FUKD");
							FeedBack("check your SQL query"); //unlilkely to happen, unless we swap database layout.
						}
					LogThings.Report(@"DatabaseManager.cs", @"ReadData, END");
				}

			private string SanitizeName(string input)
				{
					var pattern = @"[\-\w]+[a-zA-Zåäöøæ ÅÄÖØÆ]+( [aA-zZåäöøæ ÅÄÖØÆ]+)?"; // check for valid "name like strings"
					var rx = new Regex(pattern);
					var collection = rx.Matches(input); //the collection of names
					var wSpace = @"\s"; // check and count whitespaces
					rx = new Regex(wSpace);
					var spaceList = rx.Matches(collection[0].ToString());
					// check result for whitespaces and trim


					/*
					 * if whitespace #2 position >15
					 *		whitespace #1 < 15
					 *	
					 */
					


					//return collection[0].ToString().Substring(0, 49);
					if (spaceList.Count >1) //more than 1 space (2 lol++)
						{
							if (collection[0].ToString().Length > 15) // name is longer than 15 chars (including spaces)
								{
									var pos = spaceList[1].Index; //int pos of whitespace #2
									var bPos = spaceList[0].Index;
									var aoe = collection[0].ToString(); //local var of string object
									if (pos <= 15) // pos is less than our equal to max limit of 15
										{ 
											return aoe.Substring(0, pos); // return the substring based on whitespace location
										}
									if (bPos <= 15)
										{
											return aoe.Substring(0, bPos); // return the substring based on w
										}
									return collection[0].ToString().Substring(0,14); // return trunct because no whitespaces
								}
						} else if (spaceList.Count == 1)
							{
							if (collection[0].ToString().Length > 15) // name is longer than 15 chars (including spaces)
								{
								var pos = spaceList[0].Index; //int pos of whitespace #1
									if (pos <= 15)
										{
											return collection[0].ToString().Substring(0, pos);
										}
									return collection[0].ToString().Substring(0, 14); // return the substring based on whitespace location
								}
							}
						

					if (collection[0].Length > 15)
						{
							return collection[0].ToString().Substring(0, 14);
						}
					return collection[0].ToString();
				}

			public bool BeenToLunchToday(string[] input)
				{
				LogThings.Report(@"DatabaseManager.cs", @"BeenToLunchToday, Begin");
					DataCon(input[0]);
					var inputData = DateTime.Now;
					var sql =
						String.Format(
							"Select UserNamn, Datum, tidStempel, aktivitet, kommentar from tidKort where UserNamn = '{0}' and aktivitet = 'Ut' and Datum = '{1}' and kommentar like 'l%';",
							input[2], inputData.ToString("d", CultureInfo.CreateSpecificCulture("sv-SE")));
					LogThings.Report(@"DatabaseManager.cs", @"BeenToLunchToday, Lunch related SQL query : " + sql);
					try
						{
							using (var command = new SqlCommand(sql, _cnn))
								{
								LogThings.Report(@"DatabaseManager.cs", @"BeenToLunchToday, sql command created");
									try
										{
											_cnn.Open();
											LogThings.Report(@"DatabaseManager.cs", @"BeenToLunchToday, _cnn.open");
											using (var reader = command.ExecuteReader())
												{
												LogThings.Report(@"DatabaseManager.cs", @"BeenToLunchToday, executed command");
													if (!reader.HasRows)
														{
														LogThings.Report(@"DatabaseManager.cs", @"BeenToLunchToday, reader has no rows, returning false");
															return false;
														}
													LogThings.Report(@"DatabaseManager.cs", @"BeenToLunchToday, _cnn.close");
											_cnn.Close();
											LogThings.Report(@"DatabaseManager.cs", @"BeenToLunchToday, returning true");
													return true;
												}


										}
									catch (Exception)
										{
										LogThings.Report(@"DatabaseManager.cs", @"BeenToLunchToday, fukd");
											throw;
										}

								}
						}
					catch (Exception)
						{
						LogThings.Report(@"DatabaseManager.cs", @"BeenToLunchToday, SQL query fukd");
							return false;
							//FeedBack("check your SQL query"); //unlilkely to happen, unless we swap database layout.
						}
				}
		}
		

	}
