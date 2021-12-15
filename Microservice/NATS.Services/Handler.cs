using NATS.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NATS.Services
{
    //[DisallowConcurrentExecution]
    public class Handler// : IJob
    {

        #region Nats + Redis Subscribes

        public Action<string, int> Action { get; set; }
        private ConcurrentDictionary<long, string> data { get; set; }
        private ConnectionFactory natsConnectionFactory { get; set; }
        private IConnection natsConnection { get; set; }
        private IAsyncSubscription natSubscription { get; set; }
        private string natsSubject { get; set; }
        private Options natsOptions { get; set; }
        private TimeSpan natsInterval { get; set; }
        private TimeSpan natsTimeout { get; set; }
        private int bytesLimit { get; set; }
        private int natsAmount { get; set; }
        private int natsBulk { get; set; }
        private Thread natsThread { get; set; }
        private bool started { get; set; }
        private bool exited { get; set; }

        public Handler() { }

        public Handler(Config config, bool isCronJob = false)
        {
            natsConnectionFactory = new ConnectionFactory();

            natsOptions = ConnectionFactory.GetDefaultOptions();
            natsOptions.Name = config.Nats.Name;
            natsOptions.Url = config.Nats.Addr;
            natsOptions.Token = config.Nats.Token;
            if (!string.IsNullOrEmpty(config.Nats.Cert))
                natsOptions.AddCertificate(config.Nats.Cert);
            if (!string.IsNullOrEmpty(config.Nats.Cred) && !string.IsNullOrEmpty(config.Nats.Key))
                natsOptions.SetUserCredentials(config.Nats.Cred, config.Nats.Key);
            natsOptions.AllowReconnect = true; // auto reconnect.
            natsOptions.MaxReconnect = 1200; // Options.ReconnectForever
            natsOptions.ReconnectWait = 2000; // 2 second
            natsOptions.PingInterval = 60000; // 1 minute
            natsOptions.Timeout = 2000; // 2 second
            natsOptions.ReconnectBufferSize = 104857600; // 100Mb size of messages kept while busy reconnecting.
            natsOptions.SubChannelLength = config.Nats.MsgLimit; // sets number of messages the subscriber will buffer internally.
            bytesLimit = config.Nats.BytesLimit;

            natsConnection = natsConnectionFactory.CreateConnection(natsOptions);
            natsSubject = config.Subject;

            var interval = TimeSpan.FromMilliseconds(config.Nats.Interval);
            natsInterval = interval < TimeSpan.FromSeconds(1) ? TimeSpan.FromSeconds(1) : interval;
            natsTimeout = TimeSpan.FromMinutes(2);
            natsAmount = config.Nats.Amount;
            natsBulk = config.Nats.Bulk;

            if (isCronJob) return;

            data = new ConcurrentDictionary<long, string>();
            started = true; exited = false;

            natsThread = new Thread(Run) { IsBackground = true };
            natsThread.Start();
        }

        void Run()
        {
            var keysLock = new object();
            var keysRemove = new List<long>();
            long natsIndex = 0, natsCount = 0;
            int maxAmount = natsAmount, maxNum = natsBulk;
            if (maxAmount < 1) maxAmount = 0;
            if (maxNum < 1) maxNum = 1;

            natSubscription = natsConnection.SubscribeAsync(natsSubject, (sender, e) =>
            {
                if (started == false || e.Message?.Data == null) return;
                data.TryAdd(Interlocked.Increment(ref natsCount), Encoding.UTF8.GetString(e.Message.Data));
            });
            natSubscription.SetPendingLimits(natsOptions.SubChannelLength, natsOptions.SubChannelLength * bytesLimit);


            Task.Factory.StartNew(() =>
            {
                var c = maxNum;
                if (c < 100) c = 100;
                var redisList = new Util.Redis.List();
                while (true)
                {
                    Thread.Sleep(natsInterval);
                    if (started == false) continue;

                    try
                    {
                        var list = redisList.Pop(natsSubject, c);
                        if (list.Count == 0) continue;

                        foreach (string item in list) data.TryAdd(Interlocked.Increment(ref natsCount), item);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(@"[{0:T}] {1} > error: {2}", DateTime.Now, natsSubject, ex.Message);
                    }
                }
            }, TaskCreationOptions.LongRunning);


            Task.Factory.StartNew(() =>
            {
                var c = maxNum;
                if (c < 100) c = 100;
                while (true)
                {
                    Thread.Sleep(natsInterval);
                    if (started == false) continue;

                    if (Interlocked.Read(ref natsCount) > Interlocked.Read(ref natsIndex)) continue;

                    lock (keysLock) for (int i = 0; i < c && i < keysRemove.Count; i++) data.TryRemove(keysRemove[i], out _);
                }
            }, TaskCreationOptions.LongRunning);


            var ts1 = DateTimeOffset.Now.ToUnixTimeSeconds();
            while (exited == false)
            {
                Thread.Sleep(natsInterval);
                if (started == false || Action == null) continue;

                try
                {
                    int num = 0, records = 0;
                    var start = DateTime.Now;
                    long index = Interlocked.Read(ref natsIndex), count = Interlocked.Read(ref natsCount);
                    if (index < count) Console.WriteLine(@"[{0:T}] {1} > ", start, natsSubject);

                    while (index < count && (exited || maxAmount == 0 || num < maxAmount))
                    {
                        var idx = index;
                        var list = new List<string>();

                        for (int i = 0; i < maxNum; i++, num++)
                        {
                            if (index >= count)
                                break;

                            index++;

                            if (data.TryGetValue(index, out string item))
                                list.Add(item);
                        }

                        if (list.Count == 0) continue;

                        Action.Invoke("[" + string.Join(",", list) + "]", list.Count);

                        records += list.Count;
                        Interlocked.Exchange(ref natsIndex, index);
                        lock (keysLock) for (var i = 0; i < list.Count; i++) keysRemove.Add(idx + i + 1);
                    }

                    if (records > 0)
                    {
                        var end = DateTime.Now;
                        index = Interlocked.Read(ref natsIndex); count = Interlocked.Read(ref natsCount);
                        Console.WriteLine("[{0:T}] {1} > {2} > {3} \\ {4} / {5}", end, natsSubject, end.Subtract(start).ToString().Replace("00:", ""), records, count - index, count);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"[{0:T}] {1} > error: {2}", DateTime.Now, natsSubject, ex.Message);
                }
                finally
                {
                    if (DateTime.Now.Hour == 2 && Interlocked.Read(ref natsIndex) >= Interlocked.Read(ref natsCount) && natsCount > 0)
                    {
                        natsConnection?.Flush();
                        Interlocked.Exchange(ref natsIndex, 0);
                        Interlocked.Exchange(ref natsCount, 0);
                    }
                }
            }
        }

        public IConnection NewConnection() => natsConnectionFactory.CreateConnection(natsOptions);
        public IConnection Connection => natsConnection;
        public void Start() => started = true;
        public void Stop() => started = false;
        public void Wait(bool exit = false, Action callback = null)
        {
            exited = exit;
            natsThread?.Join();
            natSubscription?.Unsubscribe();
            natsConnection?.Drain();
            callback?.Invoke();
        }
        public void Exit()
        {
            if (exited) return;
            exited = true;
            natsThread?.Join();
            natSubscription?.Unsubscribe();
            natsConnection?.Drain();
        }

        #endregion

        //#region Quartz.NET Job And Cron Expressions

        //public async Task Execute(IJobExecutionContext context)
        //{
        //    var task = new Task(() =>
        //    {
        //        string subject = context.JobDetail.Key.Name, jsFunction = context.JobDetail.Key.Group;

        //        try
        //        {
        //            var js = Config.Items.Find(c => c.Subject == subject).JS;
        //            var res = js.Engine.Invoke(jsFunction, DateTime.Now);
        //            if (!(res is Undefined))
        //            {
        //                // execute sql command
        //                if ("String" == res.GetType().Name && res.ToString().Length >= 20)
        //                    res = js.Database.x(res) + " records affected database";
        //                Console.Write(@"[{0:T}] {1} > return: ", DateTime.Now, subject);
        //                V8Script.ConsoleFunctions.log(res);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(@"[{0:T}] {1} > error: {2}", DateTime.Now, subject, ex.Message);
        //        }
        //    });
        //    task.Start();
        //    await task;
        //}

        //#endregion

    }
}
