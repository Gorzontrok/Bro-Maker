using BroMakerLib.Infos;

namespace BroMakerLib.CustomObjects
{
    public interface ICustomHero
    {
        CustomBroInfo info { get; set; }
        BroBase character { get; set; }
    }
}
