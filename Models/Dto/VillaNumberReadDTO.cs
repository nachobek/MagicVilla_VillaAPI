using System.ComponentModel.DataAnnotations;

namespace MagicVilla_VillaAPI.Models
{
    class VillaNumberReadDTO
    {
        [Required]
        public int VillaNo { get; set; }

        public string? SpecialDetails { get; set; }
    }
}