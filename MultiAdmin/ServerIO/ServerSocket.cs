using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
				return client?.Connected ?? false;
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

				Task.Run(MessageListener, disposeCancellationSource.Token);
			}, listener);
		}

		public async void MessageListener()
		{
			byte[] intBuffer = new byte[IntBytes];
			while (!disposed && networkStream != null)
			{
				try
				{
					int lengthBytesRead =
						await networkStream.ReadAsync(intBuffer, 0, IntBytes, disposeCancellationSource.Token);

					// Socket has been disconnected
					if (lengthBytesRead <= 0)
					{
						Disconnect();
						break;
					}

					int length = BitConverter.ToInt32(intBuffer, 0);

					// Handle empty messages asap
					if (length == 0)
					{
						OnReceiveMessage?.Invoke(this, "");
					}
					else if (length < 0)
					{
						OnReceiveMessage?.Invoke(this, null);
					}

					byte[] messageBuffer = new byte[length];
					int messageBytesRead =
						await networkStream.ReadAsync(messageBuffer, 0, length, disposeCancellationSource.Token);

					// Socket has been disconnected
					if (messageBytesRead <= 0)
					{
						Disconnect();
						break;
					}

					string message = Encoding.GetString(messageBuffer, 0, length);

					OnReceiveMessage?.Invoke(this, message);
				}
				catch (Exception e)
				{
					Program.LogDebugException(nameof(MessageListener), e);
				}
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

			try
			{
				networkStream.Write(messageBuffer, 0, actualMessageLength + IntBytes);
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(SendMessage), e);
			}
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
