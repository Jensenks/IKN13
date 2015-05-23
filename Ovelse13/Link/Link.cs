using System;
using System.IO.Ports;

/// <summary>
/// Link.
/// </summary>
using System.Collections.Generic;


namespace Linklaget
{
	/// <summary>
	/// Link.
	/// </summary>
	public class Link
	{
		/// <summary>
		/// The DELIMITE for slip protocol.
		/// </summary>
		const byte Delimiter = (byte)'A';
		/// <summary>
		/// The buffer for link.
		/// </summary>
		private byte[] _buffer;
		/// <summary>
		/// The serial port.
		/// </summary>
		SerialPort serialPort;

		/// <summary>
		/// Initializes a new instance of the <see cref="link"/> class.
		/// </summary>
		public Link (int bufsize)
		{
			// Create a new SerialPort object with default settings.
			serialPort = new SerialPort("/dev/ttyS1",115200,Parity.None,8,StopBits.One);

			if(!serialPort.IsOpen)
				serialPort.Open();

			_buffer = new byte[(bufsize*2)+2];
		}

		/// <summary>
		/// Send the specified buf and size.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		/// <param name='size'>
		/// Size.
		/// </param>
		public void Send (byte[] buf, int size)
		{
			var list = new List<byte>();
			for (var i = 0; i < size; i++) {
				list.Add (buf [i]);
			}

			for (var i = 0; i < list.Count; i++) {
				if (list[i] == Convert.ToByte('A')) 
                {
					list[i] = Convert.ToByte('B');
					list.Insert (i + 1, Convert.ToByte('C'));
				} 
                else if (list[i] == Convert.ToByte('B')) 
                {
					list.Insert (i + 1, Convert.ToByte('D'));
				}
			}
            // Framing
            list.Insert(0, Delimiter);
            list.Add(Delimiter);

			serialPort.Write (list.ToArray (), 0, list.Count);
		}

		/// <summary>
		/// Receive the specified buf and size.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		/// <param name='size'>
		/// Size.
		/// </param>
		public int Receive (ref byte[] buf)
		{
			var tempList = new List<byte>();
			byte prev = 0;
		    if ((char) (serialPort.ReadByte ()) == 'A')
			{
			    byte byteRead;
			    while ((char)(byteRead = Convert.ToByte(serialPort.ReadByte ())) != 'A') {
					if ((char)prev == 'B' && (char)(byteRead) == 'C') {
						tempList [tempList.Count - 1] = Convert.ToByte ('A');
					} else if ((char)prev == 'B' && (char) (byteRead) == 'D') {
						tempList [tempList.Count - 1] = Convert.ToByte ('B');
					} else {
						tempList.Add (byteRead);
					}
					prev = byteRead;
				}
			}

		    Array.Copy (tempList.ToArray(), buf, tempList.Count);
			return tempList.Count;
		}
	}
}
