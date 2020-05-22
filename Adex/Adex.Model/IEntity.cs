namespace Adex.Model
{
    public interface IEntity
    {
        int Id { get; set; }

        string ExternalId { get; set; }

        string Designation { get; set; }
    }
}
