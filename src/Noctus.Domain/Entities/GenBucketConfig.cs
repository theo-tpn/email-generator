namespace Noctus.Domain.Entities
{
    public class GenBucketConfig : BaseEntity
    {
        public string Ref { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
    }
}
