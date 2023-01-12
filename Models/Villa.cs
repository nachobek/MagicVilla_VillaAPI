using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicVilla_VillaAPI.Models
{
    public class Villa
    {
        [Key] // A property called Id will be a key by default when using Entity Framework. However we can specify Key with data annotation too to specify key fields.
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {get; set;}

        [Required]
        public string Name {get; set;}

        public string Details {get; set;}

        public double Rate {get; set;}

        public int Sqft {get; set;}

        public int Occupancy { get; set; }

        public string ImageUrl { get; set; }

        public string Amenity { get; set; }

        public DateTime CreatedDate {get; set;}

        public DateTime UpdatedDate {get; set;}
    }
}