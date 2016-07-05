using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Core.Core.Message.Pool
{
    using System.Threading;

    using MiniMQ.Model.Core;
    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;
    public class LoggingMessageHandler : IMessageHandler
    {
        private readonly ILog log;

        private readonly IMessageHandler innerMessageHandler;

        private long messageNumber = 0;

        public LoggingMessageHandler(ILog log, IMessageHandler innerMessageHandler)
        {
            this.log = log;
            this.innerMessageHandler = innerMessageHandler;
        }

        public string Name => this.innerMessageHandler.Name;

        public bool SupportsSendAndReceiveMessage => this.innerMessageHandler.SupportsSendAndReceiveMessage;

        public bool SupportsWebSocketConnections => true;

        public IMessageFactory GetMessageFactory()
        {
            return this.innerMessageHandler.GetMessageFactory();
        }

        public async Task ReceiveMessageAsync(IMessagePipeline pipeline, CancellationToken cancellationToken)
        {
            try
            {
                this.log.Log(LogType.Verbose, "ReceiveMessgeAsync started...");
                var currentMessageNumber = Interlocked.Increment(ref this.messageNumber);
                var loggingPipeline = new LoggingMessagePipeline("ReceiveMessageAsync", this.log, pipeline, currentMessageNumber);
                await this.innerMessageHandler.ReceiveMessageAsync(loggingPipeline, cancellationToken);
            }
            catch (Exception exception)
            {
                this.log.Log(LogType.Error, "ReceiveMessageAstbc threw and exception: " + exception.ToString());
            }
            finally
            {
                this.log.Log(LogType.Verbose, "ReceiveMessgeAsync ended...");
            }
        }

        public IMessage ReceiveMessageOrNull()
        {
            throw new NotImplementedException();
        }

        public Task SendAndReceiveMessageAsync(IMessage message, IMessagePipeline pipeline, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SendMessage(IMessage message)
        {
            throw new NotImplementedException();
        }

        private struct LoggingMessagePipeline : IMessagePipeline
        {
            private readonly string actionName;

            private readonly ILog log;

            private readonly IMessagePipeline innerMessagePipeline;

            private readonly long messageNumber;

            public LoggingMessagePipeline(string actionName, ILog log, IMessagePipeline innerMessagePipeline, long messageNumber)
            {
                this.actionName = actionName;
                this.log = log;
                this.innerMessagePipeline = innerMessagePipeline;
                this.messageNumber = messageNumber;
            }

            public async Task SendMessage(IMessage message)
            {
                try
                {
                    this.log.Log(LogType.Verbose, "IMessagePipeline: " + this.actionName + ": SendMessage, before call " + message.UniqueIdentifier);
                    await this.innerMessagePipeline.SendMessage(message);
                }
                catch (Exception exception)
                {
                    this.log.Log(LogType.Error, "IMessagePipeline: " +  this.actionName + ": SendMessage threw and exception: " + exception.ToString());
                }
                finally
                {
                    this.log.Log(LogType.Verbose, "IMessagePipeline: " + this.actionName + ": SendMessage done" + message.UniqueIdentifier);
                }
            }
        }
    }
}
