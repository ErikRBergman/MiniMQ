﻿using System;

namespace MiniMQ.Core.Message
{
    using System.IO;
    using System.Threading.Tasks;

    using MiniMQ.Model.Core.Message;

    public class Message : IMessage
    {
        private readonly Stream stream;

        public Message(Stream stream)
        {
            this.stream = stream;
            this.UniqueIdentifier = GetNewMessageId();
        }

        private static string GetNewMessageId() => Guid.NewGuid().ToString("N");

        public Message(Stream stream, string uniqueId)
        {
            this.stream = stream;
            this.UniqueIdentifier = uniqueId ?? GetNewMessageId();
        }

        public Task<Stream> GetStream()
        {
            return Task.FromResult(this.stream);
        }

        public string UniqueIdentifier { get; }
    }
}