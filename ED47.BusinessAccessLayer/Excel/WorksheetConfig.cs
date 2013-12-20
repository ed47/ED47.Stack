using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer.Excel
{
    public class WorksheetConfig
    {
        public bool AutoFilter { get; set; }

        public WorksheetConfig()
        {
            AutoFilter = false;
        }
    }
}
