using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace ED47.BusinessAccessLayer
{
    public static class QueuedBackgroundWorkerThread
    {
        public static void Start<T>(WaitCallback callback,T obj) {
            ThreadPool.QueueUserWorkItem(callback, obj);
        }
    }

    public static class QueuedBackgroundWorker
    {


        public static void QueueWorkItem<TIn, TOut>(Queue<QueueItem<TIn>> queue, TIn inputArgument, Func<DoWorkArgument<TIn>, TOut> doWork, Action<WorkerResult<TOut>> workerCompleted, Action<WorkerProgress> progressChanged)
        {
            if (queue == null) throw new ArgumentNullException("queue");

            var bw = new BackgroundWorker {WorkerReportsProgress = true, WorkerSupportsCancellation = false};

            bw.DoWork += (sender, args) =>
                             {
                                 if (doWork != null)
                                 {
                                     args.Result = doWork(new DoWorkArgument<TIn>((TIn) args.Argument));
                                 }
                             };
            bw.ProgressChanged += (sender, args) => progressChanged(new WorkerProgress(args.ProgressPercentage, args.UserState));
    
            bw.RunWorkerCompleted += (sender, args) =>
                                         {
                                             if (workerCompleted != null)
                                             {
                                                 workerCompleted(new WorkerResult<TOut>((TOut)args.Result, args.Error));
                                             }
                                             queue.Dequeue();
                                             if (queue.Count > 0)
                                             {
                                                 QueueItem<TIn> nextItem = queue.Peek();
                                                 nextItem.BackgroundWorker.RunWorkerAsync(nextItem.Argument);
                                             }
                                         };

            queue.Enqueue(new QueueItem<TIn>(bw, inputArgument));
            if (queue.Count == 1)
            {
                QueueItem<TIn> nextItem = queue.Peek();
                nextItem.BackgroundWorker.RunWorkerAsync(nextItem.Argument);
            }
        }

     
        public class DoWorkArgument<T>
        {
            public DoWorkArgument(T argument)
            {
                Argument = argument;
            }

            public T Argument { get; private set; }
        }

        public class WorkerResult<T>
        {
            public WorkerResult(T result, Exception error)
            {
                Result = result;
                Error = error;
            }

            public T Result { get; private set; }
            public Exception Error { get; private set; }
        }

        public class WorkerProgress
        {
            public WorkerProgress(int progress, object userState)
            {
                Progress = progress;
                UserState = userState;
            }

            public int Progress { get; private set; }
            public object UserState { get; private set; }
        }

        public class QueueItem<TIn>
        {
            public QueueItem(BackgroundWorker backgroundWorker, TIn argument)
            {
                BackgroundWorker = backgroundWorker;
                Argument = argument;
            }

            public TIn Argument { get; private set; }
            public BackgroundWorker BackgroundWorker { get; private set; }
        }
    }
}
