using System;
using System.ComponentModel.DataAnnotations;

namespace CrmSystem.Models
{
    public class TasksItems
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
