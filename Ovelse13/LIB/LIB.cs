using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Library
{
	public class LIB
	{
		private LIB ()
		{
		}

		/// <summary>
		/// Extracts the name of the file.
		/// </summary>
		/// <returns>
		/// The filename only.
		/// </returns>
		/// <param name='fileName'>
		/// Filename with path.
		/// </param>
		public static String ExtractFileName(String fileName)
    	{
    		return (fileName.LastIndexOf('/')==0 ? fileName : fileName.Substring(fileName.LastIndexOf('/')+1));
    	}

		/// <summary>
		/// Check_s the file_ exists.
		/// </summary>
		/// <returns>
		/// The filesize.
		/// </returns>
		/// <param name='fileName'>
		/// The filename.
		/// </param>
		public static long check_File_Exists (String fileName)
		{
		    return !File.Exists(fileName) ? 0 : (new FileInfo(fileName)).Length;
		}
	}
}

