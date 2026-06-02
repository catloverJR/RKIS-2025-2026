using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace TodoList
{
	public class ApiDataStorage : IDataStorage
	{
		private readonly HttpClient _httpClient;
		private readonly string _baseUrl;

		public ApiDataStorage(string baseUrl)
		{
			_baseUrl = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
			_httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(4) };
		}

		public void SaveProfile(Profile profile)
		{
			try
			{
				FileManager.SaveProfile(profile, AppInfo.ProfileFilePath);

				if (File.Exists(AppInfo.ProfileFilePath))
				{
					string encryptedData = File.ReadAllText(AppInfo.ProfileFilePath);
					using (HttpContent content = new StringContent(encryptedData, Encoding.UTF8, "text/plain"))
					{
						HttpResponseMessage response = _httpClient.PostAsync(_baseUrl + "profile", content).GetAwaiter().GetResult();
						using (response)
						{
							if (response.IsSuccessStatusCode)
								Console.WriteLine("[Cloud] Профиль синхронизирован с сервером.");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[Cloud-Error] Не удалось отправить профиль: " + ex.Message);
			}
		}

		public Profile LoadProfile()
		{
			try
			{
				HttpResponseMessage response = _httpClient.GetAsync(_baseUrl + "profile").GetAwaiter().GetResult();
				using (response)
				{
					if (response.IsSuccessStatusCode)
					{
						string encryptedData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
						File.WriteAllText(AppInfo.ProfileFilePath, encryptedData);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[Cloud-Error] Ошибка сети при загрузке профиля. Использование локального кэша: " + ex.Message);
			}

			return FileManager.LoadProfile(AppInfo.ProfileFilePath);
		}

		public void SaveTodos(TodoList todos)
		{
			try
			{
				FileManager.SaveTasks(todos, AppInfo.TodoFilePath);

				string userId = AppInfo.CurrentProfile != null ? AppInfo.CurrentProfile.FirstName : "default_user";
				if (File.Exists(AppInfo.TodoFilePath))
				{
					string encryptedData = File.ReadAllText(AppInfo.TodoFilePath);
					using (HttpContent content = new StringContent(encryptedData, Encoding.UTF8, "text/plain"))
					{
						HttpResponseMessage response = _httpClient.PostAsync(_baseUrl + "todos/" + userId, content).GetAwaiter().GetResult();
						using (response)
						{
							if (response.IsSuccessStatusCode)
								Console.WriteLine("[Cloud] Список задач сохранен на сервере.");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[Cloud-Error] Не удалось сохранить задачи в сети: " + ex.Message);
			}
		}

		public TodoList LoadTodos()
		{
			try
			{
				string userId = AppInfo.CurrentProfile != null ? AppInfo.CurrentProfile.FirstName : "default_user";
				HttpResponseMessage response = _httpClient.GetAsync(_baseUrl + "todos/" + userId).GetAwaiter().GetResult();
				using (response)
				{
					if (response.IsSuccessStatusCode)
					{
						string encryptedData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
						File.WriteAllText(AppInfo.TodoFilePath, encryptedData);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[Cloud-Error] Ошибка сети при загрузке задач. Использование локального кэша: " + ex.Message);
			}

			return FileManager.LoadTasks(AppInfo.TodoFilePath);
		}
	}
}