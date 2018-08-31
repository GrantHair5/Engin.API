using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engin.API.Models
{
    public class HpiResults
    {
        public Vehicle Vehicle { get; set; }
    }

    public class Vehicle
    {
        public string Manufacturer { get; set; }
        public string Model { get; set; }
    }
}