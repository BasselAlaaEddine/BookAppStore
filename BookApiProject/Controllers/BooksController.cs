using BookApiProject.Dtos;
using BookApiProject.Models;
using BookApiProject.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookApiProject.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : Controller
    {
        private IBookRepository _bookRepository;
        private IAuthorRepository _authorRepository;
        private ICategoryRepository _categoryRepository;
        private IReviewRepository _reviewRepository;

        public BooksController(IBookRepository bookRepository, IAuthorRepository authorRepository 
                             , ICategoryRepository categoryRepository , IReviewRepository reviewRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _categoryRepository = categoryRepository;
            _reviewRepository = reviewRepository;
        }



        ///Get All Books
        ///api/Books
        [ProducesResponseType(400)]
        [ProducesResponseType(200,Type=typeof(IEnumerable<BookDto>))]
        [HttpGet]
        public IActionResult GetBooks()
        {
            var Books = _bookRepository.GetBooks();
            
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var BooksDto = new List<BookDto>();

            foreach (var Book in Books)
            {
                BooksDto.Add(new BookDto()
                {
                    Id = Book.Id,
                    Isbn=Book.Isbn,
                    DatePublished =Book.DatePublished,
                    Title = Book.Title
                });
            }
            return Ok(BooksDto);
        }



        ///Get Book by Id
        ///api/Books/{bookId}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(BookDto))]
        [HttpGet("{bookId}")]
        public IActionResult GetBook(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
                return NotFound("Invalid Id ,This book doesn't exist");

            var Book = _bookRepository.GetBookById(bookId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var BooksDto = new BookDto()
            {
                Id = Book.Id,
                Isbn = Book.Isbn,
                DatePublished = Book.DatePublished,
                Title = Book.Title
            };

            return Ok(BooksDto);
        }


        ///Get Book by ISBN
        ///api/Books/ISBN/{bookIsbn}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(BookDto))]
        [HttpGet("ISBN/{bookIsbn}")]
        public IActionResult GetBook(string bookIsbn)
        {
            if (!_bookRepository.BookExists(bookIsbn))
                return NotFound("Invalid bookIsbn, This book doesn't exist");

            var Book = _bookRepository.GetBookByISBN(bookIsbn);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var BooksDto = new BookDto()
            {
                Id = Book.Id,
                Isbn = Book.Isbn,
                DatePublished = Book.DatePublished,
                Title = Book.Title
            };

            return Ok(BooksDto);
        }


        ///Get Book Rating
        ///api/Books/{bookId}/Rating
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(decimal))]
        [HttpGet("{bookId}/Rating")]
        public IActionResult GetBookRating(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
                return NotFound("Invalid Id, This book doesn't exist");

            var Rating = _bookRepository.GetBookRating(bookId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(Rating);
        }



        //Create New book , book's authors and book's categories must be in input uri 
        //api/Books/authorsId=1&authorsId=2&categoriesId=4&categoriesId=7
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        [ProducesResponseType(201, Type = typeof(Book))]
        [HttpPost]
        public IActionResult CreateBook([FromQuery] List<int> authorsId,[FromQuery] List<int> categoriesId, [FromBody] Book book)
        {
            var statusCode = BookValidation(authorsId, categoriesId, book);

            if (!ModelState.IsValid)
                return StatusCode(statusCode.StatusCode,ModelState);

            if (!_bookRepository.CreateBook(authorsId, categoriesId, book))
            {
                ModelState.AddModelError("", $"Something went wrong while saving Book:{book.Title}");
                return StatusCode(500, ModelState);
            }
            return Created("", "Book Successfully Created");
        }



        //Update Book ,book's authors and book's categories must be in input uri 
        //api/Books/bookId?authorsId=1&authorsId=2&categoriesId=4,categoriesId=7
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        [HttpPut("{bookId}")]
        public IActionResult UpdateBook(int bookId, [FromQuery] List<int> authorsId, [FromQuery] List<int> categoriesId, [FromBody] Book book)
        {
            var statusCode = BookValidation(authorsId, categoriesId, book);

            if (book.Id != bookId)
                return BadRequest();

            if (!ModelState.IsValid)
                return StatusCode(statusCode.StatusCode,ModelState);

            if (!_bookRepository.UpdateBook(authorsId, categoriesId, book))
            {
                ModelState.AddModelError("", $"Something went wrong while updating Book:{book.Title}");
                return StatusCode(500, ModelState);
            }
            return StatusCode(204, "Book Successfully Updated");
        }




        //Delete Book
        //api/Books/{bookId}
        [HttpDelete("{bookId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        public IActionResult DeleteBook(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
                return NotFound("Book Not Found");

            var bookToDelete = _bookRepository.GetBookById(bookId);
            var reviewsToDelete = _reviewRepository.GetReviewsOfABook(bookId);

            if (!ModelState.IsValid)
                return BadRequest();


            if (reviewsToDelete != null)
            {
                foreach (var review in reviewsToDelete)
                {
                    _reviewRepository.DeleteReview(review);
                }
            }

            if (!_bookRepository.DeleteBook(bookToDelete))
            {
                ModelState.AddModelError("", $"Something went wrong while deleting Book:{bookToDelete.Title}");
                return StatusCode(500, ModelState);
            }
            return StatusCode(204, "Book Successfully Deleted");
        }


        private StatusCodeResult BookValidation(List<int> authId, List<int> cateId, Book book)
        {
            if (book == null || authId.Count <= 0 || cateId.Count <= 0)
            {
                ModelState.AddModelError("", "Missing Input Book , Author or Category");
                return BadRequest();
            }
            if (_bookRepository.IsDuplicateIsbn(book.Id, book.Isbn))
            {
                ModelState.AddModelError("", "Book ISBN is already exist ,so cannot be duplicated");
                return UnprocessableEntity();
            }
            foreach (var id in authId)
            {
                if (!_authorRepository.AuthorExists(id))
                {
                    ModelState.AddModelError("", "Invalid authorId , Author is not found");
                    return NotFound();
                }
            }
            foreach (var id in cateId)
            {
                if (!_categoryRepository.CategoryExists(id))
                {
                    ModelState.AddModelError("", "Invalid categoryId , Category is not found");
                    return NotFound();
                }
            }
            if (!ModelState.IsValid)
            {

                return BadRequest();
            }

            return NoContent();
        }

    }
}
