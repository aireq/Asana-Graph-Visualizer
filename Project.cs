using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsanaGraphVisualizer
{
    class Project
    {
        public long Id { get; set; }

        public DateTime CreatedUTC { get; set; }
        public DateTime ModifiedUTC { get; set; }

        public bool Public { get; set; }

        public string Name { get; set; }
        public string Notes { get; set; }

        public bool Archived { get; set; }


        



        /// <summary>
        /// Project should link to tasks as tasks may be found before the associated project
        /// </summary>
        public Dictionary<long,Task> Tasks { get; set; }


        public Project()
        {
            Tasks = new Dictionary<long, Task>();
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
