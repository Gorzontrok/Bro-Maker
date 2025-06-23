using UnityEngine;

namespace BroMakerLib
{
    public class DamageObjectE : DamageObject
    {
        public Vector2 Force
        {
            get
            {
                return new Vector2(xForce, yForce);
            }
            set
            {
                xForce = value.x;
                yForce = value.y;
            }
        }

        public bool penetrates = false;
        public bool knock = false;
        public bool canGib = false;
        public bool ignoreDeadUnit = false;
        public bool canHeadshot = false;
        public float range;
        public Vector2 rangeV;

        public DamageObjectE(MonoBehaviour damageSender, int damage, DamageType damageType, Vector2 force, Vector2 position) : base(damage, damageType, force.x, force.y, position.x, position.y, damageSender)
        { }
    }
}
