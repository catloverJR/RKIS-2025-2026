using System;
using System.Collections.Generic;
using System.Linq;

namespace TodoList
{
	public static class CommandParser
	{
		private static readonly Dictionary<string, Func<string, ICommand>> _linqCommands = new Dictionary<string, Func<string, ICommand>>(StringComparer.OrdinalIgnoreCase)
		{
			{ "search", (args) => new SearchCommand(args) }
		};

		public static ICommand Parse(string input)
		{
			string[] inputParts = input.Trim().Split(new[] { ' ' }, 2);
			string commandName = inputParts[0].ToLower();
			string args = inputParts.Length > 1 ? inputParts[1] : string.Empty;

			if (_linqCommands.TryGetValue(commandName, out var commandFactory))
			{
				return commandFactory(args);
			}

			return commandName switch
			{
				"help" => new HelpCommand(),
				"profile" => new ProfileCommand(AppInfo.CurrentProfile),
				"exit" => new ExitCommand(),
				"add" => ParseAddCommand(args),
				"view" => ParseViewCommand(args),
				"status" => ParseStatusCommand(args),
				"delete" => ParseDeleteCommand(args),
				"undo" => new UndoCommand(),
				_ => HandleUnknownCommand(commandName)
			};
		}

		private static ICommand ParseAddCommand(string args)
		{
			if (string.IsNullOrWhiteSpace(args))
			{
				Console.WriteLine("Ошибка: Команда add требует текст задачи.");
				return null;
			}
			return new AddCommand { TaskText = args.Trim('\"') };
		}

		private static ICommand ParseStatusCommand(string args)
		{
			string[] parts = args.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 2 && int.TryParse(parts[0], out int index))
			{
				if (Enum.TryParse<TodoStatus>(parts[1], true, out TodoStatus status))
				{
					return new StatusCommand(AppInfo.Todos, AppInfo.TodoFilePath, index, status);
				}
			}
			Console.WriteLine("Ошибка: Неверный формат команды status. Используйте: status <индекс> <статус>");
			return null;
		}

		private static ICommand ParseViewCommand(string args)
		{
			var command = new ViewCommand(AppInfo.Todos);
			if (string.IsNullOrWhiteSpace(args))
			{
				command.ShowAll = true;
				return command;
			}

			string[] flags = args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var flag in flags)
			{
				switch (flag.ToLower())
				{
					case "-i": command.ShowIndex = true; break;
					case "-s": command.ShowStatus = true; break;
					case "-d": command.ShowDate = true; break;
					case "-a": command.ShowAll = true; break;
				}
			}
			return command;
		}

		private static ICommand ParseDeleteCommand(string args)
		{
			if (int.TryParse(args.Trim(), out int index))
			{
				return new DeleteCommand(AppInfo.Todos, AppInfo.TodoFilePath, index);
			}
			Console.WriteLine("Ошибка: Команда delete требует числовой индекс задачи.");
			return null;
		}

		private static ICommand HandleUnknownCommand(string commandName)
		{
			Console.WriteLine($"Ошибка: Неизвестная команда '{commandName}'. Введите 'help' для списка команд.");
			return null;
		}
	}

	public class HelpCommand : ICommand
	{
		public void Execute()
		{
			Console.WriteLine("""

			Доступные команды:
			help — список команд

			profile — данные профиля

			add "текст" — добавить задачу

			status<idx> < status > — изменить статус задачи

			view[i, s, d, a] — просмотр списка(flags: index, status, date, all)

			search[параметры] — поиск и фильтрация задач через LINQ

			exit — выход из программы

			""");
		}
		public void Undo() { }
	}

	public class ExitCommand : ICommand
	{
		public void Execute() => Console.WriteLine("Завершение работы...\");
		public void Undo() { }
	}

	public class UndoCommand : ICommand
	{
		public void Execute()
		{
			if (AppInfo.UndoStack.Count > 0)
			{
				ICommand command = AppInfo.UndoStack.Pop();
				command.Undo();
				AppInfo.RedoStack.Push(command);
				FileManager.SaveTasks(AppInfo.Todos, AppInfo.TodoFilePath);
			}
			else
			{
				Console.WriteLine("Нечего отменять.");
			}
		}
		public void Undo() { }
	}
}