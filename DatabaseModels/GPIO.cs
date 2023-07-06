using System.ComponentModel.DataAnnotations;

namespace AVC.DatabaseModels
{
    public enum GPIO_TYPE
    {
        POWER, COUTER
    }
    public class GPIO
    {
        public virtual string name { get; set; }
        [Required]
        public virtual int port { get; set; }
        public virtual GPIO_TYPE type { get; set; }
        [Required]
        public virtual int value { get; set; }
    }
}