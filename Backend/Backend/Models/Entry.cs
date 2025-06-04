using System.ComponentModel.DataAnnotations;

namespace Jour2.Models
{
    public class Entry
    {
        public int EntryId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Json { get; set; } = string.Empty;
    }
}
