﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SQLIO2.Protocols
{
    class Sc500Protocol : ProtocolBase
    {
        private readonly RequestDelegate _stack;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;
        private static readonly byte[] EndBytes = Encoding.UTF8.GetBytes("</msg>");

        public Sc500Protocol(RequestDelegate stack, IServiceScopeFactory serviceScopeFactory, ILogger<Sc500Protocol> logger) : base(logger)
        {
            _stack = stack;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override SequencePosition Read(TcpClient client, in ReadOnlySequence<byte> sequence)
        {
            var reader = new SequenceReader<byte>(sequence);

            while (reader.TryReadTo(out ReadOnlySequence<byte> xmlBytes, EndBytes, advancePastDelimiter: true))
            {
                var xmlBytesWithEndBytes = reader.Sequence.Slice(xmlBytes.Start, xmlBytes.Length + EndBytes.Length);
                ProcessXml(client, xmlBytesWithEndBytes);
            }

            return reader.Position;
        }

        private void ProcessXml(TcpClient client, ReadOnlySequence<byte> xml)
        {
            var msg = new XmlDocument();
            msg.LoadXml(Encoding.UTF8.GetString(xml.ToArray()));

            //RunStack(client, xml);
        }

        protected void RunStack(TcpClient client, XmlDocument xml)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        //var packet = new Packet(scope.ServiceProvider, client, xmlBytes);

                        //_logger.LogInformation("Handling packet {Xml} from {RemoteEndpoint}", xml, client.Client.RemoteEndPoint);

                        //await _stack(packet);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unknown stack error while handling packet from {RemoteEndpoint}", client.Client.RemoteEndPoint);
                }
            });
        }
    }
}
