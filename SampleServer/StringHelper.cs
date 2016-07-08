using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleServer
{
    public static class StringHelper
    {
        public  static string WithEndingSlash(this string baseUrl)
        {
            if (baseUrl.EndsWith("/") == false)
            {
                baseUrl = baseUrl + "/";
            }
            return baseUrl;
        }

    }
}
