using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProgressableCopy
{
    public abstract class ProgressableCopyBase
    {
        // 16k copy buffer see http://stackoverflow.com/questions/3033771/file-io-with-streams-best-memory-buffer-size
        public readonly int BufferSize = 1024 * 1024;
        protected Object _lock = new Object();

        public event EventHandler<BytesChangedEventArgs> BytesChanged;
        public event EventHandler<CompletedEventArgs> Completed;

        protected ProgressableCopyBase(string sourcePath, string destinationPath)
        {
            CreateDirectory = false;
            DestinationPath = destinationPath;
            Overwrite = false;
            SourcePath = sourcePath;
        }

        #region Properties

        public bool CreateDirectory { get; set; }
        public abstract bool DestinationExists { get; }
        public abstract string DestinationPath { get; set; }        
        public bool Overwrite { get; set; }
        public abstract bool SourceExists { get; }
        public abstract string SourcePath { get; set; }
        public abstract long TotalBytes { get; }
        
        #endregion

        #region Instance Methods

        public virtual void Copy()
        {
            if (!SourceExists)
            {
                throw new FileNotFoundException();
            }

            if (DestinationPath == null)
            {
                throw new ArgumentException();
            }

            if (DestinationExists && !Overwrite)
            {
                throw new IOException();
            }
        }

        protected void RaiseBytesChanged(object sender, BytesChangedEventArgs args)
        {
            EventHandler<BytesChangedEventArgs> handler = BytesChanged;
            if (handler != null)
                handler(sender, args);
        }

        protected void RaiseCompleted(object sender, CompletedEventArgs args)
        {
            EventHandler<CompletedEventArgs> handler = Completed;
            if (handler != null)
                handler(sender, args);
        }

        #endregion
    }

    public class BytesChangedEventArgs : EventArgs
    {
        public long CopiedBytes { get; set; }
        public string DestinationPath { get; set; }
        public string SourcePath { get; set; }
        public long TotalBytes { get; set; }
    }

    public class CompletedEventArgs : EventArgs
    {
        public bool Successful { get; set; }
        public Exception Exception { get; set; }
    }
}
