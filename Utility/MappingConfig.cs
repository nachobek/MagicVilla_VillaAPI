using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;

namespace MagicVilla_VillaAPI.Utility
{
    class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Villa, VillaReadDTO>();
            CreateMap<VillaReadDTO, Villa>();

            CreateMap<Villa, VillaCreateDTO>().ReverseMap();

            CreateMap<Villa, VillaUpdateDTO>().ReverseMap();

            CreateMap<VillaNumber, VillaNumberReadDTO>().ReverseMap();

            CreateMap<VillaNumber, VillaNumberCreateDTO>().ReverseMap();

            CreateMap<VillaNumber, VillaNumberUpdateDTO>().ReverseMap();
        }
    }
}