using System.ComponentModel.DataAnnotations;
namespace ASPCRUDV3.Models
{
    public class DataItem
    {
        [Key]
        public int Id { get; set; } // Primary key
        public string ItemId { get; set; }
        public string ChargisRange { get; set; }
        public string EntryTime { get; set; }
        public string ExitTime { get; set; }
        public string Note { get; set; }
        public string Line { get; set; }
        public string Shift { get; set; } // Add this property
    }

}
