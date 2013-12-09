﻿using System;
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
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            System.IO.File.Copy(@"C:\swimpe_x86\media\sources\boot.wim", @"Y:\swimpe_test\boot.wim");
            stopwatch.Stop();
            Console.WriteLine("Diagnostics: {0} ms", stopwatch.ElapsedMilliseconds);
            //SFCopy();
            //DCopy();
            Console.ReadKey();
        }

        static void DCopy()
        {
            var di = new System.IO.DirectoryInfo(@"C:\swimpe_x86");
            var fi = new System.IO.FileInfo(@"C:\swimpe_x86\export.cmd");
            
            var stopwatch = new System.Diagnostics.Stopwatch();
            dc.SourcePath = @"C:\swimpe_x86";
            dc.DestinationPath = @"y:\swimpe_test";
            dc.CreateDirectory = true;
            //dc.Overwrite = true;
            dc.BytesChanged += (s, e) =>
            {
                PrintProgress(e);
            };
            dc.Completed += (s, e) =>
            {
                stopwatch.Stop();
                if (e.Successful)
                {
                    Console.WriteLine("\r\n\r\nCompleted {0} files | {1} folders | {2} bytes", dc.FileCount, dc.FolderCount, dc.TotalBytes);
                    Console.WriteLine("Diagnostics: {0} ms @ {1} bytes", stopwatch.ElapsedMilliseconds, dc.BufferSize);
                }
                else
                {
                    Console.WriteLine("\r\n\r\nERROR: {0}", e.Exception.Message);
                }
                Console.CursorVisible = true;
            };
            Console.CursorVisible = false;
            stopwatch.Start();
            dc.Copy();

        }

        static void SFCopy()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            sfc.SourcePath = @"C:\swimpe_x86\media\sources\boot.wim";
            sfc.DestinationPath = @"Y:\swimpe_test\boot.wim";
            //sfc.Overwrite = true;
            sfc.CreateDirectory = true;
            sfc.BytesChanged += (s, e) =>
            {
                PrintProgress(e);
            };
            sfc.Completed += (s, e) =>
            {
                stopwatch.Stop();
                if (e.Successful)
                {
                    Console.WriteLine("\r\n\r\nComplete {0} bytes", sfc.TotalBytes);
                    Console.WriteLine("Diagnostics: {0} ms @ {1} bytes", stopwatch.ElapsedMilliseconds, sfc.BufferSize);
                }
                else
                {
                    Console.WriteLine("\r\n\r\nERROR {0}", e.Exception.Message);
                }
                Console.CursorVisible = true;
            };

            Console.CursorVisible = false;
            stopwatch.Start();
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
