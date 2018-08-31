using System.Collections.Generic;

namespace Engin.API.Models
{
    public class AlprResults
    {
        public List<AlprResult> Results { get; set; }
    }

    public class AlprResult
    {
        public string Plate { get; set; }
        public double Confidence { get; set; }
    }
}