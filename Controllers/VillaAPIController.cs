using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    // [Route("api/[controller]]")] // [controller] will get/use the controller name (VillaAPI) automatically. If the class name ever changes, the route will change too, so anyone calling this endpoint will get an error unless their update their link.
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private readonly ILogger<VillaAPIController> _logger;

        public VillaAPIController(ILogger<VillaAPIController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IEnumerable<VillaDTO> GetVillas()
        {
            _logger.LogInformation("Getting all Villas");
            return VillaStore.villaList;
        }

        [HttpGet("{id:int}", Name = "GetVilla")] // This Name attribute is used in the CreatedAtRoute method. Without it, the said method call won't work.
        // [ProducesResponseType(200, type = typeof(VillaDTO))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VillaDTO> GetVilla(int id)
        {
            if (id == 0)
            {
                _logger.LogError($"Get Villa by ID error. Villa ID: {id}");
                return BadRequest($"Villa ID: {id} not valid");
            }

            var villa = VillaStore.villaList.FirstOrDefault( u => u.Id == id);

            if (villa == null)
            {
                return NotFound($"Villa ID: {id} not found");
            }
            else
            {
                return Ok(villa);
            }
        }

        [HttpPost]
        // [ProducesResponseType(StatusCodes.Status200OK)] // When successfull, this endpoint will not return 200 but 201, since it creates/adds a new element.
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VillaDTO> CreateVilla([FromBody]VillaDTO villaDTO)
        {
            // if (!ModelState.IsValid) // If we don't use the property [ApiController], then we need to manually check that the data received follows the DataAnnotation properties in our model VillaDTO.cs.
            // {
            //     return BadRequest(ModelState);
            // }

            if (VillaStore.villaList.FirstOrDefault(v => v.Name.ToLower() == villaDTO.Name.ToLower()) != null) // If it's not null it means it found a villa where the name matches the name being passed.
            {
                ModelState.AddModelError("CustomError", "Villa already exists."); // CustomError is the key of the error message we are adding to ModelState.
                return BadRequest(ModelState);
            }

            if (villaDTO == null)
            {
                return BadRequest(villaDTO);
            }

            if (villaDTO.Id > 0) // It means the Villa received already exists, otherwise it can't have an ID already.
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            villaDTO.Id = VillaStore.villaList.OrderByDescending(v => v.Id).FirstOrDefault().Id + 1;

            VillaStore.villaList.Add(villaDTO);

            // A common practice is to return the path/URI of the object that was just created rather than just a 200 OK.
            // return Ok(villaDTO);

            // To do so, we can use method CreatedAtRoute as follows.
            // The GetVilla name is no the name of the method to be called but the name assigned as a property.
            // The second argument must be an object, so we create a new object with the Id of the Villa that was just created.
            // Lastly we pass the Villa object just added.
            return CreatedAtRoute("GetVilla", new { id = villaDTO.Id }, villaDTO);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteVilla(int id) // Typically use the interface IActionResult when there is no need to return any data.
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var villa = VillaStore.villaList.FirstOrDefault( v => v.Id == id);

            if (villa == null)
            {
                return NotFound();
            }

            VillaStore.villaList.Remove(villa);

            return NoContent();
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateVilla(int id, [FromBody]VillaDTO villaDTO)
        {
            // Technically you wouldn't need to receive the id as a standlone field and do this kind of validation.
            // We could just work with the id found in the villa object being passed to be updated.
            if (villaDTO == null || id != villaDTO.Id)
            {
                return BadRequest();
            }

            var villa = VillaStore.villaList.FirstOrDefault( v => v.Id == id);

            if ( villa == null )
            {
                return NotFound();
            }
            else
            {
                villa.Name = villaDTO.Name;
                villa.Occupancy = villaDTO.Occupancy;
                villa.Sqft = villaDTO.Sqft;

                return NoContent();
            }
        }

        [HttpPatch("{id:int}")]
        public IActionResult UpdateVilla(int id, JsonPatchDocument<VillaDTO> villaDTOPatch)
        {
            if (villaDTOPatch == null || id == 0)
            {
                return BadRequest();
            }

            var villa = VillaStore.villaList.FirstOrDefault( v => v.Id == id);

            if ( villa == null )
            {
                return NotFound();
            }
            else
            {
                villaDTOPatch.ApplyTo(villa, ModelState); // Using the JsonPatch object functionality to update the specific property passed for that particular villa object.
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    return NoContent();
                }
            }
        }
    }
}