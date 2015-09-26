using System;
using Microsoft.Speech.Synthesis;

/*
	HOW TO USE THE SYNTHESIZER:

			Speaker sp = new Speaker(Language.English);
			//sp.AsyncMode = true;
			sp.Speak("You can use one of the voice names discovered with the above example as an argument to the SelectVoice(String) method to select a voice to use. The following example selects the US English voice.");
			
			//Speaker sp = new Speaker(Language.Finish);
			//sp.Speak("Eetu ja Ville esittelevät toimiston katu-uskottavaan tyyliin! Uusi Splay Office - jakso tulee joka perjantai aina toukokuun loppuun asti!");

			//Speaker sp = new Speaker(Language.Norwegian);
			//sp.Speak("Smarte byer er blitt et av de viktigste initiativene. Stadig mer av verdens befolkning bor i byer. Disse må forbedres både for innbyggernes skyld, men også for å redusere byenes ressursforbruk og forurensing.");

			//Speaker sp = new Speaker(Language.Russian);
			//sp.Speak("Этот сайт посвящен переводам классической русской поэзии на английский язык. Конечно, здесь же можно найти и оригиналы стихов, а также небольшое количество биографической информации о поэтах. Сайт только-только открылся, поэтому информации мало (однако планируются еженедельные обновления). Кроме того, возможны глюки.");

			//Speaker sp = new Speaker(Language.Swedish);
			//sp.Speak("I morgon arbetar jag i en annan stad. Jag susar dit genom morgontimman som är en stor svartblå cylinder. Orion hänger ovanför tjälen. Barn står i en tyst klunga och väntar på skolbussen, barn som ingen ber för. Ljuset växer sakta som vårt hår.");
			
			System.Console.WriteLine("Press any key to continue.");
			System.Console.ReadKey();

	HOW TO USE THE SSML:
			SOURCE 1: https://msdn.microsoft.com/en-us/library/office/bb813035%28v=office.12%29.aspx
			SOURCE 2: https://msdn.microsoft.com/en-us/library/office/hh361601%28v=office.14%29.aspx
			SOURCE 3: https://msdn.microsoft.com/en-us/library/hh378338%28v=office.14%29.aspx

			Speaker sp = new Speaker();
			sp.UseSSML = true;

			// Use the secondary weapon
			string str = "Use the " + sp.Phonetic("S1 S EH . K AX N . S2 D EH . RA I") + " weapon";
			sp.Speak(str);

*/

namespace UTB_voicespeaker.Synthesizer
{
	enum Language
	{
		English = 0,
		Finish,
		Norwegian,
		Russian,
		Swedish
	};

	class Speaker
	{
		private SpeechSynthesizer ss;
		private Language lang;

		public bool AsyncMode { set; get; }
		public bool UseSSML { set; get; }

		public Speaker(Language lang = Language.English)
		{
			this.lang = lang;
			AsyncMode = false;  // Default to synchron speech
			UseSSML = false;	// Default to non-SSML speech

			try
			{
				// Create synthesizer
				ss = new SpeechSynthesizer();
				ss.SetOutputToDefaultAudioDevice();

				// Select language
				if (!UseSSML)
				{
					switch (lang)
					{
						case Language.English: ss.SelectVoice("Microsoft Server Speech Text to Speech Voice (en-GB, Hazel)"); break;
						case Language.Finish: ss.SelectVoice("Microsoft Server Speech Text to Speech Voice (fi-FI, Heidi)"); break;
						case Language.Norwegian: ss.SelectVoice("Microsoft Server Speech Text to Speech Voice (nb-NO, Hulda)"); break;
						case Language.Russian: ss.SelectVoice("Microsoft Server Speech Text to Speech Voice (ru-RU, Elena)"); break;
						case Language.Swedish: ss.SelectVoice("Microsoft Server Speech Text to Speech Voice (sv-SE, Hedvig)"); break;
						default: ss.SelectVoice("Microsoft Server Speech Text to Speech Voice (en-GB, Hazel)"); break;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("An error occured: '{0}'", e);
			}
		}

		public void Speak(string message, int speed = 0)
		{

			try
			{
				// Build a prompt.
				PromptBuilder builder = new PromptBuilder();
				builder.AppendText(message);

				// Synth message
				ss.Rate = speed;
				if (UseSSML)
				{
					string ssmlString = "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\"";

					switch (lang)
					{
						case Language.English: ssmlString += " xml:lang=\"en-GB\">"; break;
						case Language.Finish: ssmlString += " xml:lang=\"fi-FI\">"; break;
						case Language.Norwegian: ssmlString += " xml:lang=\"nb-NO\">"; break;
						case Language.Russian: ssmlString += " xml:lang=\"ru-RU\">"; break;
						case Language.Swedish: ssmlString += " xml:lang=\"sv-SE\">"; break;
						default: ssmlString += " xml:lang=\"en-GB\">"; break;
					}

					ssmlString += "<s>" + message + "</s></speak>";

					if (AsyncMode) ss.SpeakSsmlAsync(ssmlString);
					else ss.SpeakSsml(ssmlString);
				}
				else
				{
					if (AsyncMode) ss.SpeakAsync(builder);
					else ss.Speak(builder);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("An error occured: '{0}'", e);
			}
		}

		public string Phonetic(string ph)
		{
			if (!UseSSML)
				return "";

			return "<phoneme alphabet =\"x-microsoft-ups\" ph=\"" + ph + "\"></phoneme>";
        }
	}
}