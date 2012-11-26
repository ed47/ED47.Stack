using System.Collections.Generic;

namespace ED47.BusinessAccessLayer.Excel.Reader
{
    public class BaseSyncClass
    { 
        public bool DeleteInExistent { get; set; }   
        public Repository Repository { get; set; }


        private IEnumerable<ExcelData> _syncData;
        public IEnumerable<ExcelData> SyncData
        {
            get { return  _syncData ?? (_syncData = new List<ExcelData>()); }
        }

        public BaseSyncClass(IEnumerable<ExcelData> syncData, Repository repository, bool deleteExistent)
        {
            _syncData = syncData;
            Repository = repository;
            DeleteInExistent = deleteExistent;
        }

    }
}