using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsanaGraphVisualizer
{
    class User
    {
        //user details
        //https://app.asana.com/api/1.0/users/11044758815686

        public long Id { get; set; }
        public string Name { get; set; }
        
        public override string ToString()
        {
            return Name;
        }


    }
}

