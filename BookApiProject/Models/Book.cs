using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookApiProject.Models
{
    public class Book
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(10,MinimumLength =3,ErrorMessage ="ISBN must be between 3 to 10 char")]
        public string Isbn { get; set; }

        [Required]
        [MaxLength(100,ErrorMessage ="Title Cannot be more than 100 char")]
        public string Title { get; set; }

        public DateTime? DatePublished { get; set; }

        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<BookAuthor>BookAuthors { get; set; }
        public virtual ICollection<BookCategory> BookCategories { get; set; }
    }
}
