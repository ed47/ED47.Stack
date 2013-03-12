using System.Collections.Generic;
using System.Runtime.Caching;

namespace ED47.BusinessAccessLayer.Excel.Reader
{
    public class BaseImportWorker
    {
     
        /// <summary>
        /// Gets an object on which to do a safe lock() unto to prevent simultaneous writes on the same item.
        /// </summary>
        /// <param name="importClassName"> </param>
        /// <param name="key">The lock key.</param>
        /// <returns>An object on which to lock() to.</returns>
        public static ImportStatus GetImportStatus(string importClassName, string key) {
            var cacheKey = "ImportStatus." + importClassName + "?key=" + key;
                var currentLock = MemoryCache.Default.Get(cacheKey) as ImportStatus;

                if (currentLock == null) {
                    currentLock = new ImportStatus();
                    MemoryCache.Default.Add(cacheKey, currentLock,
                        new CacheItemPolicy {
                            Priority = CacheItemPriority.NotRemovable
                        });
                }

                return currentLock;
        }

        protected   Queue<QueuedBackgroundWorker.QueueItem<BaseSyncClass>> m_qeue = new Queue<QueuedBackgroundWorker.QueueItem<BaseSyncClass>>();

        public void Sync(BaseSyncClass syncData)
        {
            QueuedBackgroundWorkerThread.Start(ProcessData, syncData);
        }

        public void ProcessData(object syncData)
        {
            DoProcessData((BaseSyncClass)syncData);
        }

        public void DoProcessData(BaseSyncClass syncData)
        {
            QueuedBackgroundWorker.QueueWorkItem(m_qeue, syncData, DoWorkQeued, argsend => WorkCompleted(argsend), argProg => WorkProgress(argProg));      
        }


        public virtual object WorkProgress(QueuedBackgroundWorker.WorkerProgress argProg)
        {
            return true;
        }

        public virtual BaseSyncClass DoWorkQeued(QueuedBackgroundWorker.DoWorkArgument<BaseSyncClass> args) {
           
            return null;
        }

        public virtual object WorkCompleted(QueuedBackgroundWorker.WorkerResult<BaseSyncClass> args)
        {
            return args;
        }

    }

    // Auxiliar Sync Class for background worker 
}