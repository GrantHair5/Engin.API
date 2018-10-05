using System.Collections.Generic;

namespace Engin.API.Models
{
    public class Rootobject
    {
        public Tag[] tags { get; set; }
        public string requestId { get; set; }
    }

    public class Tag
    {
        public string name { get; set; }
        public float confidence { get; set; }
    }
}