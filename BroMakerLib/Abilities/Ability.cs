using BroMakerLib.Infos;
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

        public T owner = null;
        public AbilityInfo info = null;

        public virtual void AssignOwner(T owner)
        {
            this.owner = owner;
        }

        public virtual void Init(AbilityInfo info)
        {
            this.info = info;
            info.ReadParameters(this);
            info.BeforeAwake(this);
            info.AfterAwake(this);
            info.BeforeStart(this);
            info.AfterStart(this);
        }

        public virtual void All()
        { }


        #region Unity Methods
        protected virtual void Awake()
        { }
        protected virtual void Start()
        { }
        protected virtual void Update()
        { }
        #endregion
    }
}
