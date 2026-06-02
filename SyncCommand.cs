using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace TodoList
{
	public class SyncCommand : ICommand
	{
		private readonly string _flag;
		private readonly HttpClient _client;
		private const string ServerUrl = "http://localhost:5000/";

		public SyncCommand(string flag)
		{
			_flag = flag != null ? flag.ToLower().Trim() : string.Empty;
			_client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
		}

		public void Execute()
		{
			if (!CheckServerAvailability())
			{
				throw new InvalidOperationException("Ошибка: сервер недоступен. Проверьте статус запуска TodoServer.");
			}

			string userId = AppInfo.CurrentProfile != null ? AppInfo.CurrentProfile.FirstName : "default_user";

			if (_flag == "--push")
			{
				ExecutePush(userId);
			}
			else if (_flag == "--pull")
			{
				ExecutePull(userId);
			}
			else
			{
				throw new ArgumentException("Неизвестный параметр синхронизации '" + _flag + "'. Поддерживаются только --push и --pull.");
			}
		}

		public void Undo() { }

		private bool CheckServerAvailability()
		{
			try
			{
				using (HttpResponseMessage response = _client.GetAsync(ServerUrl).GetAwaiter().GetResult())
				{
					return true;
				}
			}
			catch
			{
				return false;
			}
		}

		private void ExecutePush(string userId)
		{
			Console.WriteLine("Синхронизация: отправка данных на сервер (--push)...");

			if (File.Exists(AppInfo.ProfileFilePath))
			{
				string profileData = File.ReadAllText(AppInfo.ProfileFilePath);
				using (HttpContent content = new StringContent(profileData, Encoding.UTF8, "text/plain"))
				{
					using (HttpResponseMessage res = _client.PostAsync(ServerUrl + "profile", content).GetAwaiter().GetResult()) { }
				}
			}

			if (File.Exists(AppInfo.TodoFilePath))
			{
				string todoData = File.ReadAllText(AppInfo.TodoFilePath);
				using (HttpContent content = new StringContent(todoData, Encoding.UTF8, "text/plain"))
				{
					using (HttpResponseMessage res = _client.PostAsync(ServerUrl + "todos/" + userId, content).GetAwaiter().GetResult()) { }
				}
			}

			Console.WriteLine("Локальные зашифрованные данные успешно скопированы на сервер.");
		}

		private void ExecutePull(string userId)
		{
			Console.WriteLine("Синхронизация: получение данных с сервера (--pull)...");

			try
			{
				using (HttpResponseMessage profileResponse = _client.GetAsync(ServerUrl + "profile").GetAwaiter().GetResult())
				{
					if (profileResponse.IsSuccessStatusCode)
					{
						string profileData = profileResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
						File.WriteAllText(AppInfo.ProfileFilePath, profileData);
						AppInfo.CurrentProfile = FileManager.LoadProfile(AppInfo.ProfileFilePath);
					}
				}

				using (HttpResponseMessage todosResponse = _client.GetAsync(ServerUrl + "todos/" + userId).GetAwaiter().GetResult())
				{
					if (todosResponse.IsSuccessStatusCode)
					{
						string todosData = todosResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
						File.WriteAllText(AppInfo.TodoFilePath, todosData);
						AppInfo.Todos = FileManager.LoadTasks(AppInfo.TodoFilePath);
						Console.WriteLine("Данные с сервера успешно загружены и применены локально.");
					}
					else
					{
						Console.WriteLine("На сервере не найдено сохраненного списка задач для пользователя " + userId);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Ошибка при скачивании данных: " + ex.Message);
			}
		}
	}
}