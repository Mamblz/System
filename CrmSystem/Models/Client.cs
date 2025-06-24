using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmSystem.Models
{
    public class Client
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Company { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Interaction> Interactions { get; set; } = new();
    }

    public class Interaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime Date { get; set; }
        public string Description { get; set; }
        public InteractionType Type { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }
    }
}
