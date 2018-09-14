﻿using System;
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
        public string Colour { get; set; }
        public string ChassisNumber { get; set; }
        public string CapCode { get; set; }
        public string EngineSize { get; set; }
        public string Spec { get; set; }
    }
}