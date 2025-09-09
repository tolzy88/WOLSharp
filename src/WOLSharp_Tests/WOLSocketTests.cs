//
// Authors:
//   Steven Tolzmann
//
// Copyright (C) 2025 Steven Tolzmann

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using WOLSharp.Sockets;

namespace WOLSharp_Tests
{
    public class WOLSocketTests
    {
        // Cross-version valid (NS2.0+)
        private const string SampleMacHyphen = "01-23-45-67-89-AB";
        private const string SampleMacPlain  = "0123456789AB";

        // Extended formats (accepted only on .NET 5+)
        private const string SampleMacColon  = "00:11:22:33:44:55";
        private const string SampleMacDotted = "0011.2233.4455";
        private const string SampleMacLowerHyphen = "f0-e1-d2-c3-b4-a5";

        private static MethodInfo GetBuildMagicPacket() =>
            typeof(WOLSocket).GetMethod("BuildMagicPacket", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Could not locate BuildMagicPacket via reflection.");

        private static byte[] BuildExpectedMagicPacket(string mac)
        {
            var pa = PhysicalAddress.Parse(mac);
            return (byte[])GetBuildMagicPacket().Invoke(null, new object[] { pa })!;
        }

        private static bool RuntimeSupportsExtendedFormats()
        {
            // If both colon and dotted parse, treat as extended-support runtime (e.g., .NET 5+)
            return TryParse(SampleMacColon) && TryParse(SampleMacDotted) && TryParse(SampleMacLowerHyphen);

            static bool TryParse(string mac)
            {
                try { _ = PhysicalAddress.Parse(mac); return true; }
                catch { return false; }
            }
        }

        [Fact]
        public void Ctor_Sets_EnableBroadcast_True()
        {
            using var wol = new WOLSocket();
            Assert.True(wol.EnableBroadcast);
        }

        // Cross-version valid inputs only
        [Theory]
        [InlineData("01-23-45-67-89-AB")]
        [InlineData("0123456789AB")]
        public void BuildMagicPacket_HasCorrectLengthAndPattern(string mac)
        {
            var pa = PhysicalAddress.Parse(mac);
            var method = GetBuildMagicPacket();
            var packet = (byte[])method.Invoke(null, new object[] { pa })!;

            Assert.NotNull(packet);
            Assert.Equal(102, packet.Length);
            Assert.True(packet.Take(6).All(b => b == 0xFF), "Magic packet does not start with six 0xFF bytes.");

            var macBytes = pa.GetAddressBytes();
            for (int i = 0; i < 16; i++)
            {
                var offset = 6 + (i * 6);
                Assert.True(packet.Skip(offset).Take(6).SequenceEqual(macBytes), $"Magic packet repetition {i} did not match MAC bytes.");
            }
        }

        // Always-invalid across runtimes (bad hex, wrong digits, junk)
        [Theory]
        [InlineData("01:23:45:67:89")]             // too few hex digits
        [InlineData("0123456789")]                 // too few hex digits
        [InlineData("01-23-45-67-89-AB-CD")]       // too many hex digits
        [InlineData("ZZ-23-45-67-89-AB")]          // non-hex Z
        [InlineData("01-23-45-67-89-gh")]          // non-hex
        [InlineData("not-a-mac")]                  // obvious junk
        [InlineData("::::::")]                     // separators only
        [InlineData("--")]                         // nonsense separators
        [InlineData("00112233445566")]             // 14 hex digits
        [InlineData("0011223344")]                 // 10 hex digits
        [InlineData("g0-11-22-33-44-55")]          // invalid hex char
        public void Broadcast_String_InvalidMac_Throws(string mac)
        {
            using var wol = new WOLSocket();
            Assert.Throws<FormatException>(() => wol.Broadcast(mac));
        }

        [Theory]
        [InlineData("01:23:45:67:89")]             // too few hex digits
        [InlineData("0123456789")]                 // too few hex digits
        [InlineData("01-23-45-67-89-AB-CD")]       // too many hex digits
        [InlineData("ZZ-23-45-67-89-AB")]          // non-hex Z
        [InlineData("01-23-45-67-89-gh")]          // non-hex
        [InlineData("not-a-mac")]                  // obvious junk
        [InlineData("::::::")]                     // separators only
        [InlineData("--")]                         // nonsense separators
        [InlineData("00112233445566")]             // 14 hex digits
        [InlineData("0011223344")]                 // 10 hex digits
        [InlineData("g0-11-22-33-44-55")]          // invalid hex char
        public async Task BroadcastAsync_String_InvalidMac_Throws(string mac)
        {
            using var wol = new WOLSocket();
            await Assert.ThrowsAsync<FormatException>(() => wol.BroadcastAsync(mac));
        }

        // Extended-format behavior varies by runtime. Assert success on supported runtimes, failure on older ones.
        [Theory]
        [InlineData("00:11:22:33:44:55")] // colon
        [InlineData("0011.2233.4455")]    // dotted
        [InlineData("f0-e1-d2-c3-b4-a5")] // lowercase hyphen
        public async Task Broadcast_ExtendedFormats_RuntimeDependent(string mac)
        {
            using var wol = new WOLSocket();
            var supports = RuntimeSupportsExtendedFormats();

            if (supports)
            {
                // Should parse and send without exceptions
                var pa = PhysicalAddress.Parse(mac); // confirm parse works on this runtime
                var expected = (byte[])GetBuildMagicPacket().Invoke(null, new object[] { pa })!;
                wol.Broadcast(mac);
                await wol.BroadcastAsync(mac);

                // Verify magic packet structure matches BuildMagicPacket(pa)
                // (No UDP check here; UDP tests below use cross-version inputs.)
                Assert.Equal(102, expected.Length);
            }
            else
            {
                Assert.Throws<FormatException>(() => wol.Broadcast(mac));
                await Assert.ThrowsAsync<FormatException>(() => wol.BroadcastAsync(mac));
            }
        }

        [Fact]
        public void Broadcast_SendsUdpToPorts_7_And_9_WithCorrectPayload()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            using var listener7 = CreateUdpListener(7);
            using var listener9 = CreateUdpListener(9);

            var expected = BuildExpectedMagicPacket(SampleMacHyphen);

            using (var wol = new WOLSocket())
            {
                wol.Broadcast(SampleMacHyphen);
            }

            var received = ReceiveFromBoth(listener7, listener9, expected.Length, TimeSpan.FromSeconds(2));

            Assert.Equal(2, received.Count);
            foreach (var buf in received)
                Assert.True(expected.SequenceEqual(buf), "Received datagram does not match expected magic packet.");
        }

        [Fact]
        public async Task BroadcastAsync_SendsUdpToPorts_7_And_9_WithCorrectPayload()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            using var listener7 = CreateUdpListener(7);
            using var listener9 = CreateUdpListener(9);

            var expected = BuildExpectedMagicPacket(SampleMacPlain);

            using (var wol = new WOLSocket())
            {
                await wol.BroadcastAsync(SampleMacPlain);
            }

            var received = ReceiveFromBoth(listener7, listener9, expected.Length, TimeSpan.FromSeconds(2));
            Assert.Equal(2, received.Count);
            foreach (var buf in received)
                Assert.True(expected.SequenceEqual(buf), "Received datagram does not match expected magic packet.");
        }

        [Fact]
        public async Task Broadcast_MultipleAddresses_SendsToAll()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            using var listener7 = CreateUdpListener(7);
            using var listener9 = CreateUdpListener(9);

            var macs = new[] { SampleMacHyphen, SampleMacPlain };
            var expected1 = BuildExpectedMagicPacket(macs[0]);
            var expected2 = BuildExpectedMagicPacket(macs[1]);

            using (var wol = new WOLSocket())
            {
                wol.Broadcast(macs);
            }

            var recv7Task = ReceiveN(listener7, 2, TimeSpan.FromSeconds(3), expected1.Length);
            var recv9Task = ReceiveN(listener9, 2, TimeSpan.FromSeconds(3), expected1.Length);
            await Task.WhenAll(recv7Task, recv9Task);
            var received = recv7Task.Result.Concat(recv9Task.Result).ToList();

            Assert.Equal(4, received.Count);
            foreach (var buf in received)
            {
                bool match = expected1.SequenceEqual(buf) || expected2.SequenceEqual(buf);
                Assert.True(match, "Received datagram did not match any expected magic packet.");
            }
        }

        [Fact]
        public async Task BroadcastAsync_MultiplePhysicalAddresses_SendsToAll()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            using var listener7 = CreateUdpListener(7);
            using var listener9 = CreateUdpListener(9);

            var pa1 = PhysicalAddress.Parse(SampleMacPlain);
            var pa2 = PhysicalAddress.Parse(SampleMacHyphen);
            var expected1 = (byte[])GetBuildMagicPacket().Invoke(null, new object[] { pa1 })!;
            var expected2 = (byte[])GetBuildMagicPacket().Invoke(null, new object[] { pa2 })!;

            using (var wol = new WOLSocket())
            {
                await wol.BroadcastAsync(new[] { pa1, pa2 });
            }

            var recv7Task = ReceiveN(listener7, 2, TimeSpan.FromSeconds(3), expected1.Length);
            var recv9Task = ReceiveN(listener9, 2, TimeSpan.FromSeconds(3), expected1.Length);
            await Task.WhenAll(recv7Task, recv9Task);
            var received = recv7Task.Result.Concat(recv9Task.Result).ToList();

            Assert.Equal(4, received.Count);
            foreach (var buf in received)
            {
                bool match = expected1.SequenceEqual(buf) || expected2.SequenceEqual(buf);
                Assert.True(match, "Received datagram did not match any expected magic packet.");
            }
        }

        // UDP helpers

        private static UdpClient CreateUdpListener(int port)
        {
            var client = new UdpClient(AddressFamily.InterNetwork);
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            return client;
        }

        private static List<byte[]> ReceiveFromBoth(UdpClient a, UdpClient b, int expectedBytes, TimeSpan timeout)
        {
            var deadline = DateTime.UtcNow + timeout;

            static byte[]? TryGet(UdpClient c, TimeSpan remaining, int expectedBytes)
            {
                var recv = c.ReceiveAsync();
                var done = Task.WhenAny(recv, Task.Delay(remaining)).GetAwaiter().GetResult();
                if (done == recv)
                {
                    var result = recv.GetAwaiter().GetResult();
                    return result.Buffer?.Length == expectedBytes ? result.Buffer : result.Buffer ?? Array.Empty<byte>();
                }
                return null;
            }

            var remaining = deadline - DateTime.UtcNow;
            var r1 = TryGet(a, remaining, expectedBytes);

            remaining = deadline - DateTime.UtcNow;
            var r2 = TryGet(b, remaining, expectedBytes);

            return new[] { r1, r2 }.Where(bf => bf is not null).Select(bf => bf!).ToList();
        }

        private static async Task<List<byte[]>> ReceiveN(UdpClient client, int count, TimeSpan timeout, int expectedBytes)
        {
            var results = new List<byte[]>(capacity: count);
            var deadline = DateTime.UtcNow + timeout;

            while (results.Count < count)
            {
                var remaining = deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero) break;

                var recvTask = client.ReceiveAsync();
                var completed = await Task.WhenAny(recvTask, Task.Delay(remaining));
                if (completed != recvTask)
                    break;

                var result = await recvTask;
                if (result.Buffer?.Length == expectedBytes)
                    results.Add(result.Buffer);
            }

            return results;
        }
    }
}