using System;
using System.Collections.Generic;
using System.Linq;
using TodoList.Data;

namespace TodoList.Services
{
	public class TodoRepository
	{
		public List<TodoItem> GetAll()
		{
			using (AppDbContext context = new AppDbContext())
			{
				return context.Todos.ToList();
			}
		}

		public void Add(TodoItem item)
		{
			if (item == null) return;

			using (AppDbContext context = new AppDbContext())
			{
				context.Todos.Add(item);
				context.SaveChanges();
			}
		}

		public void Update(TodoItem item)
		{
			if (item == null) return;

			using (AppDbContext context = new AppDbContext())
			{
				context.Todos.Update(item);
				context.SaveChanges();
			}
		}

		public void Delete(int id)
		{
			using (AppDbContext context = new AppDbContext())
			{
				var item = context.Todos.FirstOrDefault(t => t.Id == id);
				if (item != null)
				{
					context.Todos.Remove(item);
					context.SaveChanges();
				}
			}
		}

		public void SetStatus(int id, TodoStatus status)
		{
			using (AppDbContext context = new AppDbContext())
			{
				var item = context.Todos.FirstOrDefault(t => t.Id == id);
				if (item != null)
				{
					item.Status = status;
					context.SaveChanges();
				}
			}
		}
	}
}