﻿// -----------------------------------------------------------------
// <copyright file="SocketConnection_v772.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Protocol.V772
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using Fibula.Communications;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Contracts.Delegates;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Enumerations;
    using Fibula.Protocol.Contracts.Abstractions;
    using Fibula.Protocol.V772.Extensions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a standard 7.72 client connection over a TCP socket.
    /// </summary>
    public class SocketConnection_v772 : ISocketConnection
    {
        /// <summary>
        /// A lock used to sempahore writes to the connection's stream.
        /// </summary>
        private readonly object writeLock;

        /// <summary>
        /// The socket of this connection.
        /// </summary>
        private readonly Socket socket;

        /// <summary>
        /// This connection's stream.
        /// </summary>
        private readonly NetworkStream stream;

        /// <summary>
        /// Gets a reference to the inbound network message.
        /// </summary>
        private readonly INetworkMessage inboundMessage;

        /// <summary>
        /// Gets a reference to the logger in use.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Stores the protocol in use by this connection.
        /// </summary>
        private readonly IProtocol protocol;

        /// <summary>
        /// A value indicating whether this connection has been authenticated.
        /// </summary>
        private bool isAuthenticated;

        /// <summary>
        /// The XTea key used for all communications through this connection.
        /// </summary>
        private uint[] xteaKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketConnection_v772"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="socket">The socket that this connection is for.</param>
        /// <param name="protocol">The protocol in use by this connection.</param>
        public SocketConnection_v772(ILogger logger, Socket socket, IProtocol protocol)
        {
            logger.ThrowIfNull(nameof(logger));
            socket.ThrowIfNull(nameof(socket));
            protocol.ThrowIfNull(nameof(protocol));

            this.writeLock = new object();
            this.socket = socket;
            this.stream = new NetworkStream(this.socket);

            this.inboundMessage = new NetworkMessage(isOutbound: false);
            this.xteaKey = new uint[4];
            this.isAuthenticated = false;

            this.logger = logger;
            this.protocol = protocol;
        }

        /// <summary>
        /// Event fired when this connection has been closed.
        /// </summary>
        public event ConnectionClosedHandler Closed;

        /// <summary>
        /// Event fired right after this connection has proccessed an <see cref="IInboundPacket"/> by any subscriber of the <see cref="PacketReady"/> event.
        /// </summary>
        public event ConnectionPacketProccessedHandler PacketProcessed;

        /// <summary>
        /// Event fired when a <see cref="IInboundPacket"/> picked up by this connection is ready to be processed.
        /// </summary>
        public event ConnectionPacketReadyHandler PacketReady;

        /// <summary>
        /// Gets the Socket IP address of this connection, if it is open.
        /// </summary>
        public string SocketIp
        {
            get
            {
                return this.socket?.RemoteEndPoint?.ToString();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the connection is an orphan.
        /// </summary>
        public bool IsOrphaned
        {
            get
            {
                return !this.socket?.Connected ?? false;
            }
        }

        /// <summary>
        /// Reads from this connection's stream.
        /// </summary>
        public void Read()
        {
            if (this.stream.CanRead)
            {
                var header = new byte[2];

                this.stream.BeginRead(header, 0, 2, this.OnDataReady, header);
            }
        }

        /// <summary>
        /// Closes this connection.
        /// </summary>
        public void Close()
        {
            this.stream.Close();
            this.socket.Close();

            // Tells the subscribers of this event that this connection has been closed.
            this.Closed?.Invoke(this);
        }

        /// <summary>
        /// Prepares a <see cref="INetworkMessage"/> with the reponse packets supplied and sends it.
        /// </summary>
        /// <param name="responsePackets">The packets that compose that response.</param>
        /// <param name="playerId">Optional. The id of the player attached to the connection, for context.</param>
        public void Send(IEnumerable<IOutboundPacket> responsePackets, uint playerId = 0)
        {
            if (responsePackets == null || !responsePackets.Any() || this.IsOrphaned)
            {
                return;
            }

            INetworkMessage outboundMessage = new NetworkMessage();

            int readAlready = 4; // 4 to skip the length bytes.

            foreach (var outPacket in responsePackets)
            {
                var writer = this.protocol.SelectPacketWriter(outPacket.PacketType);

                if (writer == null)
                {
                    this.logger.LogWarning($"Unsupported response packet type {outPacket.PacketType} without a writer. Packet was not added to the message.");

                    continue;
                }

                writer.WriteToMessage(outPacket, ref outboundMessage);

                var thisPacketLen = outboundMessage.Length - readAlready;
                var packetBytes = outboundMessage.Buffer.Slice(readAlready, thisPacketLen)
                                                        .ToArray()
                                                        .Select(b => b.ToString("X2")).Aggregate((str, e) => str += " " + e);

                this.logger.LogTrace($"Message bytes added by packet {outPacket.GetType().Name}: {packetBytes}");

                readAlready += thisPacketLen;
            }

            this.Send(outboundMessage);
        }

        /// <summary>
        /// Sets up an Xtea key expected to be matched on subsequent messages.
        /// </summary>
        /// <param name="xteaKey">The XTea key to use in this connection's communications.</param>
        public void SetupAuthenticationKey(uint[] xteaKey)
        {
            this.xteaKey = xteaKey;
            this.isAuthenticated = true;
        }

        private void OnDataReady(IAsyncResult ar)
        {
            if (!this.CompleteRead(ar))
            {
                return;
            }

            try
            {
                if (this.stream.CanRead)
                {
                    if (this.stream.DataAvailable)
                    {
                        // Read the message size from ar.AsyncState, which is the 2 byte header
                        // initially read from the pipe after the await period.
                        var messageSize = BitConverter.ToUInt16(ar.AsyncState as byte[]);

                        this.inboundMessage.Resize(messageSize);
                        this.inboundMessage.ReadBytesFromStream(this.stream, messageSize);

                        if (this.isAuthenticated)
                        {
                            // Decrypt message using XTea
                            this.inboundMessage.XteaDecrypt(this.xteaKey);

                            // Read the packet length to advance the cursor.
                            this.inboundMessage.GetUInt16();
                        }

                        var packetType = this.protocol.ByteToIncomingPacketType(this.inboundMessage.GetByte());

                        var reader = this.protocol.SelectPacketReader(packetType);

                        if (reader == null)
                        {
                            if (Enum.IsDefined(typeof(InboundPacketType), packetType))
                            {
                                this.logger.LogWarning($"No reader found that supports type '{(InboundPacketType)packetType}' of packets. Selecting default reader...");
                            }
                            else
                            {
                                this.logger.LogWarning($"No reader found that supports type '{packetType}' of packets. Selecting default reader...");
                            }

                            reader = new DefaultPacketReader(this.logger);
                        }

                        var packetRead = reader.ReadFromMessage(this.inboundMessage);

                        if (packetRead == null)
                        {
                            this.logger.LogError($"Could not read data using reader '{reader.GetType().Name}'.");
                        }
                        else
                        {
                            var responsePackets = this.PacketReady?.Invoke(this, packetRead);

                            if (responsePackets != null && responsePackets.Any())
                            {
                                // Send any response packets prepared.
                                this.Send(responsePackets);
                            }
                        }

                        this.inboundMessage.Reset();
                    }
                }
            }
            catch (Exception e)
            {
                // Invalid data from the client
                this.logger.LogWarning(e.ToString());
            }
            finally
            {
                this.PacketProcessed?.Invoke(this);
            }
        }

        private bool CompleteRead(IAsyncResult ar)
        {
            try
            {
                int read = this.stream.CanRead ? this.stream.EndRead(ar) : 0;

                if (read == 0)
                {
                    // client disconnected
                    this.Close();

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                this.logger.LogError(e.ToString());

                // TODO: is closing the connection really necesary?
                this.Close();
            }

            return false;
        }

        private void Send(INetworkMessage message)
        {
            message.PrepareToSend(this.xteaKey);

            try
            {
                lock (this.writeLock)
                {
                    // limit to the prepared length of the message only.
                    var spanToSend = message.Buffer[0..message.Length];

                    this.stream.Write(spanToSend);
                }
            }
            catch (ObjectDisposedException e)
            {
                this.logger.LogError(e.ToString());

                this.Close();
            }
        }
    }
}
