using BroMakerLib.Infos;
using Newtonsoft.Json;
using RocketLib.JsonConverters;
using UnityEngine;

namespace BroMakerLib.Abilities
{
    public class Ability<T>
        where T : class
    {
        public float DT
        {
            get
            {
                return Time.deltaTime;
            }
        }

        /// <summary>
        /// Offset position from <see cref="owner"/>
        /// </summary>
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 PositionOffset { get; set; }

        [JsonIgnore]
        public T owner = null;
        [JsonIgnore]
        public AbilityInfo info = null;

        public virtual void AssignOwner(T owner)
        {
            this.owner = owner;
        }

        public virtual void Initialize(AbilityInfo info)
        {
            this.info = info;
            info.ReadParameters(this);
        }

        public virtual void All(string calledFromMethod, params object[] objects)
        { }


        #region Unity Methods
        protected virtual void Awake()
        { }
        protected virtual void Start()
        { }
        protected virtual void Update()
        { }
        #endregion

        protected bool IsUnityMethod(string methodName)
        {
            return methodName == "Update" || methodName == "Start" || methodName == "Awake";
        }
    }
}
