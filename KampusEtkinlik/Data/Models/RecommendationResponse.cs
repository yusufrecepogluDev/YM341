namespace KampusEtkinlik.Data.Models
{
    public class RecommendationResponse
    {
        public List<int> RecommendedActivityIds { get; set; } = new List<int>();
    }

    // n8n'in döndürdüğü wrapper format
    public class N8nRecommendationResponse
    {
        public RecommendationResponse? Output { get; set; }
    }
}
