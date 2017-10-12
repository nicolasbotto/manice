using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Manice.Web.Core.Models
{
    public class Beer
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public int IBU { get; set; }
        public double ABV { get; set; }
        public Color Color { get; set; }
    }

    public enum Color
    {
        Rubia,
        Dorada,
        Colorada,
        Negra
    }
}
