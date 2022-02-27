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

    public class ReviewsController : Controller
    {
        private IReviewRepository _reviewRepository;
        private IBookRepository _bookRepository;
        private IReviewerRepository _reviewerRepository;
        public ReviewsController(IReviewRepository reviewRepository, IBookRepository bookRepository, IReviewerRepository reviewerRepository)
        {
            _reviewRepository = reviewRepository;
            _bookRepository = bookRepository;
            _reviewerRepository = reviewerRepository;
        }

      
        
        ///Get All Reviews
        ///api/Reviews
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDto>))]
        [HttpGet]
        public IActionResult GetReviews()
        {
            var Reviews = _reviewRepository.GetReviews();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ReviewsDto = new List<ReviewDto>();

            foreach (var Review in Reviews)
            {
                ReviewsDto.Add(new ReviewDto()
                {
                    Id = Review.Id,
                    HeadLine = Review.HeadLine,
                    ReviewText = Review.ReviewText,
                    Rating = Review.Rating
                });
            }
            return Ok(ReviewsDto);
        }


        ///Get A specific Review
        ///api/Reviews/{reviewId}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDto>))]
        [HttpGet("{reviewId}")]
        public IActionResult GetReview(int reviewId)
        {
            if (!_reviewRepository.ReviewExists(reviewId))
                return NotFound("Invalid Id, This review doesn't exist");

            var Review = _reviewRepository.GetReview(reviewId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ReviewDto = new ReviewDto()
            {
                Id = Review.Id,
                HeadLine = Review.HeadLine,
                ReviewText = Review.ReviewText,
                Rating = Review.Rating
            };

            return Ok(ReviewDto);
        }



        ///Get Reviews Of A specific Book
        ///api/Reviews/Books/{bookId}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDto>))]
        [HttpGet("Books/{bookId}")]
        public IActionResult GetReviewsOfABook(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
                return NotFound("Invalid bookId, This book doesn't exist");

            var Reviews = _reviewRepository.GetReviewsOfABook(bookId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ReviewsDto = new List<ReviewDto>();

            foreach (var Review in Reviews)
            {
                ReviewsDto.Add(new ReviewDto()
                {
                    Id = Review.Id,
                    HeadLine = Review.HeadLine,
                    ReviewText = Review.ReviewText,
                    Rating = Review.Rating
                });
            }
            return Ok(ReviewsDto);

        }


        ///Get A Review Book
        ///api/Reviews/{reviewId}/Book
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(BookDto))]
        [HttpGet("{reviewId}/Book")]
        public IActionResult GetBookOfAReview(int reviewId)
        {
            if (!_reviewRepository.ReviewExists(reviewId))
                return NotFound("Invalid Id, This review doesn't exist");

            var Book = _reviewRepository.GetBookOfAReview(reviewId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var BookDto = new BookDto()
            {
                Id = Book.Id,
                Isbn = Book.Isbn,
                DatePublished = Book.DatePublished,
                Title = Book.Title
            };

            return Ok(BookDto);
        }



        //Create New Review for a book
        //PostMethod: api/Review
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(201)]
        [HttpPost]
        public IActionResult CreateReview([FromBody] Review reviewToCreate)
        {
            if (reviewToCreate == null)
                return BadRequest(ModelState);

            if (!_bookRepository.BookExists(reviewToCreate.Book.Id))
                ModelState.AddModelError("", "Invalid bookId , Book doesn't exist!");

            if (!_reviewerRepository.ReviewerExists(reviewToCreate.Reviewer.Id))
                ModelState.AddModelError("", "Invalid reviewerId , Reviewer doesn't exist!");

            if (!ModelState.IsValid)
                return StatusCode(404,ModelState);

            reviewToCreate.Book = _bookRepository.GetBookById(reviewToCreate.Book.Id);
            reviewToCreate.Reviewer = _reviewerRepository.GetReviewer(reviewToCreate.Reviewer.Id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_reviewRepository.CreateReview(reviewToCreate))
                return StatusCode(500, $"Something went wrong while saving{reviewToCreate.HeadLine} ");


            return StatusCode (201);
        }


        //Update Review of a book
        //PutMethod:api/Reviews/{reviewId}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        [HttpPut("{reviewId}")]
        public IActionResult UpdateReview([FromBody] Review reviewToUpdate , int reviewId)
        {
            if (reviewToUpdate == null)
                return BadRequest(ModelState);

            if (reviewId != reviewToUpdate.Id)
                return BadRequest(ModelState);

            if(!_reviewRepository.ReviewExists(reviewId))
                ModelState.AddModelError("", "Invalid Id ,Review doesn't exist to be updated!");

            if (!_bookRepository.BookExists(reviewToUpdate.Book.Id))
                ModelState.AddModelError("", "Invalid bookId, Book doesn't exist!");

            if (!_bookRepository.BookExists(reviewToUpdate.Reviewer.Id))
                ModelState.AddModelError("", "Invalid reviewerId , Reviewer doesn't exist!");

            if (!ModelState.IsValid)
                return NotFound(ModelState);

            reviewToUpdate.Book = _bookRepository.GetBookById(reviewToUpdate.Book.Id);
            reviewToUpdate.Reviewer = _reviewerRepository.GetReviewer(reviewToUpdate.Reviewer.Id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_reviewRepository.UpdateReview(reviewToUpdate))
                return StatusCode(500, $"Something went wrong while updating{reviewToUpdate.HeadLine} ");

            ModelState.AddModelError("","review updated Successfully");
            return StatusCode(204, ModelState);
        }




        //Delete Review of a book
        //DeleteMethod:api/Reviews/{reviewId}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        [HttpDelete("{reviewId}")]
        public IActionResult DeleteReview (int reviewId)
        {
            if (!_reviewRepository.ReviewExists(reviewId))
                return NotFound("Invalid Id ,Review doesn't exist to be deleted!");

            var reviewToDelete = _reviewRepository.GetReview(reviewId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_reviewRepository.DeleteReview(reviewToDelete))
                return StatusCode(500, $"Something went wrong while deleting {reviewToDelete.HeadLine}");

            return StatusCode(204, "Review deleted successfully");  
            
        }
    }
}
