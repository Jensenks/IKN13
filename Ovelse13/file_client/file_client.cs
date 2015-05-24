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

        private file_client(string[] args)
        {
            Console.WriteLine("Client started...");
            var trans = new Transport(Bufsize);
            trans.send(GetBytes(args[0]), args[0].Length);

            Console.WriteLine("Asking server for file: " + args[0]);

            receiveFile(LIB.ExtractFileName(args[0]), trans);
        }


        private void receiveFile(String fileName, Transport transport)
        {
            var fileSize = new byte[Bufsize];
            transport.receive(ref fileSize);

            var fileSizeStr = GetString(fileSize).Trim('\0');

            if (fileSizeStr == "NOFILE")
            {
                Console.WriteLine("File does not exist");
            }
            else
            {
                Console.WriteLine("The size of the requested file is: " + fileSizeStr);
                var fs = File.Open(fileName, FileMode.Create);

                var buffer = new byte[Bufsize];
                var offset = 0;
                while (offset < int.Parse(fileSizeStr))
                {
                    var bytesRead = transport.receive(ref buffer);

                    fs.Write(buffer, 0, bytesRead);
                    offset += bytesRead;

                    var procentage = (int)(offset / double.Parse(fileSizeStr) * 100);

                    Console.CursorLeft = 0;
                    Console.Write(procentage + "% completed");
                }
                Console.CursorLeft = 0;
                Console.WriteLine("100% complete");
                fs.Close();
                Console.WriteLine("File transfer complete");
            }
        }

        public static void Main(string[] args)
        {
            new file_client(args);
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

