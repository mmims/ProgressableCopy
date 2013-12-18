using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace ProgressableCopy
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() == 2)
            {
                if (Directory.Exists(args[0]))
                {
                    DCopy(args[0], args[1]);
                }
                else
                {
                    SFCopy(args[0], args[1]);
                }
            }
            else
            {
                Console.WriteLine("ERROR: Invalid arguments.");
            }
#if DEBUG
            Console.ReadKey();
#endif
        }

        static void DCopy(string source, string destination)
        {
            var sw = new System.Diagnostics.Stopwatch();
            var dc = new DirectoryProgressableCopy();
            dc.SourcePath = source;
            dc.DestinationPath = destination;
            dc.CreateDirectory = true;
            dc.Overwrite = true;
            dc.BytesChanged += (s, e) =>
            {
                PrintProgress(e);
            };
            dc.Completed += (s, e) =>
            {
                sw.Stop();
                if (e.Successful)
                {
                    Console.WriteLine("\r\n\r\nCompleted {0} files | {1} folders", dc.FileCount, dc.FolderCount);
                    Console.WriteLine("Diagnostics: {0} bytes | {1} ms | {2:F3} MB/s", dc.TotalBytes, sw.ElapsedMilliseconds, ((double)dc.TotalBytes / sw.ElapsedMilliseconds * 1000d / 1024d / 1024d));
                }
                else
                {
                    Console.WriteLine("\r\n\r\nERROR: {0}", e.Exception.Message);
                }
                Console.CursorVisible = true;
            };
            Console.CursorVisible = false;
            sw.Start();
            dc.Copy();
        }

        static void SFCopy(string source, string destination)
        {
            var sw = new System.Diagnostics.Stopwatch();
            var sfc = new FileProgressableCopy();
            sfc.SourcePath = source;
            sfc.DestinationPath = destination;
            sfc.CreateDirectory = true;
            sfc.Overwrite = true;
            sfc.BytesChanged += (s, e) =>
            {
                PrintProgress(e);
            };
            sfc.Completed += (s, e) =>
            {
                sw.Stop();
                if (e.Successful)
                {
                    Console.WriteLine("\r\n\r\nCompleted 1 file");
                    Console.WriteLine("Diagnostics: {0} bytes | {1} ms | {2:F3} MB/s", sfc.TotalBytes, sw.ElapsedMilliseconds, ((double)sfc.TotalBytes / sw.ElapsedMilliseconds * 1000d / 1024d / 1024d));
                }
                else
                {
                    Console.WriteLine("\r\n\r\nERROR {0}", e.Exception.Message);
                }
                Console.CursorVisible = true;
            };
            Console.CursorVisible = false;
            sw.Start();
            sfc.Copy();            
        }

        static void PrintProgress(BytesChangedEventArgs args)
        {
            double percentage = (double)args.CopiedBytes / args.TotalBytes * 100d;
            string pathFormat = "{0,-" + (Console.WindowWidth - 10).ToString() + "}";
            Console.CursorLeft = 0;            
            Console.Write(pathFormat, args.SourcePath);
            Console.CursorLeft = Console.WindowWidth - 8;
            Console.Write("{0,5:F1} %", percentage);
        }
    }
}
