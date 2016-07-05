﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Client.Model
{
    using MiniMQ.Client.Implementation;

    internal interface IReactiveClientConnection
    {
        void MessageReceiveDone(ReactiveClientInputStream stream);
    }
}
