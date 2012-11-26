using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer
{
    public class CancellableRepositoryEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
    }
}
