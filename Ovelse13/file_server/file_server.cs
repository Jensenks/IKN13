using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
    class file_server
    {
        private const int BUFSIZE = 1000;

        private file_server()
        {
            Console.WriteLine("Server started...");
            var trans = new Transport(BUFSIZE);
            byte[] buf = new byte[BUFSIZE];
            while (true)
            {
                Console.WriteLine("Waiting for client to request file...");
                trans.receive(ref buf);
                var str = GetString(buf).Trim('\0');
                Console.WriteLine("Client is asking for file: " + str);

                var curFileSize = LIB.check_File_Exists(str);
                if (curFileSize == 0)
                {
                    Console.WriteLine("File does not exist");
                    trans.send(GetBytes("NOFILE"), 6);
                }
                else
                {
                    sendFile(str, curFileSize, trans);
                }
            }
        }

        private void sendFile(String fileName, long fileSize, Transport transport)
        {
            transport.send(GetBytes(fileSize.ToString()), fileSize.ToString().Length);
            Console.WriteLine("Sending file size to client: " + fileSize);

            var curFileSize = fileSize;

            var fs = File.Open(fileName, FileMode.Open);

            var buffer = new byte[BUFSIZE];

            while (curFileSize >= BUFSIZE)
            {
                fs.Read(buffer, 0, BUFSIZE);

                transport.send(buffer, BUFSIZE);
                curFileSize -= 1000;

                var procentage = (int)(((fileSize - (double)curFileSize) / fileSize) * 100);

                Console.CursorLeft = 0;
                Console.Write(procentage + "% completed");

                Array.Clear(buffer, 0, BUFSIZE);

            }
            Array.Clear(buffer, 0, BUFSIZE);
            fs.Read(buffer, 0, (int)curFileSize);	//Reads the remaining bytes
            transport.send(buffer, (int)curFileSize);
            Console.CursorLeft = 0;
            Console.WriteLine("100% complete");
            fs.Close();
            Console.WriteLine("File transfer complete");
        }

        public static void Main(string[] args)
        {
            new file_server();
        }

        static byte[] GetBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        static string GetString(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
