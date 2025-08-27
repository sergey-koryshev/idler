namespace Idler.Models
{
    using System;

    public class ShiftNote
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public string Category { get; set; }

        public decimal Effort { get; set; }

        public string Description { get; set; }

        public DateTime Date { get; set; }
    }
}