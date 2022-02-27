using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookApiProject.Models
{
    public class Review
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200,MinimumLength =10,ErrorMessage ="Review HeadLine should be between 10 - 200 char")]
        public string HeadLine { get; set; }

        [Required]
        [StringLength(2000,MinimumLength =50, ErrorMessage = "Review Text should be between 50 - 2000 char")]
        public string ReviewText { get; set; }

        [Required]
        [Range(1,5,ErrorMessage ="Rating Must be between 1 to 5 stars")]
        public int Rating { get; set; }

        public  virtual Reviewer Reviewer { get; set; }
        public virtual Book Book { get; set; }
    }
}
