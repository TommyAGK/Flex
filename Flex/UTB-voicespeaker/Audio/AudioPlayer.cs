using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media; // NOTE: For Windows Universal Apps use Windows.Media instead
using System.IO;

/*
	HOW TO USE THE AUDIO PLAYER:

			AudioPlayer ap = new AudioPlayer("Blue Oyster Bar.wav");
			ap.Play();
			//Console.ReadKey();
*/


namespace UTB_voicespeaker.Audio
{
	class AudioPlayer
	{
		private System.Media.SoundPlayer audioPlayer;

		public AudioPlayer(string soundFile)
		{
			try
			{
				audioPlayer = new SoundPlayer();
				if (Program.CustomProfile)
					{
					audioPlayer.SoundLocation = Directory.GetCurrentDirectory() + @"\Dependency\Sounds\Profile\" + soundFile;
					}
				else
					{

				audioPlayer.SoundLocation = Directory.GetCurrentDirectory() + @"\Dependency\Sounds\" + soundFile;
					}
				//audioPlayer.SoundLocation = @"C:\Users\kimlar\Source\Repos\utb-flex\Flex\UTB-voicespeaker\Dependency\Sounds\" + soundFile;
				audioPlayer.Load();
            }
			catch (Exception e)
			{
				Console.WriteLine("An error occured: {0}", e);
			}
		}

		public void Play(bool async = false)
		{
			try
			{
				if (async) audioPlayer.Play();
				else audioPlayer.PlaySync();				
			}
			catch (Exception e)
			{
				Console.WriteLine("An error occured: {0}", e);
			}
		}
	}
}
