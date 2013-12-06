using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ProgressableCopy
{
    class Program
    {
        static FileProgressableCopy sfc = new FileProgressableCopy();
        static DirectoryProgressableCopy dc = new DirectoryProgressableCopy();

        static void Main(string[] args)
        {
            //SFCopy();
            DCopy();
            Console.ReadKey();
        }

        static void DCopy()
        {
            dc.SourcePath = @"C:\swimpe_x86";
            dc.DestinationPath = @"C:\swimpe_test";
            dc.CreateDirectory = true;
            dc.Overwrite = true;
            dc.BytesChanged += (s, e) =>
                {
                    PrintProgress(e);
                };
            dc.Completed += (s, e) =>
                {
                    if (e.Successful)
                        Console.WriteLine("\r\n\r\nCompleted {0} files | {1} folders | {2} bytes", dc.FileCount, dc.FolderCount, dc.TotalBytes);
                    else
                        Console.WriteLine("\r\n\r\nERROR: {0}", e.Exception.Message);
                    Console.CursorVisible = true;
                };
            Console.CursorVisible = false;
            dc.Copy();
        }

        static void SFCopy()
        {
            sfc.SourcePath = @"C:\swimpe_x86\media\sources\boot.wim";
            sfc.DestinationPath = @"C:\swimpe_x86\test\boot.wim";
            sfc.Overwrite = true;
            sfc.CreateDirectory = true;
            sfc.BytesChanged += (s, e) =>
                {
                    PrintProgress(e);
                };
            sfc.Completed += (s, e) =>
                {
                    Console.CursorTop++;
                    Console.CursorLeft = 0;
                    if (e.Successful)
                        Console.WriteLine("\r\n\r\nComplete {0} bytes", sfc.TotalBytes);
                    else
                        Console.WriteLine("\r\n\r\nERROR {0}", e.Exception.Message);
                    Console.CursorVisible = true;
                };

            Console.CursorVisible = false;
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
