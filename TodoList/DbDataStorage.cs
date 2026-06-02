using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace TodoList
{
	public class DbDataStorage : IDataStorage
	{
		private readonly string _connectionString;

		public DbDataStorage(string dbPath)
		{
			_connectionString = "Data Source=" + dbPath;
			InitializeDatabase();
		}

		private void InitializeDatabase()
		{
			using (SqliteConnection connection = new SqliteConnection(_connectionString))
			{
				connection.Open();

				string createProfileTable = @"
					CREATE TABLE IF NOT EXISTS Profiles (
						Id INTEGER PRIMARY KEY AUTOINCREMENT,
						FirstName TEXT NOT NULL,
						LastName TEXT NOT NULL,
						BirthYear INTEGER NOT NULL
					);";

				string createTodosTable = @"
					CREATE TABLE IF NOT EXISTS Todos (
						Id INTEGER PRIMARY KEY AUTOINCREMENT,
						UserId TEXT NOT NULL,
						TaskText TEXT NOT NULL,
						Status TEXT NOT NULL,
						CreatedAt TEXT NOT NULL
					);";

				using (SqliteCommand command = new SqliteCommand(createProfileTable, connection))
				{
					command.ExecuteNonQuery();
				}

				using (SqliteCommand command = new SqliteCommand(createTodosTable, connection))
				{
					command.ExecuteNonQuery();
				}
			}
		}

		public void SaveProfile(Profile profile)
		{
			if (profile == null) return;

			using (SqliteConnection connection = new SqliteConnection(_connectionString))
			{
				connection.Open();

				string deleteQuery = "DELETE FROM Profiles;";
				using (SqliteCommand deleteCmd = new SqliteCommand(deleteQuery, connection))
				{
					deleteCmd.ExecuteNonQuery();
				}

				string insertQuery = "INSERT INTO Profiles (FirstName, LastName, BirthYear) VALUES (@firstName, @lastName, @birthYear);";
				using (SqliteCommand insertCmd = new SqliteCommand(insertQuery, connection))
				{
					insertCmd.Parameters.AddWithValue("@firstName", profile.FirstName);
					insertCmd.Parameters.AddWithValue("@lastName", profile.LastName);
					insertCmd.Parameters.AddWithValue("@birthYear", profile.BirthYear);
					insertCmd.ExecuteNonQuery();
				}
			}
		}

		public Profile LoadProfile()
		{
			using (SqliteConnection connection = new SqliteConnection(_connectionString))
			{
				connection.Open();
				string selectQuery = "SELECT FirstName, LastName, BirthYear FROM Profiles LIMIT 1;";

				using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
				{
					using (SqliteDataReader reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							string firstName = reader.GetString(0);
							string lastName = reader.GetString(1);
							int birthYear = reader.GetInt32(2);
							return new Profile(firstName, lastName, birthYear);
						}
					}
				}
			}
			return null;
		}

		public void SaveTodos(TodoList todos)
		{
			if (todos == null) return;

			string userId = AppInfo.CurrentProfile != null ? AppInfo.CurrentProfile.FirstName : "default_user";

			using (SqliteConnection connection = new SqliteConnection(_connectionString))
			{
				connection.Open();

				using (SqliteTransaction transaction = connection.BeginTransaction())
				{
					try
					{

						string deleteQuery = "DELETE FROM Todos WHERE UserId = @userId;";
						using (SqliteCommand deleteCmd = new SqliteCommand(deleteQuery, connection, transaction))
						{
							deleteCmd.Parameters.AddWithValue("@userId", userId);
							deleteCmd.ExecuteNonQuery();
						}

						string insertQuery = "INSERT INTO Todos (UserId, TaskText, Status, CreatedAt) VALUES (@userId, @taskText, @status, @createdAt);";

						foreach (TodoItem item in todos.GetAllTasks())
						{
							using (SqliteCommand insertCmd = new SqliteCommand(insertQuery, connection, transaction))
							{
								insertCmd.Parameters.AddWithValue("@userId", userId);
								insertCmd.Parameters.AddWithValue("@taskText", item.Text);
								insertCmd.Parameters.AddWithValue("@status", item.Status.ToString());
								insertCmd.Parameters.AddWithValue("@createdAt", item.CreatedAt.ToString("o"));
								insertCmd.ExecuteNonQuery();
							}
						}

						transaction.Commit();
					}
					catch (Exception)
					{
						transaction.Rollback();
						throw;
					}
				}
			}
		}

		public TodoList LoadTodos()
		{
			TodoList todoList = new TodoList();
			string userId = AppInfo.CurrentProfile != null ? AppInfo.CurrentProfile.FirstName : "default_user";

			using (SqliteConnection connection = new SqliteConnection(_connectionString))
			{
				connection.Open();
				string selectQuery = "SELECT TaskText, Status, CreatedAt FROM Todos WHERE UserId = @userId;";

				using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
				{
					command.Parameters.AddWithValue("@userId", userId);

					using (SqliteDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							string text = reader.GetString(0);
							string statusStr = reader.GetString(1);
							DateTime createdAt = DateTime.Parse(reader.GetString(2));

							TodoStatus status = (TodoStatus)Enum.Parse(typeof(TodoStatus), statusStr, true);

							TodoItem item = new TodoItem(text, status, createdAt);
							todoList.AddTask(item);
						}
					}
				}
			}
			return todoList;
		}
	}
}