namespace BroMakerLib.Infos
{
    public class CustomCharacterInfo : CustomBroforceObjectInfo
    {
        protected new string _defaultName = "CHARACTER";
        public CustomCharacterInfo() : base() { }
        public CustomCharacterInfo(string name) : base(name) { }

        public string characterPreset = "CustomHero";
    }
}
