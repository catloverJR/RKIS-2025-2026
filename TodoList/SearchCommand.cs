using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TodoList
{
	public class SearchCommand : ICommand
	{
		private readonly string _arguments;

		public SearchCommand(string arguments)
		{
			_arguments = arguments ?? string.Empty;
		}

		public void Execute()
		{
			var argsMap = ParseArguments(_arguments);

			var query = AppInfo.Todos
				.Select((task, index) => new { Task = task, OriginalIndex = index + 1 });

			if (argsMap.TryGetValue("--contains", out var containsText))
			{
				query = query.Where(q => q.Task.Text.Contains(containsText, StringComparison.OrdinalIgnoreCase));
			}
			if (argsMap.TryGetValue("--starts-with", out var startsWithText))
			{
				query = query.Where(q => q.Task.Text.StartsWith(startsWithText, StringComparison.OrdinalIgnoreCase));
			}
			if (argsMap.TryGetValue("--ends-with", out var endsWithText))
			{
				query = query.Where(q => q.Task.Text.EndsWith(endsWithText, StringComparison.OrdinalIgnoreCase));
			}

			if (argsMap.TryGetValue("--status", out var statusStr))
			{
				if (Enum.TryParse<TodoStatus>(statusStr, true, out var status))
				{
					query = query.Where(q => q.Task.Status == status);
				}
				else
				{
					throw new InvalidArgumentException($"Недопустимое значение статуса '{statusStr}' для фильтрации.");
				}
			}

			if (argsMap.TryGetValue("--from", out var fromStr))
			{
				if (DateTime.TryParseExact(fromStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate))
				{
					query = query.Where(q => q.Task.LastUpdate >= fromDate);
				}
				else
				{
					throw new InvalidArgumentException("Некорректный формат даты в параметре --from. Ожидается: yyyy-MM-dd");
				}
			}
			if (argsMap.TryGetValue("--to", out var toStr))
			{
				if (DateTime.TryParseExact(toStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
				{
					query = query.Where(q => q.Task.LastUpdate <= toDate.AddDays(1).AddTicks(-1));
				}
				else
				{
					throw new InvalidArgumentException("Некорректный формат даты в параметре --to. Ожидается: yyyy-MM-dd");
				}
			}

			bool isDesc = argsMap.ContainsKey("--desc");
			if (argsMap.TryGetValue("--sort", out var sortBy))
			{
				if (sortBy.Equals("text", StringComparison.OrdinalIgnoreCase))
				{
					query = isDesc
						? query.OrderByDescending(q => q.Task.Text)
						: query.OrderBy(q => q.Task.Text);
				}
				else if (sortBy.Equals("date", StringComparison.OrdinalIgnoreCase))
				{
					query = isDesc
						? query.OrderByDescending(q => q.Task.LastUpdate)
						: query.OrderBy(q => q.Task.LastUpdate);
				}
				else
				{
					throw new InvalidArgumentException($"Неизвестный параметр сортировки '{sortBy}'. Доступны: text, date.");
				}
			}

			if (argsMap.TryGetValue("--top", out var topStr))
			{
				if (int.TryParse(topStr, out var topCount) && topCount > 0)
				{
					query = query.Take(topCount);
				}
				else
				{
					throw new InvalidArgumentException("Значение параметра --top должно быть положительным целым числом.");
				}
			}

			var results = query.ToList();

			if (!results.Any())
			{
				Console.WriteLine("Ничего не найдено");
				return;
			}

			Console.WriteLine(new string('-', 75));
			Console.WriteLine($"{"Index",-6} | {"Text",-30} | {"Status",-12} | {"LastUpdate",-19}");
			Console.WriteLine(new string('-', 75));

			foreach (var item in results)
			{
				string shortText = item.Task.Text.Length > 30
					? item.Task.Text.Substring(0, 27) + "..."
					: item.Task.Text;

				Console.WriteLine($"{item.OriginalIndex,-6} | {shortText,-30} | {item.Task.GetStatusString(),-12} | {item.Task.LastUpdate:yyyy-MM-dd HH:mm:ss}");
			}
			Console.WriteLine(new string('-', 75));
		}

		public void Undo()
		{
		}

		private Dictionary<string, string> ParseArguments(string arguments)
		{
			var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var tokens = FormatTokens(arguments);

			var validFlags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"--contains", "--starts-with", "--ends-with", "--status", "--from", "--to", "--sort", "--desc", "--top"
			};

			for (int i = 0; i < tokens.Count; i++)
			{
				if (tokens[i].StartsWith("--"))
				{
					string flag = tokens[i];

					if (!validFlags.Contains(flag))
					{
						throw new InvalidArgumentException($"Использован неизвестный флаг поиска: '{flag}'.");
					}

					if (flag.Equals("--desc", StringComparison.OrdinalIgnoreCase) || i + 1 >= tokens.Count || tokens[i + 1].StartsWith("--"))
					{
						result[flag] = string.Empty;
					}
					else
					{
						result[flag] = tokens[i + 1];
						i++;
					}
				}
				else
				{
					throw new InvalidArgumentException($"Обнаружен изолированный аргумент '{tokens[i]}' вне управляющих флагов.");
				}
			}
			return result;
		}

		private List<string> FormatTokens(string input)
		{
			var tokens = new List<string>();
			var currentToken = new System.Text.StringBuilder();
			bool inQuotes = false;

			foreach (char c in input)
			{
				if (c == '"')
				{
					inQuotes = !inQuotes;
					continue;
				}
				if (char.IsWhiteSpace(c) && !inQuotes)
				{
					if (currentToken.Length > 0)
					{
						tokens.Add(currentToken.ToString());
						currentToken.Clear();
					}
				}
				else
				{
					currentToken.Append(c);
				}
			}
			if (currentToken.Length > 0)
			{
				tokens.Add(currentToken.ToString());
			}
			return tokens;
		}
	}
}