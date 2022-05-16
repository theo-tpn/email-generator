namespace Noctus.Domain.Entities
{
    public class GenBucket : BaseEntity
    {
        public string Ref { get; set; }
        public int CurrentStock { get; set; }
    }
}
