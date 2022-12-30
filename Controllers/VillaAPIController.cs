using System.Linq;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public VillaAPIController(ILogger<VillaAPIController> logger, ApplicationDbContext db, IMapper mapper)
        {
            _logger = logger;
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaReadDTO>>> GetVillas()
        {
            IEnumerable<Villa> villaList = await _db.Villas.AsNoTracking().ToListAsync();

            _logger.LogInformation("Getting all Villas");

            return Ok(_mapper.Map<IEnumerable<VillaReadDTO>>(villaList)); // Doing List<VillaReadDTO> as target also works.
        }

        [HttpGet("{id:int}", Name = "GetVilla")] // This Name attribute is used in the CreatedAtRoute method down below. Without it, the said method call won't work.
        // [ProducesResponseType(200, type = typeof(VillaDTO))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VillaReadDTO>> GetVilla(int id)
        {
            if (id == 0)
            {
                _logger.LogError($"Get Villa by ID error. Villa ID: {id}");
                return BadRequest($"Villa ID: {id} not valid");
            }

            var villa = await _db.Villas.FirstOrDefaultAsync( v => v.Id == id);

            if (villa == null)
            {
                return NotFound($"Villa ID: {id} not found");
            }
            else
            {
                return Ok(_mapper.Map<VillaReadDTO>(villa));
            }
        }

        [HttpPost]
        // [ProducesResponseType(StatusCodes.Status200OK)] // When successfull, this endpoint will not return 200 but 201, since it creates/adds a new element.
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VillaReadDTO>> CreateVilla([FromBody]VillaCreateDTO villaCreateDTO)
        {
            // if (!ModelState.IsValid) // If we don't use the property [ApiController], then we need to manually check that the data received follows the DataAnnotation properties in our model VillaDTO.cs.
            // {
            //     return BadRequest(ModelState);
            // }

            if (await _db.Villas.FirstOrDefaultAsync(v => v.Name.ToLower() == villaCreateDTO.Name.ToLower()) != null) // If it's not null it means it found a villa where the name matches the name being passed.
            {
                ModelState.AddModelError("CustomError", "Villa already exists."); // CustomError is the key of the error message we are adding to ModelState.
                return BadRequest(ModelState);
            }

            if (villaCreateDTO == null)
            {
                return BadRequest(villaCreateDTO);
            }

            Villa villaFromVillaCreateDTO = _mapper.Map<Villa>(villaCreateDTO);

            villaFromVillaCreateDTO.CreatedDate = DateTime.Now;

            await _db.Villas.AddAsync(villaFromVillaCreateDTO);
            await _db.SaveChangesAsync();

            // A common practice is to return the path/URI of the object that was just created rather than just a 200 OK.
            // return Ok(villaDTO);

            // To do so, we can use method CreatedAtRoute as follows.
            // The GetVilla name is no the name of the method to be called but the name assigned as a property.
            // The second argument must be an object, so we create a new object with the Id of the Villa that was just created.
            // Lastly we pass the Villa object just added.
            return CreatedAtRoute("GetVilla", new { id = villaFromVillaCreateDTO.Id }, villaFromVillaCreateDTO);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteVilla(int id) // Typically use the interface IActionResult when there is no need to return any data.
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var villa = await _db.Villas.FirstOrDefaultAsync( v => v.Id == id);

            if (villa == null)
            {
                return NotFound();
            }

            _db.Villas.Remove(villa);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody]VillaUpdateDTO villaUpdateDTO)
        {
            // Technically you wouldn't need to receive the id as a standlone field and do this kind of validation.
            // We could just work with the id found in the villa object being passed in the body to be updated.
            if (villaUpdateDTO == null || id != villaUpdateDTO.Id)
            {
                return BadRequest();
            }

            var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync( v => v.Id == id);

            if (villa == null)
            {
                return NotFound();
            }

            Villa villaFromVillaUpdateDTO = _mapper.Map<Villa>(villaUpdateDTO);

            villaFromVillaUpdateDTO.CreatedDate = villa.CreatedDate;
            villaFromVillaUpdateDTO.UpdatedDate = DateTime.Now;

            _db.Villas.Update(villaFromVillaUpdateDTO);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVilla(int id, JsonPatchDocument<VillaUpdateDTO> villaDTOPatch)
        {
            if (villaDTOPatch == null || id == 0)
            {
                return BadRequest();
            }

            var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync( v => v.Id == id);

            if ( villa == null )
            {
                return NotFound();
            }
            else
            {
                VillaUpdateDTO villaDTOFromVilla = _mapper.Map<VillaUpdateDTO>(villa); // Convert the Ville found by the given Id into VillaUpdateDTO object, so the ApplyTo patch command can be applied.

                villaDTOPatch.ApplyTo(villaDTOFromVilla, ModelState); // Using the JsonPatch object functionality to update the specific property passed for that particular villa object.
                
                Villa villaFromVillaDTO = _mapper.Map<Villa>(villaDTOFromVilla); // Convert the modified object back to Villa type so the changes can be persisted in the DB.

                villaFromVillaDTO.UpdatedDate = DateTime.Now;
                villaFromVillaDTO.CreatedDate = villa.CreatedDate;

                _db.Villas.Update(villaFromVillaDTO);
                await _db.SaveChangesAsync();

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