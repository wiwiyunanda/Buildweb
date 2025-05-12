using System;
using System.Text;
using System.Threading;

namespace SimpleWebServer
{
	class Program
	{
		static void Main(string[] args)
		{
			// Get port from environment variable for Replit compatibility
			int port = 5000;
			string portEnv = Environment.GetEnvironmentVariable("PORT");
			if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out int envPort))
			{
				port = envPort;
			}

			// Create and start the HTTP server
			SimpleHttpServer server = new SimpleHttpServer(port, HandleRoute);
			server.Start();

			// Keep the application running until Ctrl+C is pressed
			Console.WriteLine("Press Ctrl+C to stop the server");
			ManualResetEvent waitHandle = new ManualResetEvent(false);

			// Set up a handler for Ctrl+C 
			Console.CancelKeyPress += (sender, e) =>
			{
				e.Cancel = true;
				server.Stop();
				waitHandle.Set();
			};

			// Wait for the exit signal
			waitHandle.WaitOne();
		}

		static string HandleRoute(string path)
		{
			Console.WriteLine($"Request received for path: {path}");

			string title, content, newcontent;
			int statusCode = 200;

			if (string.IsNullOrEmpty(path) || path == "/")
			{
				// Home page route
				title = "Selamat Datang di Website Belajar Ika";
				newcontent = "i will show you what i learn. it's just for fun";
				content = "Enjoy!";
			}
			else if (path.Equals("/about", StringComparison.OrdinalIgnoreCase))
			{
				// About page route
				title = "Disini kamu akan membaca curhatan ika (kalau mood)";
				newcontent = "Back Home Again - John Denver";
				content = "There's a storm across the valley\r\nClouds are rolling in\r\nThe afternoon is heavy on your shoulders\r\nThere's a truck out on the four lane\r\nA mile or more away\r\nThe whining of his wheels just makes it colder\r\nHe's an hour away from riding\r\nOn your prayers up in the sky\r\nAnd ten days on the road are barely gone\r\nThere's a fire softly burning\r\nSupper's on the stove\r\nIt's the light in your eyes that makes him warm\r\nHey, it's good to be back home again\r\nSometimes this old farm feels like a long-lost friend\r\nYes, and hey, it's good to be back home again\r\nThere's all the news to tell him\r\nHow you've spent your time\r\nWhat's the latest thing the neighbors say?\r\nAnd your mother called last Friday\r\nSunshine made her cry\r\nYou felt the baby move just yesterday\r\nHey, it's good to be back home again, yes, it is\r\nSometimes this old farm feels like a long-lost friend\r\nYes, and hey, it's good to be back home again\r\nAnd all the time that I can lay this tired old body down\r\nTo feel your fingers feather soft upon me\r\nThe kisses that I live for, the love that lights my way\r\nThe happiness that living with you brings me\r\nIt's the sweetest thing I know of\r\nJust spending time with you\r\nIt's the little things that make a house a home\r\nLike a fire softly burning\r\nSupper on the stove\r\nThe light in your eyes, it makes me warm\r\nHey, it's good to be back home again\r\nSometimes this old farm feels like a long-lost friend\r\nYes, and hey, it's good to be back home again\r\nHey, it's good to be back home again, you know it is\r\nSometimes this old farm feels like a long-lost friend\r\nHey, it's good to be back home again\r\nI said, hey, it's good to be back home again";
				content = content.Replace("\r\n", "<br>");
			}
			else if (path.Equals("/learn", StringComparison.OrdinalIgnoreCase))
			{
				// About page route
				title = "What a dream?";
				newcontent = "what is your plan to drive your life?";
				content = "\"Dreams are the seeds of possibility planted in the soil of our imagination. They whisper to us in quiet moments, reminding us that we are capable of more than we know. The journey to achieving a dream is rarely easy—it demands courage, perseverance, and faith. But every step forward, no matter how small, brings us closer. Chase your dreams with both passion and patience, and one day, you'll look back and realize the impossible became your reality.\"";
			}
			else
			{
				// 404 Not Found
				statusCode = 404;
				title = "Halaman Tidak Ditemukan";
				newcontent = "Ups";
				content = "Maaf, halaman yang Anda cari tidak ditemukan.";
			}

			// Generate HTML response
			string responseBody = GenerateHtmlResponse(title, content, newcontent, new string[] { "/", "/about", "/learn" });
			//string responseBody1 = GenerateHtmlResponse(title, content, new string[] { "/", "/learn" });
			// Generate HTTP response headers
			string headers = $"HTTP/1.1 {statusCode} {(statusCode == 200 ? "OK" : "Not Found")}\r\n" +
							 "Content-Type: text/html; charset=utf-8\r\n" +
							 $"Content-Length: {Encoding.UTF8.GetByteCount(responseBody)}\r\n" +
							 "Connection: close\r\n\r\n";

			// Return the complete HTTP response
			return headers + responseBody;
		}

		static string GenerateHtmlResponse(string title, string content, string newcontent, string[] links)
		{
			// Build the navigation menu
			StringBuilder navBuilder = new StringBuilder();
			foreach (string link in links)
			{
				string linkText = link == "/" ? "Beranda" : link.Substring(1).Replace("-", " ");
				linkText = char.ToUpper(linkText[0]) + linkText.Substring(1);
				navBuilder.AppendLine($"<a href=\"{link}\" style=\"margin-right: 15px;\">{linkText}</a>");
			}

			// Create a basic HTML structure
			return $@"
<!DOCTYPE html>
<html lang=""id"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title}</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            margin: 0;
            padding: 0;
            color: #333;
        }}
        header {{
            background-color: #9370DB;
            color: white;
            text-align: center;
            padding: 1rem;
        }}
        nav {{
            display: flex;
            justify-content: center;
            background-color: #f4f4f4;
            padding: 10px;
        }}
        main {{
            max-width: 800px;
            margin: 0 auto;
            padding: 0px;
        }}
        h1 {{
            color: #333;
        }}
        footer {{
            text-align: center;
            padding: 1rem;
            background-color: #f4f4f4;
            margin-top: 2rem;
        }}
    </style>
</head>
<body>
    <header>
        <h1>{title}</h1>
    </header>
    <nav>
        {navBuilder}
    </nav>
    <main>
        <p>{newcontent}</p>
    </main> 
<main>
        <p>{content}</p>
    </main>
    <footer>
        <p>&copy; {DateTime.Now.Year} - wiwiyunanda's diary</p>
    </footer>
</body>
</html>";
		}
	}
}