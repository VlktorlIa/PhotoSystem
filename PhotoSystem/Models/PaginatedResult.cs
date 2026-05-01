namespace PhotoSystem.Models
{
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public bool HasMore { get; set; }
    }
}