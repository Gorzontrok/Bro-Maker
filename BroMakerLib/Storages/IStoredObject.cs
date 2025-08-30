namespace BroMakerLib.Storages
{
    public interface IStoredObject
    {
        string path { get; set; }
        string name { get; set; }
    }
}
