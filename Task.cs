using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsanaGraphVisualizer
{
    class Task
    {

        //Task Details
        //https://app.asana.com/api/1.0/tasks/11336810799207?opt_pretty&opt_expand=.

        public long Id { get; set; }

        public DateTime CreatedUTC { get; set; }
        public DateTime ModifiedUTC { get; set; }


        public bool Completed { get; set; }
        public DateTime? CompletedUTC { get; set; }
        public DateTime? DueOnUTC { get; set; }


        public string Name { get; set; }
        public string Notes { get; set; }


        public User Assignee { get; set; }
        public string AssigneeStatus { get; set; }

        


        



        public Dictionary<long,User> Followers { get; set; }
        public Task ParentTask { get; set; }
        public Dictionary<long, Task> SubTasks { get; set; }


        public Task()
        {
            Followers = new Dictionary<long, User>();
            SubTasks = new Dictionary<long, Task>();
        }

        
        



        public override string ToString()
        {
            return Name;
        }


    }
}

