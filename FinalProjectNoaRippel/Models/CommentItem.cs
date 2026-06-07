using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Models
{
    public class CommentItem
    {
        public string? Key { get; set; }
        public string? Text { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
