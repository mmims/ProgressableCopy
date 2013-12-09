using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProgressableCopy
{
    public class FileProgressableCopy : ProgressableCopyBase, IProgressableCopy
    {
        private FileInfo _destinationInfo;
        private FileInfo _sourceInfo;
        private long _copiedBytes = 0;

        #region Constructors

        public FileProgressableCopy() : this(null, null) { }
        public FileProgressableCopy(string sourcePath) : this(sourcePath, null) { }
        public FileProgressableCopy(string sourcePath, string destinationPath) : base(sourcePath, destinationPath) { }
        
        #endregion

        #region Properties

        public override bool DestinationExists
        {
            get
            {
                return _destinationInfo != null ? File.Exists(DestinationPath) : false;
            }
        }

        public override string DestinationPath
        {
            get
            {
                if (_destinationInfo == null) return null;

                if (_destinationInfo.Attributes >= 0 && _destinationInfo.Attributes.HasFlag(FileAttributes.Directory) && !String.IsNullOrEmpty(SourcePath))
                {
                    return Path.Combine(_destinationInfo.FullName, Path.GetFileName(SourcePath));
                }
                else
                {
                    return _destinationInfo.FullName;
                }
            }
            set
            {
                try
                {
                    _destinationInfo = new FileInfo(value);
                }
                catch (Exception)
                {
                    _destinationInfo = null;
                }
                finally
                {
                    Reset();
                }
            }
        }

        public override bool SourceExists
        {
            get
            {
                return _sourceInfo != null ? _sourceInfo.Exists : false;
            }
        }

        public override string SourcePath
        {
            get
            {
                return _sourceInfo != null ? _sourceInfo.FullName : null;
            }
            set
            {
                try
                {
                    _sourceInfo = new FileInfo(value);
                }
                catch (Exception)
                {
                    _sourceInfo = null;
                }
                finally
                {
                    Reset();
                }
            }
        }

        public override long TotalBytes
        {
            get
            {
                return _sourceInfo != null ? _sourceInfo.Length : 0;
            }
        }

        #endregion

        #region Instance Methods

        public override void Copy()
        {
            lock (_lock)
            {
                try
                {
                    base.Copy();
                    
                    if (CreateDirectory && !_destinationInfo.Directory.Exists)
                        _destinationInfo.Directory.Create();

                    using (Stream output = File.Open(DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (Stream input = _sourceInfo.OpenRead())
                        {
                            int bytesRead;
                            byte[] buffer = new byte[base.BufferSize];
                            BytesChangedEventArgs args = new BytesChangedEventArgs()
                            {
                                SourcePath = this.SourcePath,
                                DestinationPath = this.DestinationPath,
                                CopiedBytes = _copiedBytes,
                                TotalBytes = this.TotalBytes
                            };
                            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                output.Write(buffer, 0, bytesRead);
                                _copiedBytes += bytesRead;
                                args.CopiedBytes = _copiedBytes;
                                RaiseBytesChanged(this, args);
                            }
                        }
                    }
                    RaiseCompleted(this, new CompletedEventArgs() { Successful = true });
                }
                catch (Exception ex)
                {
                    RaiseCompleted(this, new CompletedEventArgs() { Successful = false, Exception = ex });
                }
            }
        }

        #endregion

        #region Helper Methods

        private void Reset()
        {
            lock (_lock)
            {
                _copiedBytes = 0;
            }
        }
        #endregion
    }
}
