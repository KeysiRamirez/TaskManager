using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Data.Entities
{
    [Table("Task")]
    public sealed class TaskEntity
    {
        // default task status 
        public TaskEntity() 
        {
            TaskStatus = false; // Pending 
        }

        [Key]
        [Column("TaskId")]
        public int TaskId { get; set; }


        [Required(ErrorMessage = "La descripcion no puede estar vacia")]
        [StringLength(100, ErrorMessage = "La longitud de la descripcion es inválida.")]
        public string TaskDescription { get; set; }

        [Required(ErrorMessage = "La fecha no puede estar vacia")] public DateTime DueDate { get; set; }

        public bool TaskStatus { get; set; } 
        //public T AdditionalData { get; set; }

    }
}
