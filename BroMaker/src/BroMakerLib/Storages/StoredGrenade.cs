using BroMakerLib.CustomObjects.Bros;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.Storages
{
#pragma warning disable 659
    public struct StoredGrenade : IStoredObject
    {
        public string name { get; set; }
        public string path { get; set; }

        public StoredGrenade(string path)
        {
            this.path = path;
            name = Path.GetFileNameWithoutExtension(this.path);
        }

        [Obsolete("Unimplemented")]
        public void Load(CustomHero bro)
        {
            // TODO
        }

        public override bool Equals(object obj)
        {
            return obj is StoredGrenade && ((StoredGrenade)obj).name == this.name;
        }

        public override string ToString()
        {
            return name;
        }


    }
}
