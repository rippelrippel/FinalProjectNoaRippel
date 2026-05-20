using System;
using System.Collections.Generic;

namespace FinalProjectNoaRippel.Models
{
    public class Recipe
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? ImageSource { get; set; }
        public string? CategoryName { get; set; }
        public string? UserId { get; set; }
        public List<string>? Ingredients { get; set; }
        public List<string>? Instructions { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}