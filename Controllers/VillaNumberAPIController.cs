using System.Data;
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
    [Route("api/VillaNumberAPI/VillaNumbers")]
    [ApiController]
    public class VillaNumberAPIController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IVillaNumberRepository _villaNumberRepository;
        private readonly IVillaRepository _villaRepository;
        protected APIResponse _response;

        public VillaNumberAPIController(IVillaNumberRepository villaNumberrepository, IMapper mapper, IVillaRepository villaRepository)
        {
            _mapper = mapper;
            _villaNumberRepository = villaNumberrepository;
            _villaRepository = villaRepository;
            _response = new APIResponse();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetVillaNumbers()
        {
            try
            {
                var villaNumbersList = await _villaNumberRepository.GetAllAsync(includeProperties:"Villa");

                if (villaNumbersList == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { "No VillaNumber found." };

                    return NotFound(_response);
                }

                _response.Result = _mapper.Map<List<VillaNumberReadDTO>>(villaNumbersList);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                
                return _response;
            }
        }

        [HttpGet("{villaNo:int}", Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetVillaNumbers(int villaNo)
        {
            try
            {
                if (villaNo == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { $"VillaNo: {villaNo} not valid." };

                    return BadRequest(_response);
                }

                var villaNumber = await _villaNumberRepository.GetOneAsync(v => v.VillaNo == villaNo, false, includeProperties: "Villa");

                if (villaNumber == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { "VillaNumber not found." };

                    return NotFound(_response);
                }

                _response.Result = _mapper.Map<VillaNumberReadDTO>(villaNumber);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;

                return Ok( _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                
                return _response;
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> CreateAllVillaNumber([FromBody]VillaNumberCreateDTO villaNumberCreateDTO)
        {
            try
            {
                var villaNumber = await _villaNumberRepository.GetOneAsync(v => v.VillaNo == villaNumberCreateDTO.VillaNo, false);

                if (villaNumber != null)
                {
                    var errorMessage = "VillaNumber already exists.";

                    ModelState.AddModelError("ErrorMessages", errorMessage);

                    _response.Result = ModelState;
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { errorMessage };

                    return BadRequest(_response);
                }

                var villa = await _villaRepository.GetOneAsync(v => v.Id == villaNumberCreateDTO.VillaId, false);

                if (villa == null)
                {
                    var errorMessage = "Villa ID is invalid.";

                    ModelState.AddModelError("ErrorMessages", errorMessage);

                    _response.Result = ModelState;
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { errorMessage };

                    return BadRequest(_response);
                }

                var villaNumberFromCreatedDTO = _mapper.Map<VillaNumber>(villaNumberCreateDTO);

                villaNumberFromCreatedDTO.CreatedDate = DateTime.Now;

                await _villaNumberRepository.CreateAsync(villaNumberFromCreatedDTO);

                _response.Result = _mapper.Map<VillaNumberCreateDTO>(villaNumberFromCreatedDTO);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.Created;

                return CreatedAtRoute("GetVillaNumber", new { villaNo = villaNumberFromCreatedDTO.VillaNo }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                
                return _response;
            }
        }

        [HttpDelete("{villaNo:int}", Name = "DeleteVillaNumer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int villaNo)
        {
            try
            {
                if (villaNo == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { $"VillaNo: {villaNo} not valid." };

                    return BadRequest(_response);
                }

                var villaNumber = await _villaNumberRepository.GetOneAsync(v => v.VillaNo == villaNo, true);

                if (villaNumber == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { "VillaNumber not found." };

                    return NotFound(_response);
                }

                await _villaNumberRepository.RemoveAsync(villaNumber);

                _response.StatusCode = HttpStatusCode.NoContent;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                
                return _response;
            }
        }

        [HttpPut("{villaNo:int}", Name = "UpdateVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber([FromBody]VillaNumberUpdateDTO villaNumberUpdateDTO, int villaNo)
        {
            try
            {
                if (villaNumberUpdateDTO.VillaNo != villaNo || villaNo == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { $"VillaNo not valid or does not match body data." };

                    return BadRequest(_response);
                }

                var villa = await _villaRepository.GetOneAsync(v => v.Id == villaNumberUpdateDTO.VillaId, false);

                if (villa == null)
                {
                    var errorMessage = "Villa ID is invalid.";

                    ModelState.AddModelError("ErrorMessages", errorMessage);

                    _response.Result = ModelState;
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { errorMessage };

                    return BadRequest(_response);
                }

                var villaNumber = await _villaNumberRepository.GetOneAsync(v => v.VillaNo == villaNo);

                if (villaNumber == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { "VillaNumber not found." };

                    return NotFound(_response);
                }

                var villaNumberFromUpdateDTO = _mapper.Map<VillaNumber>(villaNumberUpdateDTO);

                villaNumberFromUpdateDTO.CreatedDate = villaNumber.CreatedDate;

                await _villaNumberRepository.UpdateAsync(villaNumberFromUpdateDTO);

                _response.Result = _mapper.Map<VillaNumberUpdateDTO>(villaNumberFromUpdateDTO);
                _response.StatusCode = HttpStatusCode.NoContent;

                return Ok(_response);
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