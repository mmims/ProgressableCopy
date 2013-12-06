using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressableCopy
{
    public interface IProgressableCopy
    {
        event EventHandler<BytesChangedEventArgs> BytesChanged;
        event EventHandler<CompletedEventArgs> Completed;

        string DestinationPath { get; set; }
        string SourcePath { get; set; }
        
        void Copy();
    }
}
