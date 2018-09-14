using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engin.API.Models
{
    public class HpiResults
    {
        public Vehicle Model { get; set; }
    }

    public class Vehicle
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public string RegNumber { get; set; }
    }
}