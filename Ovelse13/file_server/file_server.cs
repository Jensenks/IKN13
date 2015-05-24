using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
	class file_server
	{
		/// <summary>
		/// The BUFSIZE
		/// </summary>
		private const int BUFSIZE = 1000;
		private int procentageOld = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="file_server"/> class.
		/// </summary>
		private file_server ()
		{
			Console.WriteLine ("Server started...");
			var trans = new Transport (BUFSIZE);
			var buf = new byte[BUFSIZE];
			while (true) {
				Console.WriteLine ("Waiting for client");
				trans.receive (ref buf);
				var str = GetString (buf).Trim ('\0');
				Console.WriteLine ("Client is asking for file: " + str);

				var curFileSize = LIB.check_File_Exists (str); //Returns 0 if failed else file size
				if (curFileSize == 0) {
					Console.WriteLine ("File does not exist");
					trans.send (GetBytes ("NOFILE"), 6);
				} else {
					sendFile (str, curFileSize, trans);
				}
			}
		}

		/// <summary>
		/// Sends the file.
		/// </summary>
		/// <param name='fileName'>
		/// File name.
		/// </param>
		/// <param name='fileSize'>
		/// File size.
		/// </param>
		/// <param name='tl'>
		/// Tl.
		/// </param>
		private void sendFile(String fileName, long fileSize, Transport transport)
		{
			transport.send (GetBytes (fileSize.ToString ()), fileSize.ToString ().Length);
			Console.WriteLine("Sending file size to client: " + fileSize.ToString());

			var curFileSize = fileSize;

			var fs = File.Open(fileName, FileMode.Open);

			var buffer = new byte[BUFSIZE];

			while (curFileSize >= BUFSIZE)	//Reads 1000 bytes from file until the remaining filesize is under 1000 bytes
			{
				fs.Read(buffer, 0, BUFSIZE);

				transport.send (buffer, BUFSIZE);
				curFileSize -= 1000;

				int procentage = (int)(((fileSize-(double)curFileSize) / fileSize) * 100);


				if (procentage > procentageOld) {
					drawTextProgressBar (procentage, 100);
					procentageOld = procentage;
				}

				Array.Clear(buffer, 0, BUFSIZE);

			}
			Array.Clear(buffer, 0, BUFSIZE);
			fs.Read(buffer, 0, (int)curFileSize);	//Reads the remaining bytes
			transport.send (buffer, (int)curFileSize);

			drawTextProgressBar (100, 100);
			fs.Close();
			Console.WriteLine("");
			Console.WriteLine("File closed");

		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		public static void Main (string[] args)
		{
			new file_server();
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
