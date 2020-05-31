using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MultiAdmin.ServerIO
{
	public class ServerSocket : IDisposable
	{
		private const int IntBytes = sizeof(int);
		public static readonly UTF8Encoding Encoding = new UTF8Encoding(false, true);

		private readonly CancellationTokenSource disposeCancellationSource = new CancellationTokenSource();
		private bool disposed = false;

		private readonly TcpListener listener;

		private TcpClient client;
		private NetworkStream networkStream;

		public event EventHandler<string> OnReceiveMessage;

		public int Port
		{
			get
			{
				return ((IPEndPoint)listener.LocalEndpoint).Port;
			}
		}

		public bool Connected
		{
			get
			{
				return client.Connected;
			}
		}

		// Port 0 automatically assigns a port
		public ServerSocket(int port = 0)
		{
			listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, port));
		}

		public void Connect()
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(ServerSocket));

			listener.Start();
			listener.BeginAcceptTcpClient(result =>
			{
				client = listener.EndAcceptTcpClient(result);
				networkStream = client.GetStream();

				Thread listenerThread = new Thread(MessageListener);
				listenerThread.Start();
			}, listener);
		}

		public void MessageListener()
		{
			byte[] intBuffer = new byte[IntBytes];
			while (!disposed)
			{
				networkStream.ReadAsync(intBuffer, 0, IntBytes, disposeCancellationSource.Token).Wait();
				if (disposed)
					break;

				int length = BitConverter.ToInt32(intBuffer, 0);

				byte[] messageBuffer = new byte[length];
				networkStream.ReadAsync(messageBuffer, 0, length, disposeCancellationSource.Token).Wait();
				if (disposed)
					break;

				string message = Encoding.GetString(messageBuffer, 0, length);

				OnReceiveMessage?.Invoke(this, message);
			}
		}

		public void SendMessage(string message)
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(ServerSocket));

			if (networkStream == null)
				throw new NullReferenceException($"{nameof(networkStream)} hasn't been initialized");

			byte[] messageBuffer = new byte[Encoding.GetMaxByteCount(message.Length) + IntBytes];

			int actualMessageLength = Encoding.GetBytes(message, 0, message.Length, messageBuffer, IntBytes);
			Array.Copy(BitConverter.GetBytes(actualMessageLength), messageBuffer, IntBytes);

			networkStream.WriteAsync(messageBuffer, 0, actualMessageLength + IntBytes, disposeCancellationSource.Token).Wait();
		}

		public void Disconnect()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (disposed)
				return;

			disposed = true;
			disposeCancellationSource.Cancel();
			disposeCancellationSource.Dispose();

			networkStream?.Close();
			client?.Close();
			listener.Stop();

			OnReceiveMessage = null;
		}
	}
}
