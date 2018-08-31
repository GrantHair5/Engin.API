namespace Engin.API.Models
{
    public class Predictions
    {
        public Predictions(string tagId, string tag, string probability)
        {
            TagId = tagId;
            Tag = tag;
            Probability = probability;
        }

        public string TagId { get; set; }
        public string Tag { get; set; }
        public string Probability { get; set; }
    }
}
