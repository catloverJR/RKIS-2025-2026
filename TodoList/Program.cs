using System;

namespace TodoList
{
	public class Program
	{
		public static void Main()
		{
			Console.WriteLine("Работу выполнили: Измайлов и Кузнецова");

			AppInfo.Storage = new DbDataStorage("todo.db");

			AppInfo.CurrentProfile = AppInfo.Storage.LoadProfile();
			AppInfo.Todos = AppInfo.Storage.LoadTodos();

			if (AppInfo.CurrentProfile == null)
			{
				InitializeUserProfile();
			}
			else
			{
				Console.WriteLine("Добро пожаловать назад, " + AppInfo.CurrentProfile.FirstName + "!");
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
					Console.WriteLine("Ошибка команды: " + ex.Message);
				}
				catch (InvalidArgumentException ex)
				{
					Console.WriteLine("Ошибка аргументов: " + ex.Message);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Системная ошибка работы с БД: " + ex.Message);
				}
			}
		}

		private static void InitializeUserProfile()
		{
			Console.WriteLine("Пожалуйста, заполните данные профиля для сохранения в БД.");

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