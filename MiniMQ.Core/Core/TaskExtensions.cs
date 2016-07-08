using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Core.Core
{
    internal static class TaskExt
    {
        public static Task<bool> TrueTask { get; } = Task.FromResult(true);
        public static Task<bool> FalseTask { get; } = Task.FromResult(false);
    }
}
