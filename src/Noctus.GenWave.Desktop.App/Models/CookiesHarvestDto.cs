using System.ComponentModel.DataAnnotations;

namespace Noctus.GenWave.Desktop.App.Models
{
    public class CookiesHarvestDto
    {
        [Required(ErrorMessage = "You must specify cookies number to harvest")]
        public int CookiesNumber { get; set; }
        [Required(ErrorMessage = "You must specify degree of parallelism")]
        public int Parallelism { get; set; }
        [Required(ErrorMessage = "Headless or nah ?")]
        public bool Headless { get; set; }
        [Required(ErrorMessage = "Viewport is required")]
        public string Viewport { get; set; }
    }
}
