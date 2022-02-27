using BookApiProject.Dtos;
using BookApiProject.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApiProject.Models;

namespace BookApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController:Controller
    {
    
        private ICategoryRepository _categoriesRepository;
        private IBookRepository _bookRepository;

        public CategoriesController(ICategoryRepository categoryRepository, IBookRepository bookRepository)
        {
            _categoriesRepository = categoryRepository;
            _bookRepository       = bookRepository;

        }



        ///Get all Categories
        ///"api/Categories"
        [ProducesResponseType(200,Type =typeof(IEnumerable<CategoryDto>))]
        [ProducesResponseType(400)]
        [HttpGet]
        public IActionResult GetCategories()
        {
            var Categories = _categoriesRepository.GetCategories();

            if (!ModelState.IsValid)
             return BadRequest(ModelState);
           
            var CategoriesDto = new List<CategoryDto>();

            foreach (var Category in Categories)
            {
                CategoriesDto.Add(new CategoryDto()
                {
                    Id = Category.Id,
                    Name = Category.Name
                });
            }

            return Ok(CategoriesDto);
        }



        ///Get Category by Id
        ///"api/Categories/{categoryId}"
        [ProducesResponseType(200, Type = typeof(CategoryDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpGet("{categoryId}")]
        public IActionResult GetCategory(int categoryId)
        {
            var Category = _categoriesRepository.GetCategory(categoryId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_categoriesRepository.CategoryExists(categoryId))
                return NotFound("Invalid Id, This category doesn't exist");

            var CategoryDto = new CategoryDto()
            {
                Id =   Category.Id,
                Name = Category.Name
            };
        
            return Ok(CategoryDto);
        }


        ///Get All Categories of a specific book
        ///"api/Categories/Books/{bookId}"
        [ProducesResponseType(200, Type = typeof(IEnumerable<CategoryDto>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpGet("Books/{bookId}")]
        public IActionResult GetCategoriesOfABook(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
                return NotFound("Invalid bookId, This book doesn't exist");

            var Categories = _categoriesRepository.GetCategoriesOfABook(bookId).ToList();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var CategoriesDto = new List<CategoryDto>();

            foreach (var Category in Categories)
            {
                CategoriesDto.Add(new CategoryDto()
                {
                    Id = Category.Id,
                    Name = Category.Name
                });
            }
            return Ok(CategoriesDto);
        }



        // Get all book of a specific category
        ///api/Categories/{categoryId}/Books
        [ProducesResponseType(400)]
        [ProducesResponseType(200,Type =typeof(IEnumerable<BookDto>))]
        [ProducesResponseType(404)]
        [HttpGet("{categoryId}/Books")]
        public IActionResult GetBooksOfACategory(int categoryId)
        {
            if (!_categoriesRepository.CategoryExists(categoryId))
                return NotFound("Invalid Id , This Category doesn't exist");

            var Books = _categoriesRepository.GetBooksOfACategory(categoryId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var BookDto = new List<BookDto>();
            foreach (var Book in Books)
            {
                BookDto.Add(new BookDto()
                {
                    Id = Book.Id,
                    Isbn = Book.Isbn,
                    Title = Book.Title,
                    DatePublished=Book.DatePublished
                });
            }
            return Ok(BookDto);
        }




        //Create New Category 
        //PostMethod : api/Categories
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        [ProducesResponseType(201)]
        [HttpPost]
        public IActionResult CreateCategory ([FromBody]  Category categoryToCreate)
        {
            // check if request is null
            if (categoryToCreate == null)
                return BadRequest(ModelState);

            // check duplication
            var CategoryExist = _categoriesRepository
                               .GetCategories()
                               .Where(c => c.Name.Trim().ToUpper() == categoryToCreate.Name.Trim().ToUpper())
                               .FirstOrDefault();

            if (CategoryExist != null)
                return UnprocessableEntity( $"Category { categoryToCreate.Name} already exists");

            // check model state validation 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // adding the new country and check saving validation
            if (!_categoriesRepository.CreateCategory(categoryToCreate))
            {
                ModelState.AddModelError("", $"Something went wrong while saving{categoryToCreate.Name}");
                return StatusCode(500, ModelState);
            }
            return StatusCode(201);
        }




        // Update Category
        //PutMethod : api/Categories/{categoryId}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        [ProducesResponseType(201)]
        [HttpPut("{categoryId}")]
        public IActionResult UpdateCategory([FromBody]  Category categoryToUpdate , int categoryId)
        {
            // check if request is null
            if (categoryToUpdate == null)
                return BadRequest(ModelState);


            // validate request body Id is equal to request url param (Id)
            if (categoryId != categoryToUpdate.Id)
                return BadRequest(ModelState);

            // check if a category already exist in my DBset
            if (!_categoriesRepository.CategoryExists(categoryId))
                return NotFound("Invalid Id ,This Category doesn't exist to be updated");

            // check duplication
            if (_categoriesRepository.IsDuplicateCategoryName(categoryId,categoryToUpdate.Name))
                return UnprocessableEntity($"Categories {categoryToUpdate.Name} already exists");

            // check model state validation 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // adding the new country and check saving validation
            if (!_categoriesRepository.UpdateCategory(categoryToUpdate))
            {
                ModelState.AddModelError("", $"Something went wrong while updating{categoryToUpdate.Name}");
                return StatusCode(500, ModelState);
            }
            return StatusCode(204,"Category Successfully Updated");
        }




        // Delete category 
        //DeleteMethod : api/Categories/{categoryId}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        [ProducesResponseType(201)]
        [HttpDelete("{categoryId}")]
        public IActionResult DeleteCategory(int categoryId)
        {
          
            // check if a category already exist in my DBset
            if (!_categoriesRepository.CategoryExists(categoryId))
                return NotFound("Invalid Id ,This Category doesn't exist to be deleted");

            var categoryToDelete = _categoriesRepository.GetCategory(categoryId);

            // check model state validation 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // check category contains books
            if (_categoriesRepository.GetBooksOfACategory(categoryId).Count> 0)
                return Conflict($"Category {categoryToDelete.Name}" + " cannot be deleted because it Contians at least one book");


            // adding the new country and check saving validation
            if (!_categoriesRepository.DeleteCategory(categoryToDelete))
            {
                ModelState.AddModelError("", $"Something went wrong while saving{categoryToDelete.Name}");
                return StatusCode(500, ModelState);
            }
            return StatusCode(204, "Category Successfully Deleted");
        }



    }
}
