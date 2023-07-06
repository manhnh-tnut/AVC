using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace AVC.DatabaseModels
{
    public class Machine : BaseEntity
    {
        [Required]
        public virtual string ip { get; set; }
        [Required]
        public virtual string name { get; set; }
        [Required]
        public virtual bool status { get; set; }
        public virtual List<GPIO> gpio { get; set; }
    }
}