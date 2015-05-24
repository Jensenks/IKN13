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
		private int _ackCount;
		private readonly Link _link;
		private readonly Checksum _checksum;
		private byte[] _buffer;
		private byte _seqNo;
		private byte _oldSeqNo;
		private int _errorCount;
		private const int DefaultSeqno = 2;

		public Transport (int BUFSIZE)
		{
			_link = new Link(BUFSIZE+(int)TransSize.ACKSIZE);
			_checksum = new Checksum();
			_buffer = new byte[BUFSIZE+(int)TransSize.ACKSIZE];
			_seqNo = 0;
			_oldSeqNo = DefaultSeqno;
			_errorCount = 0;
		}

		private bool receiveAck()
		{
			var buf = new byte[(int)TransSize.ACKSIZE];
			var size = _link.Receive(ref buf);
			if (size != (int)TransSize.ACKSIZE) return false;

			if(!_checksum.checkChecksum(buf, (int)TransSize.ACKSIZE) ||
			   buf[(int)TransCHKSUM.SEQNO] != _seqNo ||
			   buf[(int)TransCHKSUM.TYPE] != (int)TransType.ACK)
				return false;

			_seqNo = (byte)((buf[(int)TransCHKSUM.SEQNO] + 1) % 2);

			return true;
		}

		private void sendAck (bool ackType)
		{
			var ackBuf = new byte[(int)TransSize.ACKSIZE];

			ackBuf [(int)TransCHKSUM.SEQNO] = (byte)
				(ackType ? (byte)_buffer [(int)TransCHKSUM.SEQNO] : (byte)(_buffer[(int)TransCHKSUM.SEQNO] + 1) % 2); // Fejl rettet
			ackBuf [(int)TransCHKSUM.TYPE] = (byte)(int)TransType.ACK;
			_checksum.calcChecksum (ref ackBuf, (int)TransSize.ACKSIZE);

			_link.Send(ackBuf, (int)TransSize.ACKSIZE);
		}

		public void send(byte[] buf, int size)
		{
			Array.Copy (buf, 0, _buffer, 4, size);
			_buffer [(int)TransCHKSUM.TYPE] = 0;
			_buffer [(int)TransCHKSUM.SEQNO] = _seqNo;

			_checksum.calcChecksum (ref _buffer, size +4);
			do {
				//Testing
                _errorCount++;
                if (_errorCount == 30)
                {
					_buffer [120]++;	//Laver en fejl i bufferen
				}
                if (_errorCount == 31)
                {
					_buffer[120]--;	//Retter fejlen tilbage igen
				}

				_link.Send (_buffer, size + 4);
			} while (!receiveAck());
		}

		public int receive (ref byte[] buf)
		{
		    while (true) {
				var size = _link.Receive (ref _buffer);

				if (_buffer[(int)TransCHKSUM.SEQNO] == _oldSeqNo)
				{
					sendAck (true);
					return 0;
				}

				if (_checksum.checkChecksum (_buffer, size) && _buffer[(int)TransCHKSUM.SEQNO] != _oldSeqNo) {
					_oldSeqNo = _buffer[(int)TransCHKSUM.SEQNO];
					Array.Copy (_buffer, 4, buf, 0, size-4);
					sendAck (true);
					return size - 4;
				} else {
					sendAck (false);
					_errorCount += 1;
				}
			}
		}
	}
}
