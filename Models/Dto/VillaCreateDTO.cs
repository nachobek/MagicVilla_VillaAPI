using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Models.Dto
{
    // This VillaDTO class is being used in place of the actual model class Villa.
    // This is to hide some properties that we don't want to return to the end users.

    public class VillaCreateDTO
    {
        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        public string Details {get; set;}

        public int Sqft { get; set; }

        [Required]
        public double Rate {get; set;}

        public int Occupancy { get; set; }

        public string ImageUrl { get; set; }

        public string Amenity { get; set; }
    }
}