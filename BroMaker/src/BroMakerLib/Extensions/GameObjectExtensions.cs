using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.Extensions
{
    public static class GameObjectExtensions
    {
        [AllowedRPC]
        public static void AddComponentNoReturn(this GameObject gameObject, Type type)
        {
            gameObject.AddComponent(type);
        }
        [AllowedRPC]
        public static void AddComponentNoReturn<T>(this GameObject gameObject) where T : Component
        {
            gameObject.AddComponent<T>();
        }
    }
}
