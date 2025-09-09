//
// Authors:
//   Steven Tolzmann
//
// Copyright (C) 2025 Steven Tolzmann

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WOLSharp.Sockets
{
    internal static class SocketExtensions
    {
        public static Task<int> SendToAsync(
            this Socket socket,
            byte[] buffer,
            EndPoint remoteEP,
            SocketFlags flags = SocketFlags.None,
            CancellationToken cancellationToken = default) =>
            socket.SendToAsync(new ArraySegment<byte>(buffer), remoteEP, flags, cancellationToken);

        public static Task<int> SendToAsync(
            this Socket socket,
            byte[] buffer,
            int offset,
            int count,
            EndPoint remoteEP,
            SocketFlags flags = SocketFlags.None,
            CancellationToken cancellationToken = default) =>
            socket.SendToAsync(new ArraySegment<byte>(buffer, offset, count), remoteEP, flags, cancellationToken);

        public static Task<int> SendToAsync(
            this Socket socket,
            ArraySegment<byte> buffer,
            EndPoint remoteEP,
            SocketFlags flags = SocketFlags.None,
            CancellationToken cancellationToken = default)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));
            if (buffer.Array == null) throw new ArgumentNullException(nameof(buffer));

            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<int>(cancellationToken);

            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var args = new SocketAsyncEventArgs
            {
                RemoteEndPoint = remoteEP,
                SocketFlags = flags
            };
            args.SetBuffer(buffer.Array, buffer.Offset, buffer.Count);

            CancellationTokenRegistration ctr = default;
            if (cancellationToken.CanBeCanceled)
                ctr = cancellationToken.Register(() => tcs.TrySetCanceled());

            void Cleanup()
            {
                ctr.Dispose();
                args.Dispose();
            }

            void CompletedHandler(object sender, SocketAsyncEventArgs e)
            {
                args.Completed -= CompletedHandler;
                if (e.SocketError == SocketError.Success)
                    tcs.TrySetResult(e.BytesTransferred);
                else
                    tcs.TrySetException(new SocketException((int)e.SocketError));
                Cleanup();
            }

            args.Completed += CompletedHandler;

            bool willRaiseEvent;
            try
            {
                willRaiseEvent = socket.SendToAsync(args);
            }
            catch (Exception ex)
            {
                args.Completed -= CompletedHandler;
                tcs.TrySetException(ex);
                Cleanup();
                return tcs.Task;
            }

            if (!willRaiseEvent)
            {
                args.Completed -= CompletedHandler;
                if (args.SocketError == SocketError.Success)
                    tcs.TrySetResult(args.BytesTransferred);
                else
                    tcs.TrySetException(new SocketException((int)args.SocketError));
                Cleanup();
            }

            return tcs.Task;
        }
    }
}
