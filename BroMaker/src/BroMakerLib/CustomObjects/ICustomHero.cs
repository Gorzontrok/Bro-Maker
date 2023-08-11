using BroMakerLib.Infos;

namespace BroMakerLib.CustomObjects
{
    public interface ICustomHero
    {
        [Syncronize]
        CustomBroInfo info { get; set; }
        [Syncronize]
        BroBase character { get; set; }
    }
}
