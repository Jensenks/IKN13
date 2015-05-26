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
	    private readonly Link link;
		private readonly Checksum checksum;
		private byte[] buffer;
		private byte seqNo;
		private byte oldSeqNo;
		private int errorCount;
        private const int DEFAULT_SEQNO = 2;

	    private int countToError;
	    private int countToError2;

		public Transport (int BUFSIZE)
		{
			link = new Link(BUFSIZE+(int)TransSize.ACKSIZE);
			checksum = new Checksum();
			buffer = new byte[BUFSIZE+(int)TransSize.ACKSIZE];
			seqNo = 0;
			oldSeqNo = DEFAULT_SEQNO;
			errorCount = 0;
		}

		private bool receiveAck()
		{
			var buf = new byte[(int)TransSize.ACKSIZE];
			var size = link.Receive(ref buf);
			if (size != (int)TransSize.ACKSIZE) return false;

			if(!checksum.checkChecksum(buf, (int)TransSize.ACKSIZE) ||
			   buf[(int)TransCHKSUM.SEQNO] != seqNo ||
			   buf[(int)TransCHKSUM.TYPE] != (int)TransType.ACK)
				return false;

			seqNo = (byte)((buf[(int)TransCHKSUM.SEQNO] + 1) % 2);

			return true;
		}

		private void sendAck (bool ackType)
		{
			var ackBuf = new byte[(int)TransSize.ACKSIZE];

            countToError++;
            if (countToError == 30)
            {
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
                countToError2++;
                if (countToError2 == 30)
                {
					buffer [120]++;	//Laver en fejl i bufferen
				}
                if (countToError2 == 31)
                {
					buffer[120]--;	//Retter fejlen tilbage igen
				}
				link.Send (buffer, size + 4);
			} while (!receiveAck());
		}

		public int receive (ref byte[] buf)
		{
		    while (true) {
				var size = link.Receive (ref buffer);

				if (buffer[(int)TransCHKSUM.SEQNO] == oldSeqNo)
				{
					sendAck (true);
					return 0;
				}

				if (checksum.checkChecksum (buffer, size) && buffer[(int)TransCHKSUM.SEQNO] != oldSeqNo) {
					oldSeqNo = buffer[(int)TransCHKSUM.SEQNO];
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
