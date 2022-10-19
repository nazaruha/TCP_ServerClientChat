using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Context.Entities
{
    [Table("tblUserMessages")]
    public class UserMessage
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(50)] 
        public string Name { get; set; }
        [Required, StringLength(1000)]
        public string Message { get; set; }
        [Required]
        public DateTime Date { get; set; }

    }
}
