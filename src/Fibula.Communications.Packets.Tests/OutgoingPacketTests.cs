﻿// -----------------------------------------------------------------
// <copyright file="OutgoingPacketTests.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Communications.Packets.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Enumerations;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Structures;
    using Fibula.Utilities.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests for packets that implement the <see cref="IOutboundPacket"/> interface.
    /// </summary>
    [TestClass]
    public class OutgoingPacketTests
    {
        /// <summary>
        /// Checks <see cref="AddCreaturePacket"/> initialization.
        /// </summary>
        [TestMethod]
        public void AddCreaturePacket_Initialization()
        {
            const OutboundPacketType ExpectedPacketType = OutboundPacketType.AddThing;
            var creatureMock = new Mock<ICreature>();
            var playerMock = new Mock<IPlayer>();

            ExceptionAssert.Throws<ArgumentException>(() => new AddCreaturePacket(null, creatureMock.Object), "Value cannot be null. (Parameter 'player')");
            ExceptionAssert.Throws<ArgumentException>(() => new AddCreaturePacket(playerMock.Object, null), "Value cannot be null. (Parameter 'creature')");

            var packet = new AddCreaturePacket(playerMock.Object, creatureMock.Object);

            Assert.AreEqual(ExpectedPacketType, packet.PacketType, $"Expected {nameof(packet.PacketType)} to match {ExpectedPacketType}.");
            Assert.AreSame(creatureMock.Object, packet.Creature, $"Expected {nameof(packet.Creature)} to be the same instance as {creatureMock.Object}.");
        }

        /// <summary>
        /// Checks <see cref="AnimatedTextPacket"/> initialization.
        /// </summary>
        [TestMethod]
        public void AnimatedTextPacket_Initialization()
        {
            const OutboundPacketType ExpectedPacketType = OutboundPacketType.AnimatedText;
            const TextColor ExpectedTextColor = TextColor.Green;
            const string ExpectedText = "this is a test!";

            Location expectedLocation = new Location() { X = 100, Y = 150, Z = 7 };

            var packet = new AnimatedTextPacket(expectedLocation, ExpectedTextColor, ExpectedText);

            Assert.AreEqual(ExpectedPacketType, packet.PacketType, $"Expected {nameof(packet.PacketType)} to match {ExpectedPacketType}.");
            Assert.AreEqual(expectedLocation, packet.Location, $"Expected {nameof(packet.Location)} to match {expectedLocation}.");
            Assert.AreEqual(ExpectedTextColor, packet.Color, $"Expected {nameof(packet.Color)} to match {ExpectedTextColor}.");
            Assert.AreEqual(ExpectedText, packet.Text, $"Expected {nameof(packet.Text)} to match {ExpectedText}.");
        }

        /// <summary>
        /// Checks <see cref="CharacterListPacket"/> initialization.
        /// </summary>
        [TestMethod]
        public void CharacterListPacket_Initialization()
        {
            const OutboundPacketType ExpectedPacketType = OutboundPacketType.CharacterList;
            const ushort ExpectedPremiumDays = 7;

            ExceptionAssert.Throws<ArgumentException>(() => new CharacterListPacket(null, ExpectedPremiumDays), "Value cannot be null. (Parameter 'characters')");

            var expectedCharList = new List<CharacterLoginInformation>()
            {
                new CharacterLoginInformation() { Name = "Char 1", Ip = IPAddress.Loopback.GetAddressBytes(), Port = 123, World = "Fibula" },
                new CharacterLoginInformation() { Name = "Char 2", Ip = IPAddress.Any.GetAddressBytes(), Port = 321, World = "Fibula 2" },
            };

            var packet = new CharacterListPacket(expectedCharList, ExpectedPremiumDays);

            Assert.AreEqual(ExpectedPacketType, packet.PacketType, $"Expected {nameof(packet.PacketType)} to match {ExpectedPacketType}.");

            Assert.IsNotNull(packet.Characters);
            CollectionAssert.AreEqual(expectedCharList, packet.Characters.ToArray(), $"Expected {nameof(packet.Characters)} to match {expectedCharList}.");

            Assert.AreEqual(ExpectedPremiumDays, packet.PremiumDaysLeft, $"Expected {nameof(packet.PremiumDaysLeft)} to match {ExpectedPremiumDays}.");
        }
    }
}
