using Transportlaget;
using Library;
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Application
{
	class file_client
	{	
		const int Bufsize = 1000;

		private int _progress = 0;

		private file_client (string[] args)
		{
			var trans = new Transport (Bufsize);
			trans.send (GetBytes(args[0]), args[0].Length);

			Console.WriteLine ("Asking server for file: " + args [0]);

			receiveFile (LIB.ExtractFileName (args [0]), trans);
		}


		private void receiveFile (String fileName, Transport transport)
		{
			var fileSize = new byte[Bufsize];

			transport.receive (ref fileSize);	//Receives the file size
			var fileSizeStr = GetString (fileSize).Trim ('\0');
			Console.WriteLine ("Receive filesize: " + fileSizeStr);
			if (fileSizeStr == "Fejl40") {
				Console.WriteLine ("Server could'nt find file - terminating program");
			} else {			
				var fs = File.Open (fileName, FileMode.Create);

				var buffer = new byte[Bufsize];
				var offset = 0;
				while (offset < int.Parse(fileSizeStr)) {
					var bytesRead = transport.receive (ref buffer);

					fs.Write (buffer, 0, bytesRead);
					offset += bytesRead;

					//PROGRESS BAR
					int procentage = (int)((double)offset / double.Parse (fileSizeStr) * 100);

					if (procentage > _procentageOld) {
						drawTextProgressBar (procentage, 100);
						//Console.WriteLine (procentage);
						_procentageOld = procentage;
					}


				}
				fs.Close ();
				Console.WriteLine ("");
			}
		}


		public static void Main (string[] args)
		{
			Console.WriteLine ("Client starts...");
			new file_client(args);
		}

		private static void drawTextProgressBar(int progress, int total)
		{
			//draw empty progress bar
			Console.CursorLeft = 0;
			Console.Write("["); //start
			Console.CursorLeft = 32;
			Console.Write("]"); //end
			Console.CursorLeft = 1;
			float onechunk = 30.0f / total;

			//draw filled part
			int position = 1;
			for (int i = 0; i < onechunk * progress; i++)
			{
				Console.BackgroundColor = ConsoleColor.Gray;
				Console.CursorLeft = position++;
				Console.Write(" ");
			}

			//draw unfilled part
			for (int i = position; i <= 31 ; i++)
			{
				Console.BackgroundColor = ConsoleColor.Green;
				Console.CursorLeft = position++;
				Console.Write(" ");
			}

			//draw totals
			Console.CursorLeft = 35;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.Write(progress.ToString() + "% of " + total.ToString() + "%    "); //blanks at the end remove any excess
		}
		static byte[] GetBytes(string str)
		{
			return Encoding.ASCII.GetBytes (str);
		}

		static string GetString(byte[] bytes)
		{
			return Encoding.ASCII.GetString (bytes);
		}
	}
}

