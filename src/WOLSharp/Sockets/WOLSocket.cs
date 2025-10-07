//
// Authors:
//   Steven Tolzmann
//
// Copyright (C) 2025 Steven Tolzmann

using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace WOLSharp.Sockets
{
    /// <summary>
    /// A class that provides methods for sending Wake-on-LAN (WOL) magic packets to wake up computers on a network.
    /// Encapsulates a <see cref="Socket"/> object for sending UDP packets.
    /// </summary>
    /// <remarks>
    /// This socket is configured for IPv4 UDP broadcast and will attempt to send the magic packet to the broadcast
    /// address on common WOL ports (0, 7, and 9).
    /// </remarks>
    public class WOLSocket : Socket
    {
        private static readonly IPEndPoint _endpoint = new IPEndPoint(IPAddress.Broadcast, 9); // Default WOL Endpoint (UDP Port 9) as recognized by most devices/Wireshark

        /// <summary>
        /// Initializes a new instance of the <see cref="WOLSocket"/> class for IPv4 UDP broadcast.
        /// </summary>
        /// <remarks>
        /// The socket is created with <see cref="SocketType.Dgram"/> and <see cref="ProtocolType.Udp"/> and has
        /// <see cref="Socket.EnableBroadcast"/> set to <see langword="true"/> for compatibility with broadcast scenarios.
        /// </remarks>
        public WOLSocket() : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            EnableBroadcast = true; // Enable broadcast, required for macOS compatibility
        }

        /// <summary>
        /// Broadcasts a Wake-on-LAN magic packet to the specified MAC address.
        /// </summary>
        /// <param name="macAddress">
        /// MAC Address to wake in string format (parsed via <see cref="PhysicalAddress.Parse(string)"/>).
        /// Accepted formats depend on the target runtime; on .NET Standard 2.0 the supported formats are typically
        /// "AABBCCDDEEFF" and "AA-BB-CC-DD-EE-FF".
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="macAddress"/> is <see langword="null"/>.</exception>
        /// <exception cref="FormatException">Thrown when <paramref name="macAddress"/> is not a valid MAC address string.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the socket has been disposed.</exception>
        /// <exception cref="SocketException">Thrown when a socket error occurs while sending.</exception>
        public void Broadcast(string macAddress)
        {
            var physicalAddress = PhysicalAddress.Parse(macAddress);
            Broadcast(physicalAddress);
        }

        /// <summary>
        /// Broadcasts a Wake-on-LAN magic packet to the specified MAC addresses.
        /// </summary>
        /// <param name="macAddresses">
        /// MAC Addresses to wake in string format (each parsed via <see cref="PhysicalAddress.Parse(string)"/>).
        /// Accepted formats depend on the target runtime; on .NET Standard 2.0 the supported formats are typically
        /// "AABBCCDDEEFF" and "AA-BB-CC-DD-EE-FF".
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="macAddresses"/> is <see langword="null"/> or contains a <see langword="null"/> element.</exception>
        /// <exception cref="FormatException">Thrown when any element is not a valid MAC address string.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the socket has been disposed.</exception>
        /// <exception cref="SocketException">Thrown when a socket error occurs while sending.</exception>
        public void Broadcast(IEnumerable<string> macAddresses)
        {
            var physicalAddresses = macAddresses
                .Select(mac => PhysicalAddress.Parse(mac));
            Broadcast(physicalAddresses);
        }

        /// <summary>
        /// Asynchronously broadcasts a Wake-on-LAN magic packet to the specified MAC address.
        /// </summary>
        /// <param name="macAddress">
        /// MAC Address to wake in string format (parsed via <see cref="PhysicalAddress.Parse(string)"/>).
        /// Accepted formats depend on the target runtime; on .NET Standard 2.0 the supported formats are typically
        /// "AABBCCDDEEFF" and "AA-BB-CC-DD-EE-FF".
        /// </param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="macAddress"/> is <see langword="null"/>.</exception>
        /// <exception cref="FormatException">Thrown when <paramref name="macAddress"/> is not a valid MAC address string.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the socket has been disposed.</exception>
        /// <exception cref="SocketException">Thrown when a socket error occurs while sending.</exception>
        public async Task BroadcastAsync(string macAddress)
        {
            var physicalAddress = PhysicalAddress.Parse(macAddress);
            await BroadcastAsync(physicalAddress).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously broadcasts a Wake-on-LAN magic packet to the specified MAC address.
        /// </summary>
        /// <param name="macAddress">MAC Address to wake.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="macAddress"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the socket has been disposed.</exception>
        /// <exception cref="SocketException">Thrown when a socket error occurs while sending.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task BroadcastAsync(PhysicalAddress macAddress) =>
            await BroadcastAsync_Internal(macAddress).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously broadcasts a Wake-on-LAN magic packet to the specified MAC addresses.
        /// </summary>
        /// <param name="macAddresses">
        /// MAC Addresses to wake in string format (each parsed via <see cref="PhysicalAddress.Parse(string)"/>).
        /// Accepted formats depend on the target runtime; on .NET Standard 2.0 the supported formats are typically
        /// "AABBCCDDEEFF" and "AA-BB-CC-DD-EE-FF".
        /// </param>
        /// <returns>A task that completes when all sends have finished.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="macAddresses"/> is <see langword="null"/> or contains a <see langword="null"/> element.</exception>
        /// <exception cref="FormatException">Thrown when any element is not a valid MAC address string.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the socket has been disposed.</exception>
        /// <exception cref="SocketException">Thrown when a socket error occurs while sending.</exception>
        public async Task BroadcastAsync(IEnumerable<string> macAddresses)
        {
            var physicalAddresses = macAddresses
                .Select(mac => PhysicalAddress.Parse(mac));
            await BroadcastAsync(physicalAddresses).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously broadcasts a Wake-on-LAN magic packet to the specified MAC addresses.
        /// </summary>
        /// <param name="macAddresses">MAC Addresses to wake.</param>
        /// <returns>A task that completes when all sends have finished.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="macAddresses"/> is <see langword="null"/> or contains a <see langword="null"/> element.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the socket has been disposed.</exception>
        /// <exception cref="SocketException">Thrown when a socket error occurs while sending.</exception>
        public async Task BroadcastAsync(IEnumerable<PhysicalAddress> macAddresses)
        {
            var tasks = new List<Task>();
            foreach (var macAddress in macAddresses)
            {
                tasks.Add(BroadcastAsync_Internal(macAddress));
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        #region Core Functionality

        /// <summary>
        /// Broadcasts a Wake-on-LAN magic packet to the specified MAC address.
        /// </summary>
        /// <param name="macAddress">MAC Address to wake.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="macAddress"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the socket has been disposed.</exception>
        /// <exception cref="SocketException">Thrown when a socket error occurs while sending.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Broadcast(PhysicalAddress macAddress) =>
            Broadcast_Internal(macAddress);

        /// <summary>
        /// Broadcasts a Wake-on-LAN magic packet to the specified MAC addresses.
        /// </summary>
        /// <param name="macAddresses">MAC Addresses to wake.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="macAddresses"/> is <see langword="null"/> or contains a <see langword="null"/> element.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the socket has been disposed.</exception>
        /// <exception cref="SocketException">Thrown when a socket error occurs while sending.</exception>
        public void Broadcast(IEnumerable<PhysicalAddress> macAddresses)
        {
            foreach (var macAddress in macAddresses)
            {
                Broadcast_Internal(macAddress);
            }
        }

        private void Broadcast_Internal(PhysicalAddress mac)
        {
            byte[] magicPacket = BuildMagicPacket(mac); // Get magic packet byte array based on MAC Address
            this.SendTo(magicPacket, _endpoint); // Broadcast magic packet
        }

        private async Task BroadcastAsync_Internal(PhysicalAddress mac)
        {
            byte[] magicPacket = BuildMagicPacket(mac); // Get magic packet byte array based on MAC Address
            using (var cts = new CancellationTokenSource(delay: TimeSpan.FromSeconds(3))) // Timeout after 3 seconds -> UDP doesn't wait for a response, so this should never happen
            {
                await this.SendToAsync(
                    buffer: magicPacket,
                    remoteEP: _endpoint,
                    cancellationToken: cts.Token)
                    .ConfigureAwait(false); // Broadcast magic packet asynchronously
            }
        }

        private static byte[] BuildMagicPacket(PhysicalAddress macAddress)
        {
            byte[] macBytes = macAddress.GetAddressBytes(); // Convert 48 bit MAC Address to array of bytes
            if (macBytes.Length != 6)
                throw new FormatException("MAC Address must be 6 bytes (48 bits) long.");
            byte[] magicPacket = new byte[102];
            for (int i = 0; i < 6; i++) // 0xFF 6 times
            {
                magicPacket[i] = 0xFF;
            }
            for (int i = 6; i < 102; i += 6) // 16 times MAC Address
            {
                Buffer.BlockCopy(macBytes, 0, magicPacket, i, 6);
            }
            return magicPacket; // 102 Byte Magic Packet
        }

        #endregion
    }
}
