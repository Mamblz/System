using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CrmSystem.ViewModels
{
    public class RequestViewModel
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
