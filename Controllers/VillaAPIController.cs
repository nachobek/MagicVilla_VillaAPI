using System.Linq;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers
{
    // [Route("api/[controller]]")] // [controller] will get/use the controller name (VillaAPI) automatically. If the class name ever changes, the route will change too, so anyone calling this endpoint will get an error unless their update their link.
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private readonly ILogger<VillaAPIController> _logger;
        private readonly ApplicationDbContext _db;

        public VillaAPIController(ILogger<VillaAPIController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {
            _logger.LogInformation("Getting all Villas");
            return Ok(_db.Villas.AsNoTracking().ToList());
            // return Ok((IEnumerable<VillaDTO>)_db.Villas.AsNoTracking().ToList()); // Return Ok(Villas) is not possible since the method was defined to return a VillaDTO type, so cast is added to transform Villa to VillaDTO.
        }

        [HttpGet("{id:int}", Name = "GetVilla")] // This Name attribute is used in the CreatedAtRoute method down below. Without it, the said method call won't work.
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

            // var villa = _db.Villas.FirstOrDefault( u => u.Id == id);

            var villa = _db.Villas.FirstOrDefault( v => v.Id == id);

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

            if (_db.Villas.FirstOrDefault(v => v.Name.ToLower() == villaDTO.Name.ToLower()) != null) // If it's not null it means it found a villa where the name matches the name being passed.
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

            Villa villaFromVillaDTO = new()
            {
                Amenity = villaDTO.Amenity,
                Details = villaDTO.Details,
                Id = villaDTO.Id,
                ImageUrl = villaDTO.ImageUrl,
                Name = villaDTO.Name,
                Occupancy = villaDTO.Occupancy,
                Rate = villaDTO.Rate,
                Sqft = villaDTO.Sqft
            };

            _db.Villas.Add(villaFromVillaDTO);
            _db.SaveChanges();

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

            var villa = _db.Villas.FirstOrDefault( v => v.Id == id);

            if (villa == null)
            {
                return NotFound();
            }

            _db.Villas.Remove(villa);
            _db.SaveChanges();

            return NoContent();
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateVilla(int id, [FromBody]VillaDTO villaDTO)
        {
            // Technically you wouldn't need to receive the id as a standlone field and do this kind of validation.
            // We could just work with the id found in the villa object being passed to be updated.
            if (villaDTO == null || id != villaDTO.Id)
            {
                return BadRequest();
            }

            Villa villaFromVillaDTO = new()
            {
                Amenity = villaDTO.Amenity,
                Details = villaDTO.Details,
                Id = villaDTO.Id,
                ImageUrl = villaDTO.ImageUrl,
                Name = villaDTO.Name,
                Occupancy = villaDTO.Occupancy,
                Rate = villaDTO.Rate,
                Sqft = villaDTO.Sqft
            };

            var createdDateTemp = _db.Villas.AsNoTracking().FirstOrDefault( v => v.Id == id).CreatedDate;

            _db.Villas.Update(villaFromVillaDTO);
            _db.Villas.FirstOrDefault( v => v.Id == id).UpdatedDate = DateTime.Now;
            _db.Villas.FirstOrDefault( v => v.Id == id).CreatedDate = createdDateTemp;
            _db.SaveChanges();

            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public IActionResult UpdateVilla(int id, JsonPatchDocument<VillaDTO> villaDTOPatch)
        {
            if (villaDTOPatch == null || id == 0)
            {
                return BadRequest();
            }

            var villa = _db.Villas.AsNoTracking().FirstOrDefault( v => v.Id == id);

            if ( villa == null )
            {
                return NotFound();
            }
            else
            {
                VillaDTO villaDTOFromVilla = new()
                {
                    Amenity = villa.Amenity,
                    Details = villa.Details,
                    Id = villa.Id,
                    ImageUrl = villa.ImageUrl,
                    Name = villa.Name,
                    Occupancy = villa.Occupancy,
                    Rate = villa.Rate,
                    Sqft = villa.Sqft
                };

                villaDTOPatch.ApplyTo(villaDTOFromVilla, ModelState); // Using the JsonPatch object functionality to update the specific property passed for that particular villa object.
                
                Villa villaFromVillaDTO = new()
                {
                    Amenity = villaDTOFromVilla.Amenity,
                    Details = villaDTOFromVilla.Details,
                    Id = villaDTOFromVilla.Id,
                    ImageUrl = villaDTOFromVilla.ImageUrl,
                    Name = villaDTOFromVilla.Name,
                    Occupancy = villaDTOFromVilla.Occupancy,
                    Rate = villaDTOFromVilla.Rate,
                    Sqft = villaDTOFromVilla.Sqft
                };

                _db.Villas.Update(villaFromVillaDTO);
                _db.SaveChanges();

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