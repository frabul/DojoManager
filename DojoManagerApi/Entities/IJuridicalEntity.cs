namespace DojoManagerApi.Entities
{
    public interface IJuridicalEntity
    {
        Address Address { get; set; }
        string EMail { get; set; }
        int Id { get; set; }
        string Name { get; set; }
        string PhoneNumber { get; set; }
    }
}