using System;

namespace BroMakerLib
{
    public class HeroPresetAttribute : CustomObjectPresetAttribute
    {
        public HeroType baseType = HeroType.Rambro;
        public HeroPresetAttribute(string name, HeroType baseType = HeroType.Rambro) : base(name)
        {
            if (baseType == HeroType.None || baseType == HeroType.Random || baseType == HeroType.TankBroTank || baseType == HeroType.Final)
                throw new Exception($"Can't assign HeroType of type {baseType}");
            this.baseType = baseType;
        }
    }
}
