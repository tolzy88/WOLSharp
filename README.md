# WOLSharp

Interop helpers for sending **Wake‑on‑LAN (WOL) magic packets** to wake machines on your network.

---

## Why WOLSharp?

* **Dead‑simple API** — one call to broadcast a magic packet.
* **.NET Standard 2.0** — works on .NET Framework 4.6.1+, .NET Core 2.0+, and modern .NET.

---

## Install
[Get it on NuGet!](https://www.nuget.org/packages/WOLSharp)

```bash
# Package Id
WOLSharp

# dotnet CLI
dotnet add package WOLSharp

# Package Manager
Install-Package WOLSharp
```

---

## Quick start

```csharp
using System.Net.NetworkInformation;
using WOLSharp;

// Create a socket (will broadcast)
using var wol = new WOLSocket();

// Send using a string MAC (accepted: AA-BB-CC-DD-EE-FF, AA:BB:CC:DD:EE:FF, or AABBCCDDEEFF)
wol.Broadcast("01-23-45-67-89-AB");

// Or using PhysicalAddress
var mac = PhysicalAddress.Parse("0123456789AB");
wol.Broadcast(mac);
```

> **Heads‑up:** WOL uses UDP broadcast (commonly port **9** or **7**). Ensure your NIC/BIOS/UEFI and OS power settings allow Wake‑on‑LAN and that your network permits directed or global broadcast.

---

## What’s a magic packet?

A WOL magic packet is:

* 6 bytes of `0xFF`
* followed by your 6‑byte MAC **repeated 16 times**

Total length: **102 bytes**.

```text
FF FF FF FF FF FF  <mac x16>
```

---

## Troubleshooting

* **No wake?**

  * Enable WOL in BIOS/UEFI and in the NIC’s driver settings ("Wake on Magic Packet").
  * Some systems only wake from S3/S4, not from full power‑off (S5).
  * On Windows, uncheck “Allow the computer to turn off this device to save power” for the NIC if needed.
* **Across subnets/VLANs:** you may need a router that permits **directed broadcast** or a WOL relay/proxy.

---

## Notes

* WOL is unauthenticated by design. Don’t expose a “wake anything” endpoint to untrusted networks.
* Some NICs require "Wake on **Magic Packet** only" to be enabled.
