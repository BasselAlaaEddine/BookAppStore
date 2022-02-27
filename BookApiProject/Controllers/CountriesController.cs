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
    public class CountriesController: Controller
    {
        private ICountryRepository _countryRepository;
        private IAuthorRepository _authorRepository;
        public CountriesController(ICountryRepository countryRepository, IAuthorRepository authorRepository)
        {
            _countryRepository = countryRepository;
            _authorRepository  = authorRepository;
        }



        //Get all Countries
        ///api/Countries
        [ProducesResponseType(400)]
        [ProducesResponseType(200,Type =typeof(IEnumerable<CountryDto>))]
        [HttpGet]
        public IActionResult GetCountries()
        {
            var Countries = _countryRepository.GetCountries().ToList();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var countryDtos = new List<CountryDto>();
            foreach (var Country in Countries)
            {
                countryDtos.Add(new CountryDto()
                {
                    Id = Country.Id,
                    Name = Country.Name
                });
            }
            return Ok(countryDtos);
        }




        //Get Specific Country
        ///api/Countries/{county_id}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(CountryDto))]
        [HttpGet("{countryId}")]
        public IActionResult GetCountry(int countryId)
        {
            if (_countryRepository.CountryExists(countryId)==false)
                return NotFound("Invalid Id, This country doesn't exist");
           
           Country Country = _countryRepository.GetCountry(countryId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var countryDto = new CountryDto()
            {
                Id = Country.Id,
                Name = Country.Name
            };

            return Ok(countryDto);
        }


        //Get Country of An Author
        ///api/Countries/Authors/{author_id}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(CountryDto))]
        [HttpGet("Authors/{authorId}")]
        public IActionResult GetCountryOfAnAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
                return NotFound("Invalid authorId , This author doesn't exist");

            Country Country = _countryRepository.GetCountryOfAnAuthor(authorId);
          
            
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            

            CountryDto CountryDto = new CountryDto()
            {
                Id = Country.Id,
                Name = Country.Name
            };

            return Ok(CountryDto);
        }



        // Get All Author of An Country
        ///api/Countries/{countryId}/Authors
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(200,Type =typeof(IEnumerable<AuthorDto>))]
        [HttpGet("{countryId}/Authors")]
        public IActionResult GetAuthorsOfACountry(int countryId)
        {
            if (!_countryRepository.CountryExists(countryId))
                return NotFound("Invalid Id, This country doesn't exist");

            var Authors = _countryRepository.GetAuthorsOfACountry(countryId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var AuthorsDto = new List<AuthorDto>();

            foreach (var Author in Authors)
            {
                AuthorsDto.Add(new AuthorDto()
                {
                    Id= Author.Id,
                    FirstName=Author.FirstName,
                    LastName = Author.LastName
                });
            }
            return Ok(AuthorsDto);
        }



        ///Add New Country 
        ///Post Method
        ///api/Countries
        [HttpPost]
        [ProducesResponseType(422)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(201, Type = typeof(Country))]
        public IActionResult CreateCountry([FromBody] Country countryToCreate)
        {
            // check if request is null
            if (countryToCreate == null)
                return BadRequest(ModelState);

            // check if new country is duplicated 
            var CountryIfExist = _countryRepository.GetCountries()
                                  .Where(c => c.Name.Trim().ToUpper() == countryToCreate.Name.Trim().ToUpper())
                                  .FirstOrDefault();

            if (CountryIfExist != null)
            {
                ModelState.AddModelError("", $"Country {countryToCreate.Name} already exists");
                return UnprocessableEntity(ModelState);
            }
            // check model state validation 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // adding the new country and check saving validation
            if (!_countryRepository.CreateCountry(countryToCreate))
            {
                ModelState.AddModelError("", $"Something went wrong while updating{countryToCreate.Name}");
                return StatusCode(500,ModelState);
            }
            return StatusCode(201);
        }



        ///Update Country by id 
        ///Put Method
        ///api/Countries/{countryId}
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        [HttpPut("{countryId}")]
        public IActionResult UpdateCountry([FromBody] Country countryToUpdate , int countryId)
        {
            // check if request is null
            if (countryToUpdate == null)
                return BadRequest(ModelState);

            // validate request body Id is equal to request url param (Id)
            if (countryId != countryToUpdate.Id)
                return BadRequest(ModelState);

            // check if a country already exist in my DBset
            if (!_countryRepository.CountryExists(countryId))
                return NotFound("This Country doesn't exist to be updated");

            // check if new updated country is duplicated 
            if (_countryRepository.IsDuplicateCountryName(countryId, countryToUpdate.Name))
                return UnprocessableEntity($"Country {countryToUpdate.Name} already exists");

            // check if Somthing wents wrong while updating in DBset (Internal Server Error) 
            if (!_countryRepository.UpdateCountry(countryToUpdate))
                return StatusCode(500, $"Something went wrong while deleting{ countryToUpdate.Name}");

             return NoContent();
             

        }





        ///Delete Country by id 
        ///Delete Method
        ///api/Countries/{countryId}
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        [HttpDelete("{countryId}")]
        public IActionResult DeleteCountry(int countryId)
        {
            // check if a country already exist in my DBset
            if (!_countryRepository.CountryExists(countryId))
                return NotFound("This Country isn't exist to be delete");

            var CountryToDelete = _countryRepository.GetCountry(countryId);

            if (_countryRepository.GetAuthorsOfACountry(countryId).Count > 0)
                return Conflict($"Country {CountryToDelete.Name}"+ " cannot be deleted because it used by at least one author");


            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            if (!_countryRepository.DeleteCountry(CountryToDelete))
                return StatusCode(500, $"Something went wrong while deleting Country {CountryToDelete.Name}");

            return NoContent();
        }

    }
}
