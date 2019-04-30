using RPool.NET.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPool.NET
{

    /// <summary>
    ///     进程状态
    /// </summary>
    public enum ProcessState
    {
        /// <summary>
        ///     初始化中
        /// </summary>
        Initializing,
        /// <summary>
        ///     就绪，等待任务
        /// </summary>
        Ready,
        /// <summary>
        ///     忙
        /// </summary>
        Busy,
        /// <summary>
        ///     准备释放
        /// </summary>
        ByeBye
    }
}
