using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TodoServer
{
	class Program
	{
		private const string ListeningAddress = "http://localhost:5000/";

		static async Task Main(string[] args)
		{
			Console.WriteLine("=== ЗАПУСК СЕТЕВОГО ХРАНИЛИЩА TODO ===");

			using (HttpListener listener = new HttpListener())
			{
				listener.Prefixes.Add(ListeningAddress);
				try
				{
					listener.Start();
					Console.WriteLine($"Сервер успешно запущен. Ожидание запросов на {ListeningAddress}");
				}
				catch (HttpListenerException ex)
				{
					Console.WriteLine($"Ошибка запуска: {ex.Message}");
					return;
				}

				while (true)
				{
					try
					{
						HttpListenerContext context = await listener.GetContextAsync();
						_ = Task.Run(() => HandleClientRequestAsync(context));
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Ошибка при приеме сетевого пакета: {ex.Message}");
					}
				}
			} 
		}

		private static async Task HandleClientRequestAsync(HttpListenerContext context)
		{
			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;

			Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Получен запрос: {request.HttpMethod}");

			try
			{
				string responseText = "Базовая заглушка ответа сервера.";
				byte[] buffer = Encoding.UTF8.GetBytes(responseText);

				response.StatusCode = (int)HttpStatusCode.OK;
				response.ContentLength64 = buffer.Length;
				response.ContentType = "text/plain; charset=utf-8";

				using (Stream output = response.OutputStream)
				{
					await output.WriteAsync(buffer, 0, buffer.Length);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка отправки данных: {ex.Message}");
			}
		}
	}
}