namespace eCommerceUoWBasic.Models
{
    public class IdempotencyRecord
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string RequestHash { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string ResponseBody { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
    }
}
