using Microsoft.EntityFrameworkCore;

namespace TodoList.Data
{
	public class AppDbContext : DbContext
	{
		public DbSet<TodoItem> Todos { get; set; }
		public DbSet<Profile> Profiles { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Data Source=todos.db");
		}
	}
}