using System.Collections.Generic;

namespace ED47.BusinessAccessLayer.Excel.Reader
{
    public class BaseSyncClass
    { 
        public bool DeleteInExistent { get; set; }   
        public IRepository Repository { get; set; }
        public string UserName { get; set; }

        private IEnumerable<ExcelData> _syncData;
        public IEnumerable<ExcelData> SyncData
        {
            get { return  _syncData ?? (_syncData = new List<ExcelData>()); }
        }

        public BaseSyncClass(IEnumerable<ExcelData> syncData, IRepository repository, bool deleteExistent, string userName)
        {
            _syncData = syncData;
            Repository = repository;
            DeleteInExistent = deleteExistent;
            UserName = userName;
        }

    }
}