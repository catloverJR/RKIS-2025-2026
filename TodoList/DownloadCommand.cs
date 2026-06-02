using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList
{
	public class DownloadCommand : ICommand
	{
		private readonly int _downloadsCount;
		private static readonly object _consoleLock = new object();
		private const int TotalSegments = 20;

		public DownloadCommand(int downloadsCount)
		{
			_downloadsCount = downloadsCount;
		}

		public void Execute()
		{
			RunAsync().GetAwaiter().GetResult();
		}

		public void Undo()
		{
		}

		private async Task RunAsync()
		{
			int startRow = Console.CursorTop;

			for (int i = 0; i < _downloadsCount; i++)
			{
				Console.WriteLine();
			}

			var tasks = new List<Task>();
			for (int i = 0; i < _downloadsCount; i++)
			{
				tasks.Add(DownloadTaskAsync(i, startRow));
			}

			await Task.WhenAll(tasks);

			lock (_consoleLock)
			{
				Console.SetCursorPosition(0, startRow + _downloadsCount);
				Console.WriteLine("Все загрузки завершены.");
			}
		}

		private async Task DownloadTaskAsync(int index, int startRow)
		{
			var random = new Random();
			int progress = 0;
			int targetRow = startRow + index;

			UpdateProgressBar(index, targetRow, progress);

			while (progress < 100)
			{
				await Task.Delay(random.Next(200, 1000));
				progress += random.Next(5, 15);
				if (progress > 100) progress = 100;

				UpdateProgressBar(index, targetRow, progress);
			}
		}

		private void UpdateProgressBar(int index, int row, int progress)
		{
			int filledSegments = progress / 5;
			int emptySegments = TotalSegments - filledSegments;

			string bar = new string('#', filledSegments) + new string('-', emptySegments);
			string output = $"Загрузка {index + 1}: [{bar}] {progress}%";

			lock (_consoleLock)
			{
				int currentLeft = Console.CursorLeft;
				int currentTop = Console.CursorTop;

				Console.SetCursorPosition(0, row);
				Console.Write(new string(' ', Console.WindowWidth));
				Console.SetCursorPosition(0, row);
				Console.Write(output);

				Console.SetCursorPosition(currentLeft, currentTop);
			}
		}
	}
}