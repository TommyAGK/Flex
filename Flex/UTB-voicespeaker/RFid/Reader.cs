using System;
using System.IO;
using System.IO.Ports;
using System.Net.NetworkInformation;
using System.Timers;
using UTB_voicespeaker.Audio;
using UTB_voicespeaker.DataManager;
using UTB_voicespeaker.Synthesizer;

namespace UTB_voicespeaker.RFid
	{
		internal class Reader
			{
				// TODO: Threading.
				// TODO: Process ID
				// TODO: return Data

				private string[] _orgData;
				private string _comport;
				private string rfid;
				private string _lastComPort;
				public bool Diagnostic { get; private set; }
				public bool DiagDatabase { get; set; }
				public bool DiagXML { get; set; }
				private SerialPort serialPort;
				private Timer timerComPort = new Timer(20000); // 20 sec
				Speaker sp = new Speaker(Language.Norwegian);
				Logger LogThings = new Logger();


		public Reader()
					{
						//determine serialport the reader is connected too.
				//LogThings.Report(@"Reader.cs",@"Constructed object and starting timer/event");
				timerComPort.Enabled = true;
				timerComPort.Elapsed += OnTimedEvent;
				timerComPort.AutoReset = true;

					}

				private void OnTimedEvent(Object source, ElapsedEventArgs e)
					{
						//LogThings.Report(@"Reader.cs", @"OnTimedEvent, calling DetectComport");
						DetectComPort();
						//LogThings.Report(@"Reader.cs", @"OnTimedEvent, after calling DetectComport");
					}

				public void DetectComPort()
					{
					//LogThings.Report(@"Reader.cs", @"DetectComPort, Begin");
					_lastComPort = _comport;
					//LogThings.Report(@"Reader.cs", @"DetectComPort, Getting active comports, _lastComPort comport: " + _lastComPort);
					var ports = SerialPort.GetPortNames();
					_comport = ports.Length > 1 ? ports[ports.Length - 1] : ports[0];
					//LogThings.Report(@"Reader.cs", @"DetectComPort, Determine the port for the device, ended up with _comport: " + _comport);
						if (_lastComPort == null)
							{
							//LogThings.Report(@"Reader.cs", @"DetectComPort, LastComport is null, this is first run");
							//LogThings.Report(@"Reader.cs", @"DetectComPort, END ");
								return;
							}
						if (_lastComPort != _comport && (_comport != "COM1" && ports.Length > 1))
							{
							//LogThings.Report(@"Reader.cs", @"DetectComPort, The comport was changed for some reason");
								try
									{
									//LogThings.Report(@"Reader.cs", @"DetectComPort, Comport Changed");
										serialPort.DataReceived -= DataRecievedHandler;
										var Error = "Noen byttet comport";
										sp.Speak(Error,-1);

									}
								catch (Exception e)
									{
									//LogThings.Report(@"Reader.cs", @"DetectComPort, Unable to detect comports");
										sp.Speak("Kunne ikke finne noen com porter.",0);
									}
								//LogThings.Report(@"Reader.cs", @"DetectComPort, Calling ConnectToRfidDevice");
								ConnectToRfidDevice();
							}
						//LogThings.Report(@"Reader.cs", @"DetectComPort, End");
					}



				/// <summary>
				/// This returns the ID from a RFID reader in string format. 
				/// 10 characters is to be expected
				/// </summary>
				/// <returns>Numeric String</returns>
				public string[] GetData()
					{
					//LogThings.Report(@"Reader.cs", @"GetData, Got Called");
						DetectComPort();
						ConnectToRfidDevice();
						//LogThings.Report(@"Reader.cs", @"GetData, Returning string");
						return _orgData;
					}

				/// <summary>
				/// Same as GetData, except only called if something fucked up.
				/// </summary>
				/// <returns></returns>
				public string GetBackup()
					{
					//LogThings.Report(@"Reader.cs", @"GetBackup, Begin");

						if (rfid == null)
							{
							//LogThings.Report(@"Reader.cs", @"GetBackup, RFID is null, calling CconnectToRfidDevice");
								
								ConnectToRfidDevice();
							}
						//LogThings.Report(@"Reader.cs", @"GetBackup, returning rfid");
						//LogThings.Report(@"Reader.cs", @"GetBackup, END");
						return rfid;
					}


				private void ConnectToRfidDevice()
					{
					//LogThings.Report(@"Reader.cs", @"ConnectToRfidDevice, Begin");
						
						//Initates the connection to the RF reader on the comport specified 	
						serialPort = new SerialPort(_comport);
						//LogThings.Report(@"Reader.cs", @"ConnectToRfidDevice, set " + _comport + " as source for serialport communication");
						try
							{
							if (!serialPort.IsOpen)
									{
									//LogThings.Report(@"Reader.cs", @"ConnectToRfidDevice, Serialport was not open, so opening it.");
										serialPort.Open();
										//LogThings.Report(@"Reader.cs", @"ConnectToRfidDevice, Serialport is now open");
									}
								//LogThings.Report(@"Reader.cs", @"ConnectToRfidDevice, Starting eventhandler for serialport");
								serialPort.DataReceived += DataRecievedHandler;
							}
						catch (Exception)
							{
							//LogThings.Report(@"Reader.cs", @"ConnectToRfidDevice, EXCEPTION: something wrong with "+_comport+" could not connect, trying to detect comports again");
							PrintDiagnosticData(rfid,true);
								sp.Speak(String.Format("Feil, kunne ikke koble til på port {0}, prøver å lokalisere ny port", _comport),0);
								DetectComPort();
							}

					}

				public void CloseConnection()
					{
					//LogThings.Report(@"Reader.cs", @"CloseConnection, Begin");
						try
							{
								if (serialPort.IsOpen)
									{
									//LogThings.Report(@"Reader.cs", @"CloseConnection, Serialport is open");
										serialPort.Close();
										//LogThings.Report(@"Reader.cs", @"CloseConnection, Serialport is now closed");
									}
								//LogThings.Report(@"Reader.cs", @"CloseConnection, END");
							}
						catch (Exception)
							{
							//LogThings.Report(@"Reader.cs", @"CloseConnection, FUCKED UP");
							// summat brok
							}
					}

				private void DataRecievedHandler(object sender, SerialDataReceivedEventArgs e)
					{
					//LogThings.Report(@"Reader.cs", @"DataRecievedHandler, Begin");
						// returns the ID read from tag, provides easteregg.
					//LogThings.Report(@"Reader.cs", @"DataRecievedHandler, setting local vars, serialport and databasemanager");
						var serp = (SerialPort) sender;
						var dbm = new DatabaseManager();
						var DiagnosticID = "4510720867";
						var BlueOysterID = "4475412741"; // 4475412741 tommy oyster// 4519992145 - old

						rfid = serp.ReadExisting().Trim();
						//LogThings.Report(@"Reader.cs", @"DataRecievedHandler, Detected RFID: " + rfid);
						//Console.WriteLine(rfid);
						if (rfid == DiagnosticID)
							{
							//LogThings.Report(@"Reader.cs", @"DataRecievedHandler, Diagnostic RFID detected");
								// Play blue oyster soundtrack.
								//Console.WriteLine("Dun duu du du duuu duu duuu");
								Diagnostic = true;
								if (Diagnostic)
									{
									//LogThings.Report(@"Reader.cs", @"DataRecievedHandler, Calling PrintDiagnosticData");
										PrintDiagnosticData(DiagnosticID);
									}
							}
                            else if (rfid == BlueOysterID)
                            {
							//LogThings.Report(@"Reader.cs", @"DataRecievedHandler, BlueOyster...");
                                AudioPlayer ap = new AudioPlayer("Blue Oyster Bar.wav");
                                ap.Play();
                            }
						else
							{
							//LogThings.Report(@"Reader.cs", @"DataRecievedHandler, Populating field string array with databasemanager returned result");
								_orgData = dbm.GenerateData(rfid, this); //The data giveth the data taketh
							//LogThings.Report(@"Reader.cs", @"DataRecievedHandler, END");
							}
					}

				private void PrintDiagnosticData(string rfid, bool localerror = false)
					{
						var port = _comport;
						var networkAccess = "Status på nettverk: Tilkobling mislyktes";
						if (NetworkInterface.GetIsNetworkAvailable())
							{
								networkAccess = "Status på nettverk: Tilkobling OK";
							}
						
						string str;
						if (localerror)
							{
								str = String.Format("Flex diagnostikk. finner ikke enhet ved {0}", port);
								sp.Speak(str, 0);
							}
						else
							{
								str = String.Format("Flex diagnostikk. Enhet koblet til ved {0}, {1}", port, networkAccess);
								sp.Speak(str, 0);
								var dataReader = new XmlDataReader(XmlDataReader.ConfigType.Students, this, Diagnostic);
								_orgData = new DatabaseManager().GenerateData(rfid, this, Diagnostic);
								Diagnostic = false;
							}
						//var str = String.Format("Flex diagnostic Data. Device at port {0}, access to network {1}", port,networkAccess);
					}
			}
	}
