namespace Engin.API.Models
{
    public class Predictions
    {
        public Predictions(string tag, string probability)
        {
            Tag = tag;
            Probability = probability;
        }

        public string Tag { get; set; }
        public string Probability { get; set; }
    }
}