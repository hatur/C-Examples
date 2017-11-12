using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public sealed class SingleThreadSynchronizationContext :
        SynchronizationContext
    {
        private readonly
         BlockingCollection<KeyValuePair<SendOrPostCallback, object>>
          m_queue =
           new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        public override void Post(SendOrPostCallback d, object state)
        {
            m_queue.Add(
                new KeyValuePair<SendOrPostCallback, object>(d, state));
        }

        public void RunOnCurrentThread()
        {
            KeyValuePair<SendOrPostCallback, object> workItem;
            while (m_queue.TryTake(out workItem, Timeout.Infinite))
                workItem.Key(workItem.Value);
        }

        public void Complete() { m_queue.CompleteAdding(); }

    }

    public class AsyncPump
    {
        public static void Run(Func<Task> func)
        {
            var prevCtx = SynchronizationContext.Current;
            try
            {
                var syncCtx = new SingleThreadSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(syncCtx);

                var t = func();
                t.ContinueWith(
                    delegate { syncCtx.Complete(); }, TaskScheduler.Default);

                syncCtx.RunOnCurrentThread();

                t.GetAwaiter().GetResult();
            }
            finally { SynchronizationContext.SetSynchronizationContext(prevCtx); }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var prog = new Program();

            Task.Run(() => prog.TestProc());

            for (int i = 0; i < 1000000; ++i)
            {
                if (i % 1000 == 0)
                { 
                    Console.Clear();
                }

                Console.Write(i);
            }

            Console.In.ReadLine();
        }

        public void TestProc()
        {
            Debug.WriteLine("Click invoked by " + Thread.CurrentThread.ManagedThreadId);

            AsyncPump.Run(async delegate
            {
                await InnerAsync();
            });

            Debug.WriteLine("Click finished by " + Thread.CurrentThread.ManagedThreadId);
        }

        private async Task<string> InnerAsync()
        {
            return await Task.Run(() =>
            {
                Debug.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " sleeping..");
                Thread.Sleep(2500);
                Debug.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " ended sleep!");
                return "Nobby" + Thread.CurrentThread.ManagedThreadId;
            });
        }
    }
}
