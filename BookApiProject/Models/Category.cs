using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookApiProject.Models
{
    public class Category
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50,ErrorMessage ="Category Name Cannot be more than 50 Char")]
        public string Name { get; set; }

        public virtual ICollection<BookCategory> BookCategories{ get; set; }
    }
}
