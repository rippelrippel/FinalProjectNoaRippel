using System;
using System.Collections.Generic;

namespace FinalProjectNoaRippel.Models
{
    //מחלקת מודל המתאר מתכון 

    public class Recipe
    {

        public string? Id { get; set; }//מפתח של הפיירבייס
        public string? Name { get; set; }
        public string? ImageSource { get; set; }
        public string? CategoryName { get; set; }
        public List<string>? Ingredients { get; set; }
        public List<string>? Instructions { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<string>? Tags { get; set; }
        public string? PrepTime { get; set; }
    }
}