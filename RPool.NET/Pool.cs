
using RPool.NET.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;

namespace RPool.NET
{
    public static class Pool
    {
        private static int minPoolSize = ((PoolSetting)ConfigurationManager.GetSection(PoolSettingConstants.SECTION_NAME)).MinPoolSize;
        private static int maxPoolSize = ((PoolSetting)ConfigurationManager.GetSection(PoolSettingConstants.SECTION_NAME)).MaxPoolSize;
        private static int limitLife = ((PoolSetting)ConfigurationManager.GetSection(PoolSettingConstants.SECTION_NAME)).LimitLife;
        private static int limitTimes = ((PoolSetting)ConfigurationManager.GetSection(PoolSettingConstants.SECTION_NAME)).LimitTimes;

        private static bool _inlnitialize = false;
        private static object poolLevelLock = new object();
        public static PoolItemWithLock[] processArr = new PoolItemWithLock[maxPoolSize];
        private static Random random = new Random();
        private static Type T=null;
        public static void InInitialize<T>() where T : PoolItemWithLock, new()
        {
            Pool.T =typeof(T);
            if (_inlnitialize == false)
            {
                lock (poolLevelLock)
                {
                    if (_inlnitialize == false)
                    {
                        var minPoolSize = ((PoolSetting)ConfigurationManager.GetSection(PoolSettingConstants.SECTION_NAME)).MinPoolSize;
                        for (int i = 0; i < minPoolSize; i++)
                        {
                            processArr[i] = new T();
                        }
                    }
                    _inlnitialize = true;
                }
            }
        }
        internal static void AutoAddPoolItem()
        {
            KeepPoolItem();
            var targets = processArr.Where(s => s != null).ToList();
            if (targets.Count < maxPoolSize)
            {
                lock (poolLevelLock)
                {
                     targets = processArr.Where(s => s != null).ToList();
                    if (targets.Count < maxPoolSize)
                    {
                        lock (poolLevelLock)
                        {
                            for (int z = 0; z < maxPoolSize; z++)
                            {
                                if (processArr[z] == null)
                                {
                                    processArr[z] =(PoolItemWithLock) T.Assembly.CreateInstance(T.FullName);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

        }
        internal static void KeepPoolItem()
        {
            try
            {
                var now = DateTime.Now;
                var targets = processArr.Where(s => s != null && (s.State == ProcessState.ByeBye || (now - s.CreateTime).Minutes > limitLife || s.TaskRunTimes >= limitTimes)).ToList();
                if (targets.Count > 0)
                {
                    lock (poolLevelLock)
                    {
                        for (int i = 0; i < maxPoolSize; i++)
                        {
                            if (processArr[i]!=null&& (processArr[i] .State== ProcessState.ByeBye|| (processArr[i].TaskRunTimes>limitTimes)))
                            {
                                processArr[i].Dispose();
                                processArr[i] = null;
                            }
                        }
                      
                    }
                }
            }
            catch (Exception e)
            {

            }
            try
            {
                lock (poolLevelLock)
                {
                    var nowCount = processArr.Count(s => s != null);
                    for (int i = 0; i < minPoolSize - nowCount; i++)
                    {
                        var full = false;
                        for (int z = 0; z < maxPoolSize; z++)
                        {
                            if (processArr[z] == null)
                            {
                                processArr[z] = (PoolItemWithLock)T.Assembly.CreateInstance(T.FullName);
                                break;
                            }
                            else if (i == maxPoolSize)
                            {
                                full = true;
                            }
                        }
                        if (full)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
          

        }

        public static CommandReault ExecuteR(string command, string args)
        {
            return ExecuteR(command, args, true);
        }
        public static CommandReault ExecuteR(string command, string args,bool canRetry)
        {
            var processs = processArr.Where(s =>s!=null&& s.State == ProcessState.Ready).ToList();
            CommandReault result;
            try
            {
                if (processs.Count>0 )
                {
                    var pro = processs[0];
                    lock (pro.Lock)
                    {
                        result =pro.ExecuteCommand(command, args);
                    }
                }
                else
                {
                    var availableProcess = processArr.Where(s => s != null && s.State != ProcessState.ByeBye && s.State != ProcessState.InInitializing).ToList();
                    if (availableProcess.Count == 0)
                    {
                        lock (poolLevelLock)
                    {
                        AutoAddPoolItem();
                        availableProcess = processArr.Where(s => s != null && s.State != ProcessState.ByeBye && s.State != ProcessState.InInitializing).ToList();
                    }
                }
                    else
                {
                    //线程负责动态增加新的执行程序
                    Thread t = new Thread(AutoAddPoolItem);
                    t.Start();
                }
                //该任务随机交给一个可用执行程序执行，可能会排队，但一般情况下会比启动新程序更快
                var pro = availableProcess[random.Next(availableProcess.Count)];
                    lock (pro.Lock)
                    {
                        result = pro.ExecuteCommand(command, args);
                    }
                }
                if ((result.State == CommandReaultType.ProcessMayClosed || result.State == CommandReaultType.ProcessNotReady) && canRetry)
                {
                  //  Thread.Sleep(random.Next(10));
                    return ExecuteR(command, args, true);
                }
                else
                {
                    return result;
                }
            }
            catch (Exception)
            {
                return new CommandReault { State = CommandReaultType.ProcessMayClosed , Result="进程错误且重试失败"};
            }
           
        }
     }
}
