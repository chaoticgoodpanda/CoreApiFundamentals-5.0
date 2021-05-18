using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using AutoMapper;




namespace CoreCodeCamp.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;
        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        //use case where you return an array containing ALL camps
        //includeTalks = false important because parameters are mapped to query strings that may or may not exist
        //query strings are different from URI bc we want to take the query string and add control for our users
        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false) 
        {
            try 
            {
                //since Async() winds up being a task of camps
                var results = await _repository.GetAllCampsAsync(includeTalks);

                return _mapper.Map<CampModel[]>(results);
            }


            catch (Exception) 
            {
                //returns 500 error if GET request doesn't work
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        } 

        //use case where you return an individual camp (not an array)
        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await _repository.GetCampAsync(moniker);

                if (result == null) return NotFound();

                return _mapper.Map<CampModel>(result);
            }
            catch (System.Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsByEventDate(theDate, includeTalks);
                
                if (!results.Any()) return NotFound();
                    
                return _mapper.Map<CampModel[]>(results);
                
            }
            catch (System.Exception)
            {
                
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var campExisting = await _repository.GetCampAsync(model.Moniker);

                //check to see if camp inputted in JSON POST is already in the database, and if so throw an error
                if(campExisting != null)
                {
                    return BadRequest("moniker already in use.");
                }

                //creating a link (pathway) to the new data created through model binding by using the POST method
                //to the Controller and Get method to reference the newly created data

                var location = _linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker});

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker.");
                }

                //Create a new Camp
                var camp =  _mapper.Map<Camp>(model);
                _repository.Add(camp);
                if (await _repository.SaveChangesAsync())
                {
                    return Created($"/api/camps/{camp.Moniker}", _mapper.Map<CampModel>(camp));
                }
                
            }
            catch (System.Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }

            return BadRequest();
        }

        //note that we are updating a specific item, not accessing the whole collection
        //so need to have the "string moniker" as a modifier.
        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model)
        {
            try
            {
                //lowercase 'moniker' here because we want to compare the moniker that was actually put into the JSON call.
                var oldCamp = await _repository.GetCampAsync(moniker);

                if (oldCamp == null) return NotFound($"no existing camp with moniker of {moniker}.");

                //putting existing model from database and applying the changes
                _mapper.Map(model, oldCamp);

                if (await _repository.SaveChangesAsync())
                {
                    //should return the existing moniker updated with the new put data.
                    return _mapper.Map<CampModel>(oldCamp);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        //use of IActionResult instead of ActioNResult because there is no body returned, just an action
        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = await _repository.GetCampAsync(moniker);
                if (oldCamp == null) return NotFound();

                _repository.Delete(oldCamp);

                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (System.Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

    }
}