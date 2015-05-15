using System;
using Linklaget;

/// <summary>
/// Transport.
/// </summary>
namespace Transportlaget
{
	/// <summary>
	/// Transport.
	/// </summary>
	public class Transport
	{
		private int x;
		private int y;
		private Link link;
		private Checksum checksum;
		private byte[] buffer;
		private byte seqNo;
		private byte old_seqNo;
		private int errorCount;
		private const int DEFAULT_SEQNO = 2;

		public Transport (int BUFSIZE)
		{
			link = new Link(BUFSIZE+(int)TransSize.ACKSIZE);
			checksum = new Checksum();
			buffer = new byte[BUFSIZE+(int)TransSize.ACKSIZE];
			seqNo = 0;
			old_seqNo = DEFAULT_SEQNO;
			errorCount = 0;
		}

		private bool receiveAck()
		{
			byte[] buf = new byte[(int)TransSize.ACKSIZE];
			int size = link.Receive(ref buf);
			if (size != (int)TransSize.ACKSIZE) return false;

			if(!checksum.checkChecksum(buf, (int)TransSize.ACKSIZE) ||
			   buf[(int)TransCHKSUM.SEQNO] != seqNo ||
			   buf[(int)TransCHKSUM.TYPE] != (int)TransType.ACK)
				return false;

			seqNo = (byte)((buf[(int)TransCHKSUM.SEQNO] + 1) % 2); // Fejl rettet

			return true;
		}

		private void sendAck (bool ackType)
		{
			byte[] ackBuf = new byte[(int)TransSize.ACKSIZE];

			//Testing
			y++;
			if (y == 30) {
				buffer [12]++;
			}

			ackBuf [(int)TransCHKSUM.SEQNO] = (byte)
				(ackType ? (byte)buffer [(int)TransCHKSUM.SEQNO] : (byte)(buffer[(int)TransCHKSUM.SEQNO] + 1) % 2); // Fejl rettet
			ackBuf [(int)TransCHKSUM.TYPE] = (byte)(int)TransType.ACK;
			checksum.calcChecksum (ref ackBuf, (int)TransSize.ACKSIZE);

			link.Send(ackBuf, (int)TransSize.ACKSIZE);
		}

		public void send(byte[] buf, int size)
		{
			Array.Copy (buf, 0, buffer, 4, size);
			buffer [(int)TransCHKSUM.TYPE] = 0;
			buffer [(int)TransCHKSUM.SEQNO] = seqNo;

			checksum.calcChecksum (ref buffer, size +4);
			do {
				//Testing
				x++;
				if (x == 30) {
					buffer [120]++;	//Laver en fejl i bufferen
				}
				if (x == 31) {
					buffer[120]--;	//Retter fejlen tilbage igen
				}

				link.Send (buffer, size + 4);
			} while (!receiveAck());
		}

		public int receive (ref byte[] buf)
		{
			int size;
			while (true) {
				size = link.Receive (ref buffer);

				if (buffer[(int)TransCHKSUM.SEQNO] == old_seqNo)
				{
					sendAck (true);
					return 0;
				}

				if (checksum.checkChecksum (buffer, size) && buffer[(int)TransCHKSUM.SEQNO] != old_seqNo) {
					old_seqNo = buffer[(int)TransCHKSUM.SEQNO];
					Array.Copy (buffer, 4, buf, 0, size-4);
					sendAck (true);
					return size - 4;
				} else {
					sendAck (false);
					errorCount += 1;
				}
			}
		}
	}
}
