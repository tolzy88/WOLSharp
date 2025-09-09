//
// Authors:
//   Steven Tolzmann
//
// Copyright (C) 2025 Steven Tolzmann

using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using WOLSharp.Sockets;

namespace WOLSharp
{
    /// <summary>
    /// A static class that provides one-and-done methods for sending Wake-on-LAN (WOL) magic packets to wake up computers on a network.
    /// If you are using these methods in quick succession, consider using the <see cref="WOLSocket"/> class instead, to avoid socket exhaustion.
    /// </summary>
    /// <remarks>
    /// Each call creates and disposes a <see cref="WOLSocket"/>. For bulk operations, prefer reusing a single <see cref="WOLSocket"/> instance.
    /// </remarks>
    public static class WakeOnLan
    {
        /// <summary>
        /// Broadcasts a Wake-on-LAN magic packet to the specified MAC addresses.
        /// </summary>
        /// <param name="macAddresses">
        /// MAC addresses to wake in string format (each parsed via <see cref="PhysicalAddress.Parse(string)"/>).
        /// Accepted formats depend on the target runtime; on .NET Standard 2.0 the supported formats are typically
        /// "AABBCCDDEEFF" and "AA-BB-CC-DD-EE-FF".
        /// </param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="macAddresses"/> is <see langword="null"/> or contains a <see langword="null"/> element.</exception>
        /// <exception cref="System.FormatException">Thrown when any element is not a valid MAC address string.</exception>
        /// <exception cref="System.Net.Sockets.SocketException">Thrown when a socket error occurs while sending.</exception>
        public static void Broadcast(IEnumerable<string> macAddresses)
        {
            using (var socket = new WOLSocket())
            {
                socket.Broadcast(macAddresses);
            }
        }

        /// <summary>
        /// Asynchronously broadcasts a Wake-on-LAN magic packet to the specified MAC addresses.
        /// </summary>
        /// <param name="macAddresses">
        /// MAC addresses to wake in string format (each parsed via <see cref="PhysicalAddress.Parse(string)"/>).
        /// Accepted formats depend on the target runtime; on .NET Standard 2.0 the supported formats are typically
        /// "AABBCCDDEEFF" and "AA-BB-CC-DD-EE-FF".
        /// </param>
        /// <returns>A task that completes when all sends have finished.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="macAddresses"/> is <see langword="null"/> or contains a <see langword="null"/> element.</exception>
        /// <exception cref="System.FormatException">Thrown when any element is not a valid MAC address string.</exception>
        /// <exception cref="System.Net.Sockets.SocketException">Thrown when a socket error occurs while sending.</exception>
        public static async Task BroadcastAsync(IEnumerable<string> macAddresses)
        {
            using (var socket = new WOLSocket())
            {
                await socket.BroadcastAsync(macAddresses).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Broadcasts a Wake-on-LAN magic packet to the specified MAC address.
        /// </summary>
        /// <param name="macAddress">
        /// MAC address to wake in string format (parsed via <see cref="PhysicalAddress.Parse(string)"/>).
        /// Accepted formats depend on the target runtime; on .NET Standard 2.0 the supported formats are typically
        /// "AABBCCDDEEFF" and "AA-BB-CC-DD-EE-FF".
        /// </param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="macAddress"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.FormatException">Thrown when <paramref name="macAddress"/> is not a valid MAC address string.</exception>
        /// <exception cref="System.Net.Sockets.SocketException">Thrown when a socket error occurs while sending.</exception>
        public static void Broadcast(string macAddress)
        {
            using (var socket = new WOLSocket())
            {
                socket.Broadcast(macAddress);
            }
        }

        /// <summary>
        /// Asynchronously broadcasts a Wake-on-LAN magic packet to the specified MAC address.
        /// </summary>
        /// <param name="macAddress">
        /// MAC address to wake in string format (parsed via <see cref="PhysicalAddress.Parse(string)"/>).
        /// Accepted formats depend on the target runtime; on .NET Standard 2.0 the supported formats are typically
        /// "AABBCCDDEEFF" and "AA-BB-CC-DD-EE-FF".
        /// </param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="macAddress"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.FormatException">Thrown when <paramref name="macAddress"/> is not a valid MAC address string.</exception>
        /// <exception cref="System.Net.Sockets.SocketException">Thrown when a socket error occurs while sending.</exception>
        public static async Task BroadcastAsync(string macAddress)
        {
            using (var socket = new WOLSocket())
            {
                await socket.BroadcastAsync(macAddress).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Broadcasts a Wake-on-LAN magic packet to the specified MAC address.
        /// </summary>
        /// <param name="macAddress">MAC address to wake.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="macAddress"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.Net.Sockets.SocketException">Thrown when a socket error occurs while sending.</exception>
        public static void Broadcast(PhysicalAddress macAddress)
        {
            using (var socket = new WOLSocket())
            {
                socket.Broadcast(macAddress);
            }
        }

        /// <summary>
        /// Broadcasts a Wake-on-LAN magic packet to the specified MAC addresses.
        /// </summary>
        /// <param name="macAddresses">MAC addresses to wake.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="macAddresses"/> is <see langword="null"/> or contains a <see langword="null"/> element.</exception>
        /// <exception cref="System.Net.Sockets.SocketException">Thrown when a socket error occurs while sending.</exception>
        public static void Broadcast(IEnumerable<PhysicalAddress> macAddresses)
        {
            using (var socket = new WOLSocket())
            {
                socket.Broadcast(macAddresses);
            }
        }

        /// <summary>
        /// Asynchronously broadcasts a Wake-on-LAN magic packet to the specified MAC address.
        /// </summary>
        /// <param name="macAddress">MAC address to wake.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="macAddress"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.Net.Sockets.SocketException">Thrown when a socket error occurs while sending.</exception>
        public static async Task BroadcastAsync(PhysicalAddress macAddress)
        {
            using (var socket = new WOLSocket())
            {
                await socket.BroadcastAsync(macAddress).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Asynchronously broadcasts a Wake-on-LAN magic packet to the specified MAC addresses.
        /// </summary>
        /// <param name="macAddresses">MAC addresses to wake.</param>
        /// <returns>A task that completes when all sends have finished.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="macAddresses"/> is <see langword="null"/> or contains a <see langword="null"/> element.</exception>
        /// <exception cref="System.Net.Sockets.SocketException">Thrown when a socket error occurs while sending.</exception>
        public static async Task BroadcastAsync(IEnumerable<PhysicalAddress> macAddresses)
        {
            using (var socket = new WOLSocket())
            {
                await socket.BroadcastAsync(macAddresses).ConfigureAwait(false);
            }
        }
    }
}
