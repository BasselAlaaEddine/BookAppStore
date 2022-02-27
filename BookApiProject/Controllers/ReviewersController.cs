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
    public class ReviewersController : Controller
    {
        private IReviewerRepository _reviewerRepository;
        private IReviewRepository _reviewRepository;
        public ReviewersController(IReviewerRepository reviewerRepository, IReviewRepository reviewRepository)
        {
            _reviewerRepository = reviewerRepository;
            _reviewRepository   = reviewRepository;
        }

        ///Get All Reviewers
        ///api/Reviewers
        [ProducesResponseType(400)]
        [ProducesResponseType(200,Type =typeof(IEnumerable<ReviewerDto>))]
        [HttpGet]
        public IActionResult GetReviewers()
        {
            var Reviewers = _reviewerRepository.GetReviewers();
            var ReviewersDto =new List<ReviewerDto>();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            foreach (var Reviewer in Reviewers)
            {
                ReviewersDto.Add(new ReviewerDto()
                {
                    Id = Reviewer.Id,
                    FirstName = Reviewer.FirstName,
                    LastName = Reviewer.LastName
                });
            }
            return Ok(ReviewersDto);
        }



        ///Get Specific Reviewer by Id
        ///api/Reviewers/{reviewerId}
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(200,Type =typeof(ReviewerDto))]
        [HttpGet("{reviewerId}")]
        public IActionResult GetReviewer(int reviewerId)
        {
            if (!_reviewerRepository.ReviewerExists(reviewerId))
                return NotFound("Invalid Id, This reviewer doesn't exist");

            var Reviewer = _reviewerRepository.GetReviewer(reviewerId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ReviewerDto = new ReviewerDto()
            {
                Id = Reviewer.Id,
                FirstName = Reviewer.FirstName,
                LastName = Reviewer.LastName
            };

            return Ok(ReviewerDto);
        }



        ///Get  the Reviewer of a specific Review
        ///api/Reviewers/{reviewId}/Reviewer
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(ReviewerDto))]
        [HttpGet("{reviewId}/Reviewer")]
        public IActionResult GetReviewerofAReview(int reviewId)
        {

            if (!_reviewRepository.ReviewExists(reviewId))
                return NotFound("Invalid Id, This review doesn't exist");

            var Reviewer = _reviewerRepository.GetReviewerofAReview(reviewId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ReviewerDto = new ReviewerDto()
            {
                Id = Reviewer.Id,
                FirstName = Reviewer.FirstName,
                LastName = Reviewer.LastName
            };

            return Ok(ReviewerDto);
        }


        ///Get  Reviews of a specific Reviewer
        ///api/Reviewers/{reviewerId}/Reviews
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(ReviewDto))]
        [HttpGet("{reviewerId}/Reviews")]
        public IActionResult GetReviewsOfAReviewer(int reviewerId)
        {
            if (!_reviewerRepository.ReviewerExists(reviewerId))
                return NotFound("Invalid Id, This reviwer doesn't exist");

            var Reviews = _reviewerRepository.GetReviewsOfAReviewer(reviewerId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ReviewsDto = new List<ReviewDto>();

            foreach (var Review in Reviews)
            {
                ReviewsDto.Add(new ReviewDto()
                {
                    Id = Review.Id,
                    HeadLine= Review.HeadLine,
                    Rating = Review.Rating ,
                    ReviewText = Review.ReviewText
                });
            }
          

            return Ok(ReviewsDto);
        }


        // Create New Reviewer
        //PostMethod : api/Reviewers
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        [ProducesResponseType(201)]
        [HttpPost]
        public IActionResult CreateReviewer ([FromBody] Reviewer reviewerToCreate)
        {
            if (reviewerToCreate == null)
                return BadRequest(ModelState);

           var reviewDuplicated= _reviewerRepository.GetReviewers().Where(r => r.FirstName.Trim().ToUpper()== reviewerToCreate.FirstName.Trim().ToUpper()
                                                  && r.LastName.Trim().ToUpper()== reviewerToCreate.LastName.Trim().ToUpper()).FirstOrDefault();
            if (reviewDuplicated != null)
                return UnprocessableEntity($"Reviewer { reviewerToCreate.FirstName + reviewerToCreate.LastName} already exists");


            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_reviewerRepository.CreateReviewer(reviewerToCreate))
                return StatusCode(500, $"Something went wrong while saving{ reviewerToCreate.FirstName + reviewerToCreate.LastName}");
            return StatusCode(201);

        }



        //Update Reviewer 
        //PutMethod: api/Reviewers/{reviewerId}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        [HttpPut("{reviewerId}")]
        public IActionResult UpdateReviewer([FromBody] Reviewer reviewerToUpdate , int reviewerId)
        {
            if(reviewerToUpdate == null)
                return BadRequest(ModelState);


            if (reviewerToUpdate.Id != reviewerId)
                return BadRequest(ModelState);


            if (!_reviewerRepository.ReviewerExists(reviewerId))
                return NotFound("Invalid Id , This Reviewer doesn't exist to be updated");


            var reviewDuplicated = _reviewerRepository.GetReviewers().Where(r => r.FirstName.Trim().ToUpper() == reviewerToUpdate.FirstName.Trim().ToUpper()
                                                    && r.LastName.Trim().ToUpper() == reviewerToUpdate.LastName.Trim().ToUpper()).FirstOrDefault();

            if (reviewDuplicated != null)
                return UnprocessableEntity($"Reviewer { reviewerToUpdate.FirstName + reviewerToUpdate.LastName} already exists");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_reviewerRepository.UpdateReviewer(reviewerToUpdate))
                return StatusCode(500, $"Something went wrong while updating {reviewerToUpdate.FirstName + reviewerToUpdate.LastName}");
            return StatusCode(204, $"Reviewer updated Successfully");
        }



        //Delete Reviewer 
        //DeleteMethod:api/Reviewers/{reviewerId}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        [HttpDelete("{reviewerId}")]
        public IActionResult DeleteReviewer(int reviewerId)
        {

            if (!_reviewerRepository.ReviewerExists(reviewerId))
                return NotFound("Invalid Id ,This Reviewer doesn't exist to be deleted");

            var reviewerToDelete = _reviewerRepository.GetReviewer(reviewerId);
            var reviewsToDelete = _reviewerRepository.GetReviewsOfAReviewer(reviewerId);


            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
          
            if(!_reviewerRepository.DeleteReviewer(reviewerToDelete))
                return StatusCode(500, $"Something went wrong while deleting {reviewerToDelete.FirstName + " " + reviewerToDelete.LastName}");

            if (!_reviewRepository.DeleteReviews(reviewsToDelete.ToList()))
                return StatusCode(500, $"Something went wrong while deleting {reviewerToDelete.FirstName + " " + reviewerToDelete.LastName}");


            return StatusCode(204, "Reviewer deleted successfully");
        }

    }
}
