using RPool.NET.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPool.NET
{
    public class PoolItemWithLock : IDisposable
    {
        public DateTime CreateTime { get; set; }
        public object Lock { get; set; }
        public PoolItemWithLock()
        {
            Lock = new object();
            ProcessStartInfo info =new ProcessStartInfo {
                        FileName =((PoolSetting)ConfigurationManager.GetSection(PoolSettingConstants.SECTION_NAME)).RDirectory,
                        WorkingDirectory =((PoolSetting)ConfigurationManager.GetSection(PoolSettingConstants.SECTION_NAME)).WorkingDirectory  ,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput=true,
                        Arguments = "--no-save",
                        CreateNoWindow = true,//不显示程序窗口
                    };
            Process = new Process
            {
                StartInfo = info

            };


            lock (this.Lock)
            {
                CreateTime = DateTime.Now;
                this.State = ProcessState.InInitializing;
                this.Process.Start();
                OnInInitialize();
                this.State = ProcessState.Ready;
            }
        }
        public virtual void OnInInitialize()
        {
          
        }

        protected CommandReault ExecuteCommand(string command,bool ignoreState, string args)
        {
            if (this.State == ProcessState.Ready|| ignoreState)
            {
                lock (this.Lock)
                {
                    if (this.State == ProcessState.Ready|| ignoreState)
                    {

                        this.State = ProcessState.Busy;
                     
                        try
                        {
                            var fn = "f" + DateTime.Now.ToString("HHmmssfff");
                            var commandStr = string.Format("{0}{1}{2}", fn + "<-function(args){\n", command, "\nprint(\"ok\")\n}\n" + fn + "(\"" + args + "\")");
                            this.Process.StandardInput.WriteLine(commandStr);
                            StringBuilder output = new StringBuilder();
                            var outputline = this.Process.StandardOutput.ReadLine();
                            bool hasError = false;
                            while (outputline != "[1] \"ok\"")
                            {
                                output.Append(outputline + "\n");
                                outputline = this.Process.StandardOutput.ReadLine();
                                if (outputline == null)
                                {
                                    hasError = true;
                                    break;
                                }
                            }
                            if (!ignoreState)
                            {
                                ++this.TaskRunTimes;
                            }
                            //设置状态
                            if (this.TaskRunTimes >= ((PoolSetting)ConfigurationManager.GetSection(PoolSettingConstants.SECTION_NAME)).LimitTimes)
                            {
                                this.State = ProcessState.ByeBye;
                            }
                            else
                            {
                                this.State = ProcessState.Ready;
                            }
                            if (!ignoreState)
                            {
                                Thread t = new Thread(Pool.KeepPoolItem);
                                t.Start();
                            }
                         

                            if (hasError)
                            {
                                return new CommandReault { Result = this.Process.StandardError.ReadToEnd(), State = CommandReaultType.Error };
                            }
                            else
                            {
                                var start = fn + "(\"" + args + "\")";
                                var outputStr = output.ToString();
                                outputStr = outputStr.Substring(outputStr.IndexOf(start) + start.Length + 1);
                              
                                return new CommandReault { Result = outputStr, State = CommandReaultType.Success };
                            }
                        }
                        catch (Exception)
                        {
                            return new CommandReault { Result = "进程可能已被关闭或状态异常", State= CommandReaultType.ProcessMayClosed  };
                        }

                    }
                    else
                    {
                        return new CommandReault { Result = "进程未就绪", State = CommandReaultType.ProcessNotReady };
                    }
                }
            }
            else
            {
                return new CommandReault { Result = "进程未就绪", State = CommandReaultType.ProcessNotReady };
            }
        }

     

        public CommandReault ExecuteCommand( string command,  string args) {
           return ExecuteCommand(command, false, args);
        }

        public void Dispose()
        {
            Process.Dispose();
        }

        public Process Process { get; set; }

        public ProcessState State { get; set; }

        public int TaskRunTimes { get; set; }

    

      



    }
}
