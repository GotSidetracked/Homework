namespace Homework.DBEntities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedUTC { get; set; }
    }
}
