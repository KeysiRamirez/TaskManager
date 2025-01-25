using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Data.Models
{
    public class TaskModel<TType>
    {
        public int TaskId { get; set; }
        public string TaskDescription { get; set; }
        public DateTime DueDate { get; set; }
        public bool TaskStatus { get; set; }
        public TType AdditionalData { get; set; }
    }
}
