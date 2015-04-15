/*

Copyright (C) 2015 Chris Poole

to compile on Windows 7 and prior:
%windir%\Microsoft.NET\Framework\v3.5\csc.exe /target:exe hotlinesav.cs

to compile on Windows 8 and later:
%windir%\Microsoft.NET\Framework\v4.0.30319\csc.exe /target:exe hotlinesav.cs

*/

namespace HotlineSav
{
    using System;
    using System.IO;
    using System.Text;

    class Program
    {
        static string basePath;
        static string saveFilename;
        static string unpackDirectory;

        static void Unpack()
        {
            Console.WriteLine("Creating unpack directory {0}", unpackDirectory);
            Directory.CreateDirectory(unpackDirectory);

            Console.WriteLine("Opening {0}", saveFilename);
            using (BinaryReader reader = new BinaryReader(File.Open(saveFilename, FileMode.Open)))
            {
                int version = reader.ReadByte();

                Console.WriteLine("Version {0}", version);

                uint filenameLength = reader.ReadUInt32();

                while ((filenameLength > 0) && (filenameLength != 0xDEADBEEF))
                {
                    byte[] filenameBytes = reader.ReadBytes((int)filenameLength);

                    string filename = Encoding.UTF8.GetString(filenameBytes, 0, filenameBytes.Length);

                    Console.WriteLine("working on file {0}", filename);

                    int fileLength = reader.ReadInt32();

                    byte[] fileBytes = reader.ReadBytes(fileLength);

                    using (BinaryWriter writer = new BinaryWriter(File.Open(unpackDirectory + @"\" + filename, FileMode.Create)))
                    {
                        writer.Write(fileBytes);
                    }

                    filenameLength = reader.ReadUInt32();
                }
            }
        }

        static void Pack()
        {
            string[] files = Directory.GetFiles(unpackDirectory);

            using (BinaryWriter writer = new BinaryWriter(File.Open(saveFilename, FileMode.Create)))
            {
                writer.Write((byte)1);


                foreach (string filename in files)
                {
                    byte[] filenameBytes = Encoding.UTF8.GetBytes(Path.GetFileName(filename));
                    writer.Write((uint)filenameBytes.Length);
                    writer.Write(filenameBytes);

                    using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
                    {
                        long length = (uint)reader.BaseStream.Length;
                        byte[] fileBytes = reader.ReadBytes((int)length);

                        writer.Write((uint)fileBytes.Length);
                        writer.Write(fileBytes);
                    }
                }

                writer.Write((uint)0xDEADBEEF);

                long position = writer.BaseStream.Position;
                long remainder = 262144 - position;

                byte[] emptyBytes = new byte[remainder];

                writer.Write(emptyBytes);
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("Hotline Miami Save Unpacker\r\n\r\nHotlineSav unpack - unpacks .sav file into unpack folder\r\nHotlineSav pack - pack unpack folder into .sav file\r\n");
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\my games\HotlineMiami";
            saveFilename = basePath + @"\SaveData.sav";
            unpackDirectory = basePath + @"\Unpack";

            switch (args[0])
            {
                case "pack": Pack(); break;
                case "unpack": Unpack(); break;
                default: ShowHelp(); break;
            }
        }
    }
}
