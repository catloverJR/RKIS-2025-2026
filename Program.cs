using System;

namespace TodoList
{
	public class Program
	{
		public static void Main()
		{
			Console.WriteLine("Работу выполнил: Измайлов");

			ApiDataStorage cloudStorage = new ApiDataStorage("http://localhost:5000/");

			AppInfo.CurrentProfile = cloudStorage.LoadProfile();
			AppInfo.Todos = cloudStorage.LoadTodos();

			if (AppInfo.CurrentProfile == null)
			{
				InitializeUserProfile();
			}
			else
			{
				Console.WriteLine($"Добро пожаловать назад, {AppInfo.CurrentProfile.FirstName}!");
			}

			while (true)
			{
				Console.Write("\nВведите команду: ");
				string input = Console.ReadLine();

				if (string.IsNullOrWhiteSpace(input)) continue;

				try
				{
					ICommand command = CommandParser.Parse(input);

					if (command != null)
					{
						if (command is ExitCommand)
						{
							command.Execute();
							break;
						}
						command.Execute();
					}
				}
				catch (InvalidCommandException ex)
				{
					Console.WriteLine($"Ошибка команды: {ex.Message}");
				}
				catch (InvalidArgumentException ex)
				{
					Console.WriteLine($"Ошибка аргументов: {ex.Message}");
				}
				catch (TaskNotFoundException ex)
				{
					Console.WriteLine($"Ошибка задачи: {ex.Message}");
				}
				catch (EmptyStackException ex)
				{
					Console.WriteLine($"Ошибка истории: {ex.Message}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Неожиданная системная ошибка приложения: {ex.Message}");
				}
			}
		}

		private static void InitializeUserProfile()
		{
			Console.WriteLine("Пожалуйста, заполните данные профиля.");

			Console.Write("Введите ваше имя: ");
			string firstName = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(firstName)) firstName = "Гость";

			Console.Write("Введите вашу фамилию: ");
			string lastName = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(lastName)) lastName = "Гость";

			int birthYear;
			while (true)
			{
				Console.Write("Введите ваш год рождения: ");
				if (int.TryParse(Console.ReadLine(), out birthYear) && birthYear > 1900 && birthYear <= DateTime.Now.Year)
				{
					break;
				}
				Console.WriteLine("Ошибка: Некорректный год рождения.");
			}

			AppInfo.CurrentProfile = new Profile(firstName, lastName, birthYear);
			AppInfo.Storage.SaveProfile(AppInfo.CurrentProfile);
		}
	}
}