using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleWebServer
{
	public class SimpleHttpServer
	{
		private TcpListener _listener;
		private bool _isRunning;
		private CancellationTokenSource _cancellationTokenSource;

		public delegate string RouteHandler(string path);
		private RouteHandler _routeHandler;

		public SimpleHttpServer(int port, RouteHandler routeHandler)
		{
			_listener = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
			_routeHandler = routeHandler;
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public void Start()
		{
			_isRunning = true;
			_listener.Start();

			Console.WriteLine($"Server started on port {((IPEndPoint)_listener.LocalEndpoint).Port}");
			Console.WriteLine("Silahkan kunjungi http://localhost:5000/ di Browser");

			Task.Run(() => ListenForClientsAsync(_cancellationTokenSource.Token));
		}

		private async Task ListenForClientsAsync(CancellationToken cancellationToken)
		{
			try
			{
				while (_isRunning && !cancellationToken.IsCancellationRequested)
				{
					TcpClient client = await _listener.AcceptTcpClientAsync();
					_ = Task.Run(() => HandleClientAsync(client, cancellationToken));
				}
			}
			catch (Exception ex)
			{
				if (!cancellationToken.IsCancellationRequested)
				{
					Console.WriteLine($"Error accepting client: {ex.Message}");
				}
			}
		}

		private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
		{
			using (client)
			{
				try
				{
					using NetworkStream stream = client.GetStream();

					// Set a timeout for reading from the client
					client.ReceiveTimeout = 5000; // 5 seconds

					// Buffer for reading data
					byte[] buffer = new byte[4096];
					int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

					if (bytesRead > 0)
					{
						// Parse the HTTP request
						string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
						string path = ParseRequestPath(request);

						// Get the response content based on the path
						string content = _routeHandler(path);

						// Send the HTTP response
						byte[] response = Encoding.UTF8.GetBytes(content);
						await stream.WriteAsync(response, 0, response.Length, cancellationToken);
					}
				}
				catch (Exception ex)
				{
					if (!cancellationToken.IsCancellationRequested)
					{
						Console.WriteLine($"Error handling client: {ex.Message}");
					}
				}
			}
		}

		private string ParseRequestPath(string request)
		{
			// Get the first line of the HTTP request
			int lineEnd = request.IndexOf('\n');
			if (lineEnd == -1) return "/";

			string firstLine = request.Substring(0, lineEnd).Trim();

			// Find the path in the request line
			int startIndex = firstLine.IndexOf(' ') + 1;
			if (startIndex >= firstLine.Length) return "/";

			int endIndex = firstLine.IndexOf(' ', startIndex);
			if (endIndex == -1) return "/";

			// Extract the path
			string path = firstLine.Substring(startIndex, endIndex - startIndex);

			// Remove query parameters if present
			int queryIndex = path.IndexOf('?');
			if (queryIndex != -1)
			{
				path = path.Substring(0, queryIndex);
			}

			// Handle trailing slashes
			if (path.Length > 1 && path.EndsWith("/"))
			{
				path = path.TrimEnd('/');
			}

			return path;
		}

		public void Stop()
		{
			_isRunning = false;
			_cancellationTokenSource.Cancel();
			try
			{
				_listener.Stop();
				Console.WriteLine("Server stopped");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error stopping server: {ex.Message}");
			}
		}
	}
}