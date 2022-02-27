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
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : Controller
    {
        private IAuthorRepository _authorRepository;
        private IBookRepository   _bookRepository;
        private ICountryRepository _countryRepository;
        public AuthorsController(IAuthorRepository authorRepository , IBookRepository bookRepository , ICountryRepository countryRepository)
        {
            _authorRepository = authorRepository;
            _bookRepository   = bookRepository;
            _countryRepository = countryRepository;
        }



        ///Get All Authours
        ///api/Authors
        [ProducesResponseType(400)]
        [ProducesResponseType(200,Type =typeof(IEnumerable<AuthorDto>))]
        [HttpGet]
        public IActionResult GetAuthors()
        {
            var Authors = _authorRepository.GetAuthors();
            var AuthorsDto = new List<AuthorDto>();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            foreach (var Author in  Authors)
            {
                AuthorsDto.Add(new AuthorDto()
                {
                    Id = Author.Id,
                    FirstName = Author.FirstName,
                    LastName = Author.LastName
                });
            }

            return Ok(AuthorsDto);
        }



        ///Get An Authour by Id
        ///api/Authors/{authorId}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(AuthorDto))]
        [HttpGet("{authorId}")]
        public IActionResult GetAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
                return NotFound("Invalid Id, This author doesn't exist");

            var Author = _authorRepository.GetAuthor(authorId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var AuthorDto = new AuthorDto()
            {
                Id = Author.Id,
                FirstName = Author.FirstName,
                LastName = Author.LastName
            };

           
            return Ok(AuthorDto);
        }



        ///Get Authors for book
        ///api/Authors/Books/{bookId}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<AuthorDto>))]
        [HttpGet("Books/{bookId}")]
        public IActionResult GetAuthorsOfABook(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
                return NotFound("Invalid bookId, This book doesn't exist");

            var Authors = _authorRepository.GetAuthorsOfABook(bookId);
            var AuthorsDto = new List<AuthorDto>();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            foreach (var Author in Authors)
            {
                AuthorsDto.Add(new AuthorDto()
                {
                    Id = Author.Id,
                    FirstName = Author.FirstName,
                    LastName = Author.LastName
                });
            }

            return Ok(AuthorsDto);
        }



        ///Get All book for Specific author
        ///api/Authors/{authorId}/Books
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<AuthorDto>))]
        [HttpGet("{authorId}/Books")]
        public IActionResult GetBooksOfAnAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
                return NotFound("Invalid Id, This author doesn't exist");

            var Books = _authorRepository.GetBooksOfAnAuthor(authorId);
            var BooksDto = new List<BookDto>();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            foreach (var Book in Books)
            {
                BooksDto.Add(new BookDto()
                {
                    Id = Book.Id,
                    Isbn= Book.Isbn,
                    Title = Book.Title,
                    DatePublished=Book.DatePublished
                });
            }

            return Ok(BooksDto);
        }


         
        //Create  New Author 
        //api/Authors
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        [ProducesResponseType(201,Type =typeof(Author))]
        [HttpPost]
        public IActionResult CreateAuthor([FromBody] Author authorToCreate)
        {
            if (authorToCreate == null)
                return BadRequest(ModelState);

            var authorDuplicated =_authorRepository.GetAuthors().Where(a => a.FirstName.Trim().ToUpper() == authorToCreate.FirstName.Trim().ToUpper()
                                                                   &&a.LastName.Trim().ToUpper()== authorToCreate.LastName.Trim().ToUpper())
                                                                   .FirstOrDefault();
            if (authorDuplicated != null)
                return UnprocessableEntity($"Author { authorToCreate.FirstName+" "+authorToCreate.LastName} already exists");


            if (!_countryRepository.CountryExists(authorToCreate.Country.Id))
                return BadRequest("Author's country doesn't exist");

            authorToCreate.Country = _countryRepository.GetCountry(authorToCreate.Country.Id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_authorRepository.CreateAuthor(authorToCreate))
                return StatusCode(500, $"Something wents wrong while Saving {authorToCreate.FirstName + " " + authorToCreate.LastName}");

            return StatusCode(201, "Author Successfully Created");

        }



        //Update Author
        //api/Authors/{authorId}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        [HttpPut("{authorId}")]
        public IActionResult UpdateAuthor([FromBody]  Author authorToUpdate, int authorId)
        {
            // check if request is null
            if (authorToUpdate == null)
                return BadRequest(ModelState);


            // validate request body Id is equal to request url param (Id)
            if (authorToUpdate.Id!=authorId)
                return BadRequest(ModelState);

            // check if a Author already exist in my DBset
            if (!_authorRepository.AuthorExists(authorId))
                return NotFound("Invalid Id ,This author doesn't exist to be updated");

            if (!_countryRepository.CountryExists(authorToUpdate.Country.Id))
                return NotFound("Invalid countryId , This Country doesn't exist");
       

            // check model state validation 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // adding the new country and check saving validation
            if (!_authorRepository.UpdateAuthor(authorToUpdate))
            {
                ModelState.AddModelError("", $"Something went wrong while saving{authorToUpdate.FirstName}");
                return StatusCode(500, ModelState);
            }
            return StatusCode(204, "Author Successfully Updated");
        }



        //Delete Author
        //api/Authors/{authorId}
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        [HttpDelete("{authorId}")]
        public IActionResult DeleteAuthor(int authorId)
        {
            // check if a author already exist in my DBset
            if (!_authorRepository.AuthorExists(authorId))
                return NotFound("Invalid Id ,This Author doesn't exist to be deleted");

            var authorToDelete = _authorRepository.GetAuthor(authorId);

            // check model state validation 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // check author wrote books
            if (_authorRepository.GetBooksOfAnAuthor(authorId).Count > 0)
                return Conflict($"Author {authorToDelete.FirstName + " " +authorToDelete.LastName }" + " cannot be deleted because it wrote at least one book");


             if (!_authorRepository.DeleteAuthor(authorToDelete))
            {
                ModelState.AddModelError("", $"Something went wrong while deleting{authorToDelete.FirstName + " " + authorToDelete.LastName }");
                return StatusCode(500, ModelState);
            }
            return StatusCode(204, "Author Successfully Deleted");
        }
    
    }
}
