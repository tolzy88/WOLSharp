//
// Authors:
//   Steven Tolzmann
//
// Copyright (C) 2025 Steven Tolzmann

using WOLSharp.Sockets;

namespace WOLSharp_Con
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var wol = new WOLSocket();
            if (args.Length == 0) // No args provided, get console input
            {
                while (true) // Continue prompting user until no more input is received
                {
                    Console.Write("Enter MAC Address: ");
                    string mac = Console.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(mac))
                        break; // User is done, exit
                    try
                    {
                        await wol.BroadcastAsync(mac);
                        Console.WriteLine($"{mac} [OK]");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{mac} [FAIL] {ex}");
                    }
                }
            }
            else // Args provided
            {
                await wol.BroadcastAsync(args);
            }
        }
    }
}
