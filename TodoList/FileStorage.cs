using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace TodoList
{
	public class FileStorage : IDataStorage
	{
		private readonly string _todoFilePath;
		private readonly string _profileFilePath;

		private const char Separator = ';';
		private const string NewLineReplacement = "\\n";

		// Фиксированные криптографические ключи для детерминированного шифрования/дешифрования AES
		private static readonly byte[] AesKey = new byte[] { 0x54, 0x6F, 0x64, 0x6F, 0x4C, 0x69, 0x73, 0x74, 0x41, 0x65, 0x73, 0x4B, 0x65, 0x79, 0x32, 0x30, 0x32, 0x36, 0x4E, 0x65, 0x74, 0x39, 0x43, 0x68, 0x61, 0x72, 0x73, 0x31, 0x32, 0x33, 0x34, 0x35 }; // 32 байта (AES-256)
		private static readonly byte[] AesIV = new byte[] { 0x49, 0x6E, 0x69, 0x74, 0x56, 0x65, 0x63, 0x74, 0x6F, 0x72, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36 };  // 16 байт

		public FileStorage(string todoFilePath, string profileFilePath)
		{
			_todoFilePath = todoFilePath ?? throw new ArgumentNullException(nameof(todoFilePath));
			_profileFilePath = profileFilePath ?? throw new ArgumentNullException(nameof(profileFilePath));

			EnsureDirectoryExists(_todoFilePath);
			EnsureDirectoryExists(_profileFilePath);
		}

		private void EnsureDirectoryExists(string filePath)
		{
			string dir = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
		}

		public void SaveProfile(Profile profile)
		{
			if (profile == null) return;

			try
			{
				string jsonString = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });

				using (FileStream fs = new FileStream(_profileFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
				using (BufferedStream bs = new BufferedStream(fs))
				using (Aes aes = Aes.Create())
				using (ICryptoTransform encryptor = aes.CreateEncryptor(AesKey, AesIV))
				using (CryptoStream cs = new CryptoStream(bs, encryptor, CryptoStreamMode.Write))
				using (StreamWriter sw = new StreamWriter(cs, Encoding.UTF8))
				{
					sw.Write(jsonString);
				}
				Console.WriteLine($"[Storage] Зашифрованный профиль пользователя успешно обновлен.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка сохранения профиля: {ex.Message}");
			}
		}

		public Profile Profile LoadProfile()
		{
			if (!File.Exists(_profileFilePath)) return null;

			try
			{
				using (FileStream fs = new FileStream(_profileFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (BufferedStream bs = new BufferedStream(fs))
				using (Aes aes = Aes.Create())
				using (ICryptoTransform decryptor = aes.CreateDecryptor(AesKey, AesIV))
				using (CryptoStream cs = new CryptoStream(bs, decryptor, CryptoStreamMode.Read))
				using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
				{
					string jsonString = sr.ReadToEnd();
					return JsonSerializer.Deserialize<Profile>(jsonString);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Предупреждение: Не удалось расшифровать или прочитать профиль ({ex.Message}). Создан новый.");
				return null;
			}
		}

		public void SaveTodos(TodoList todos)
		{
			if (todos == null) return;

			try
			{
				using (FileStream fs = new FileStream(_todoFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
				using (BufferedStream bs = new BufferedStream(fs))
				using (Aes aes = Aes.Create())
				using (ICryptoTransform encryptor = aes.CreateEncryptor(AesKey, AesIV))
				using (CryptoStream cs = new CryptoStream(bs, encryptor, CryptoStreamMode.Write))
				using (StreamWriter sw = new StreamWriter(cs, Encoding.UTF8))
				{
					foreach (var item in todos)
					{
						string text = EscapeTextForCsv(item.Text);
						string status = item.Status.ToString();
						string date = item.LastUpdate.ToString("o");
						sw.WriteLine($"{text}{Separator}{status}{Separator}{date}");
					}
				}
				Console.WriteLine($"[Storage] Данные задач успешно зашифрованы и сохранены.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка сохранения задач: {ex.Message}");
			}
		}

		public TodoList LoadTodos()
		{
			var todoList = new TodoList();
			if (!File.Exists(_todoFilePath)) return todoList;

			try
			{
				using (FileStream fs = new FileStream(_todoFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (BufferedStream bs = new BufferedStream(fs))
				using (Aes aes = Aes.Create())
				using (ICryptoTransform decryptor = aes.CreateDecryptor(AesKey, AesIV))
				using (CryptoStream cs = new CryptoStream(bs, decryptor, CryptoStreamMode.Read))
				using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
				{
					string line;
					while ((line = sr.ReadLine()) != null)
					{
						if (string.IsNullOrWhiteSpace(line)) continue;

						string[] parts = ParseCsvLine(line);
						if (parts.Length >= 3)
						{
							string text = UnescapeTextFromCsv(parts[0]);
							if (Enum.TryParse<TodoStatus>(parts[1], out TodoStatus status) &&
								DateTime.TryParse(parts[2], out DateTime lastUpdate))
							{
								todoList.Add(new TodoItem(text, status, lastUpdate));
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Предупреждение при чтении или дешифровании базы задач: {ex.Message}");
			}

			return todoList;
		}

		private string EscapeTextForCsv(string data)
		{
			if (string.IsNullOrEmpty(data)) return string.Empty;
			string escapedData = data.Replace("\n", NewLineReplacement);
			escapedData = escapedData.Replace("\"", "\"\"");
			if (escapedData.Contains(Separator.ToString()) || escapedData.Contains("\""))
			{
				escapedData = $"\"{escapedData}\"";
			}
			return escapedData;
		}

		private string UnescapeTextFromCsv(string data)
		{
			if (string.IsNullOrEmpty(data)) return string.Empty;
			if (data.StartsWith("\"") && data.EndsWith("\""))
			{
				data = data.Substring(1, data.Length - 2);
			}
			string unescapedData = data.Replace("\"\"", "\"");
			return unescapedData.Replace(NewLineReplacement, "\n");
		}

		private string[] ParseCsvLine(string line)
		{
			List<string> parts = new List<string>();
			StringBuilder sb = new StringBuilder();
			bool inQuotes = false;

			for (int i = 0; i < line.Length; i++)
			{
				char c = line[i];
				if (c == '"')
				{
					if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
					{
						sb.Append('"');
						i++;
					}
					else
					{
						inQuotes = !inQuotes;
					}
				}
				else if (c == Separator && !inQuotes)
				{
					parts.Add(sb.ToString());
					sb.Clear();
				}
				else
				{
					sb.Append(c);
				}
			}
			parts.Add(sb.ToString());
			return parts.ToArray();
		}
	}
}