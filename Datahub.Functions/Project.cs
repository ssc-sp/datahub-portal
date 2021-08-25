using System.Collections.Generic;
using System.Linq;

namespace NRCanDataHub
{
    public class Project
    {
        public static Project FromData(string projectName, string dbConnString, string adminUsers, string regularUsers)
        {
            return new Project
            {
                Name = projectName,
                DBConnString = dbConnString,
                Users = adminUsers.Split(";")
                    .Select(x => new User() { Username = x.Trim(), IsAdmin = true })
                    .Union(regularUsers.Split(";").Select(x => new User() { Username = x.Trim(), IsAdmin = false }))
                    .Where(x => x.Username.Trim() != "")
            };
        }
        public string Name { get; set; }
        public string DBConnString { get; set; }
        public IEnumerable<User> Users { get; set; }

    }
}
