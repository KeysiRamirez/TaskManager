using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Data.Models
{
   public class UserModel
   
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string UserPassword { get; set; } 
        public string Role { get; set; } 
    }

}
