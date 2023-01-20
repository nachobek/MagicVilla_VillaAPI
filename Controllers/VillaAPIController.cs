using System.Linq;
using System.Net;
using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers
{
    // [Route("api/[controller]]")] // [controller] will get/use the controller name (VillaAPI) automatically. If the class name ever changes, the route will change too, so anyone calling this endpoint will get an error unless their update their link.
    [Route("api/VillaAPI/Villas")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private readonly ILogger<VillaAPIController> _logger;
        private readonly IVillaRepository _villaRepository;
        private readonly IMapper _mapper;
        protected APIResponse _response;

        public VillaAPIController(ILogger<VillaAPIController> logger, IVillaRepository villaRepository, IMapper mapper)
        {
            _logger = logger;
            _villaRepository = villaRepository;
            _mapper = mapper;
            _response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillas()
        {
            try
            {
                IEnumerable<Villa> villaList = await _villaRepository.GetAllAsync();

                _logger.LogInformation("Getting all Villas");

                _response.Result = _mapper.Map<IEnumerable<VillaReadDTO>>(villaList);
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                
                return _response;
            }
        }

        [HttpGet("{id:int}", Name = "GetVilla")] // This Name attribute is used in the CreatedAtRoute method down below. Without it, the said method call won't work.
        // [Route("Villa/{id:int}")] // Using Route attribute we can also manipulate the route.
        // [ProducesResponseType(200, type = typeof(VillaDTO))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    string errorMessage = $"Villa ID: {id} is not valid";

                    _logger.LogError(errorMessage);

                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { errorMessage };

                    return BadRequest(_response);
                }

                var villa = await _villaRepository.GetOneAsync( v => v.Id == id, false);

                if (villa == null)
                {
                    var errorMessage = $"Villa ID: {id} not found";

                    _logger.LogError(errorMessage);

                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { errorMessage };

                    return NotFound(_response);
                }
                else
                {
                    _response.Result = _mapper.Map<VillaReadDTO>(villa);
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;

                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                
                return _response;
            }
        }

        [HttpPost]
        // [ProducesResponseType(StatusCodes.Status200OK)] // When successfull, this endpoint will not return 200 but 201, since it creates/adds a new element.
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "admin")] // With this, only an authorized user with "admin" role can create a villa.
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody]VillaCreateDTO villaCreateDTO)
        {
            try
            {
                // if (!ModelState.IsValid) // If we don't use the property [ApiController], then we need to manually check that the data received follows the DataAnnotation properties in our model VillaDTO.cs.
                // {
                //     return BadRequest(ModelState);
                // }

                if (await _villaRepository.GetOneAsync(v => v.Name.ToLower() == villaCreateDTO.Name.ToLower(), false) != null) // If it's not null it means it found a villa where the name matches the name being passed.
                {
                    var errorMessage = "Villa already exists.";

                    ModelState.AddModelError("ErrorMessages", errorMessage); // CustomError is the key of the error message we are adding to ModelState.

                    _response.Result = ModelState;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { errorMessage };

                    return BadRequest(_response);
                }

                if (villaCreateDTO == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "Villa is null" };

                    return BadRequest(_response);
                }

                Villa villaFromVillaCreateDTO = _mapper.Map<Villa>(villaCreateDTO);

                villaFromVillaCreateDTO.CreatedDate = DateTime.Now;

                await _villaRepository.CreateAsync(villaFromVillaCreateDTO);

                _response.Result = _mapper.Map<VillaReadDTO>(villaFromVillaCreateDTO);
                _response.StatusCode = HttpStatusCode.Created;
                _response.IsSuccess = true;

                // A common practice is to return the path/URI of the object that was just created rather than just a 200 OK.
                // return Ok(villaDTO);

                // To do so, we can use method CreatedAtRoute as follows.
                // The GetVilla name is not the name of the method to be called but the name assigned as a property.
                // The second argument must be an object, so we create a new object with the Id of the Villa that was just created.
                // Lastly we pass the Villa object just added.
                return CreatedAtRoute("GetVilla", new { id = villaFromVillaCreateDTO.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                
                return _response;
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "admin")] // With this, only an authorized user with "admin" role can delete a villa.
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id) // Typically use the interface IActionResult when there is no need to return any data.
        {
            try
            {
                if (id == 0)
                {
                    var errorMessage = $"Villa ID: {id} is not valid";

                    _logger.LogError(errorMessage);

                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { errorMessage };

                    return BadRequest(_response);
                }

                var villa = await _villaRepository.GetOneAsync( v => v.Id == id, true);

                if (villa == null)
                {
                    var errorMessage = $"Villa ID: {id} not found";

                    _logger.LogError(errorMessage);

                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { errorMessage };

                    return NotFound(_response);
                }

                await _villaRepository.RemoveAsync(villa);

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;

                // return NoContent(); // Since we are always returning "_response" now, we can't use NoContent as the response type.
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                
                return _response;
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody]VillaUpdateDTO villaUpdateDTO)
        {
            try
            {
                // Technically you wouldn't need to receive the id as a standlone field and do this kind of validation.
                // We could just work with the id found in the villa object being passed in the body to be updated.
                if (villaUpdateDTO == null || id != villaUpdateDTO.Id)
                {
                    var errorMessage = $"Villa is null or Id: {id} does not match the Villa Id";

                    _logger.LogError(errorMessage);

                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { errorMessage };

                    return BadRequest(_response);
                }

                var villa = await _villaRepository.GetOneAsync( v => v.Id == id, false);

                if (villa == null)
                {
                    var errorMessage = $"Villa ID: {id} not found";

                    _logger.LogError(errorMessage);

                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { errorMessage };

                    return NotFound(_response);
                }

                Villa villaFromVillaUpdateDTO = _mapper.Map<Villa>(villaUpdateDTO);

                villaFromVillaUpdateDTO.CreatedDate = villa.CreatedDate;
                villaFromVillaUpdateDTO.UpdatedDate = DateTime.Now;

                await _villaRepository.UpdateAsync(villaFromVillaUpdateDTO);

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;

                // return NoContent(); // Since we are always returning "_response" now, we can't use NoContent as the response type.
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                
                return _response;
            }
        }

        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, JsonPatchDocument<VillaUpdateDTO> villaDTOPatch)
        {
            try
            {
                if (villaDTOPatch == null || id == 0)
                {
                    var errorMessage = $"Villa is null or Id: {id} is invalid.";

                    _logger.LogError(errorMessage);

                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { errorMessage };

                    return BadRequest(_response);
                }

                var villa = await _villaRepository.GetOneAsync( v => v.Id == id, false);

                if ( villa == null )
                {
                    var errorMessage = $"Villa ID: {id} not found";

                    _logger.LogError(errorMessage);

                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { errorMessage };

                    return NotFound(_response);
                }
                else
                {
                    VillaUpdateDTO villaDTOFromVilla = _mapper.Map<VillaUpdateDTO>(villa); // Convert the Ville found by the given Id into VillaUpdateDTO object, so the ApplyTo patch command can be applied.

                    villaDTOPatch.ApplyTo(villaDTOFromVilla, ModelState); // Using the JsonPatch object functionality to update the specific property passed for that particular villa object.
                    
                    Villa villaFromVillaDTO = _mapper.Map<Villa>(villaDTOFromVilla); // Convert the modified object back to Villa type so the changes can be persisted in the DB.

                    villaFromVillaDTO.UpdatedDate = DateTime.Now;
                    villaFromVillaDTO.CreatedDate = villa.CreatedDate;

                    await _villaRepository.UpdateAsync(villaFromVillaDTO);

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }
                    else
                    {
                        _response.StatusCode = HttpStatusCode.NoContent;
                        _response.IsSuccess = true;

                        return Ok(_response);
                    }
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                
                return _response;
            }
        }
    }
}