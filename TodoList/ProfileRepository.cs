using System.Linq;
using TodoList.Data;

namespace TodoList.Services
{
	public class ProfileRepository
	{
		public void Save(Profile profile)
		{
			if (profile == null) return;

			using (AppDbContext context = new AppDbContext())
			{

				var oldProfiles = context.Profiles.ToList();
				if (oldProfiles.Any())
				{
					context.Profiles.RemoveRange(oldProfiles);
				}

				context.Profiles.Add(profile);
				context.SaveChanges();
			}
		}

		public Profile Load()
		{
			using (AppDbContext context = new AppDbContext())
			{

				return context.Profiles.FirstOrDefault();
			}
		}
	}
}