using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsanaGraphVisualizer
{
    class Team
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public Dictionary<long,Project> Projects { get; set; }
        public Dictionary<long, User> Users { get; set; }

        public Team()
        {
            this.Projects = new Dictionary<long, Project>();
            this.Users = new Dictionary<long, User>();
        }


        public override string ToString()
        {
            return Name;
        }
    }
}
