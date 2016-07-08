using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Client.Implementation
{
    public static class ArraySegmentExtensions
    {
        public static ArraySegment<T> AsArraySegment<T>(this T[] array)
        {
            return new ArraySegment<T>(array);
        }

        public static ArraySegment<T> AsArraySegment<T>(this T[] array, int offset, int count)
        {
            return new ArraySegment<T>(array, offset, count);
        }

    }
}
