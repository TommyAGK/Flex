using UTB_voicespeaker.Audio;

namespace UTB_voicespeaker.Synthesizer
{
	class SpeechMacroProcessor
	{
		// TODO: Surround with try-catch
		public void Process(Language lang, string say, int speed = 0, string custLang = null)
			{
                Language curLanguage = lang;
				if (Program.CustomProfile)
					{
						if (custLang != null)
							{
								if (custLang.Equals("NOR"))
									{
									curLanguage = Language.Norwegian;
									}
								else if (custLang.Equals("SWE"))
									{
									curLanguage = Language.Swedish;
									}
								else if (custLang.Equals("FIN"))
									{
									curLanguage = Language.Finish;
									}
								else if (custLang.Equals("RUS"))
									{
									curLanguage = Language.Russian;
									}
								else
									{
									curLanguage = Language.English; // english default
									}
							}
					}
				Speaker sp = new Speaker(curLanguage);
				
				
			if (say.Contains("@phonetic"))
				sp.UseSSML = true;

			// Speech Macro Processor
			int spPos = 0;
			for (int i = 0; i < say.Length; i++)
			{
				if (say[i] == '@')
				{
				// @wav(Pacman.wav)
				if (say.Substring(i).StartsWith("@wav(Pacman.wav)"))
					{
					sp.Speak(say.Substring(spPos, i - spPos), speed);
					spPos = i + "@wav(Pacman.wav)".Length;

					AudioPlayer ap = new AudioPlayer("wav(Pacman.wav).wav");
					ap.Play();
					}



					// @phonetic(...)
					if (say.Substring(i).StartsWith("@phonetic("))
						{
						sp.Speak(say.Substring(spPos, i - spPos), speed);
						spPos = i + "@phonetic(".Length;

						// TODO: try-catch
						// Find closing ')'
						string ph = say.Substring(spPos, say.Substring(spPos).IndexOf(')'));
						spPos += ph.Length + 1;

						// Speak with phonetic
						//Speaker spph = new Speaker(lang);
						//spph.UseSSML = true;
						//spph.Speak(spph.Phonetic(ph));
						sp.Speak(sp.Phonetic(ph), speed);
						}
						
						// @wav(...)
				
					if (say.Substring(i).StartsWith("@wav("))
						{
						sp.Speak(say.Substring(spPos, i - spPos), speed);
						spPos = i + "@wav(".Length;

						// TODO: try-catch
						// Find closing ')'
						string ph = say.Substring(spPos, say.Substring(spPos).IndexOf(')'));
						spPos += ph.Length + 1;
						AudioPlayer ap = new AudioPlayer(ph);
						ap.Play();
						}

				}

			}
			sp.Speak(say.Substring(spPos), speed);

		}
	}
}
