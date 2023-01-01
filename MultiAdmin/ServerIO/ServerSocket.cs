using System;
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
		public static readonly UTF8Encoding Encoding = new(false, true);

		private readonly CancellationTokenSource disposeCancellationSource = new();
		private bool disposed = false;

		private readonly TcpListener listener;

		private TcpClient? client;
		private NetworkStream? networkStream;

		public readonly struct MessageEventArgs
		{
			public MessageEventArgs(string? message, byte color)
			{
				this.message = message;
				this.color = color;
			}

			public readonly string? message;
			public readonly byte color;
		}

		public event EventHandler<MessageEventArgs>? OnReceiveMessage;
		public event EventHandler<byte>? OnReceiveAction;

		public int Port => ((IPEndPoint)listener.LocalEndpoint).Port;

		public bool Connected => client?.Connected ?? false;

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
				try
				{
					client = listener.EndAcceptTcpClient(result);
					networkStream = client.GetStream();

					Task.Run(MessageListener, disposeCancellationSource.Token);
				}
				catch (ObjectDisposedException)
				{
					// IGNORE
				}
				catch (Exception e)
				{
					Program.LogDebugException(nameof(Connect), e);
				}
			}, listener);
		}

		public async void MessageListener()
		{
			byte[] typeBuffer = new byte[1];
			Memory<byte> typeBufferMemory = new(typeBuffer);

			byte[] intBuffer = new byte[IntBytes];
			Memory<byte> intBufferMemory = new(intBuffer);

			while (!disposed && networkStream != null)
			{
				try
				{
					int messageTypeBytesRead =
						await networkStream.ReadAsync(typeBufferMemory, disposeCancellationSource.Token);

					// Socket has been disconnected
					if (messageTypeBytesRead <= 0)
					{
						Disconnect();
						break;
					}

					byte messageType = typeBuffer[0];

					// 16 colors reserved, otherwise process as control message (action)
					if (messageType >= 16)
					{
						OnReceiveAction?.Invoke(this, messageType);
						continue;
					}

					int lengthBytesRead =
						await networkStream.ReadAsync(intBufferMemory, disposeCancellationSource.Token);

					// Socket has been disconnected or integer read is invalid
					if (lengthBytesRead != IntBytes)
					{
						Disconnect();
						break;
					}

					// Decode integer
					int length = (intBuffer[0] << 24) | (intBuffer[1] << 16) | (intBuffer[2] << 8) | intBuffer[3];

					// Handle empty messages asap
					if (length == 0)
					{
						OnReceiveMessage?.Invoke(this, new MessageEventArgs("", messageType));
					}
					else if (length < 0)
					{
						OnReceiveMessage?.Invoke(this, new MessageEventArgs(null, messageType));
					}

					byte[] messageBuffer = new byte[length];
					int messageBytesRead =
						await networkStream.ReadAsync(messageBuffer.AsMemory(0, length), disposeCancellationSource.Token);

					// Socket has been disconnected
					if (messageBytesRead <= 0)
					{
						Disconnect();
						break;
					}

					string message = Encoding.GetString(messageBuffer, 0, length);

					OnReceiveMessage?.Invoke(this, new MessageEventArgs(message, messageType));
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

			GC.SuppressFinalize(this);

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
