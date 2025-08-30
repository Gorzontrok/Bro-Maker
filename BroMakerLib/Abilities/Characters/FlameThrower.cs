namespace BroMakerLib.Abilities.Characters
{

    public class FlameThrower : CharacterAbility
    {
        public Projectile projectile2 = null;
        public Projectile projectile3 = null;
        public override void UseFire()
        {
        }

        public override void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            int num = UnityEngine.Random.Range(0, 3);
            if (num == 0)
            {
                ProjectileController.SpawnProjectileLocally(owner.projectile, owner, x, y, xSpeed, ySpeed, PlayerNum);
            }
            else if (num == 1)
            {
                ProjectileController.SpawnProjectileLocally(projectile2, owner, x, y, xSpeed, ySpeed, PlayerNum);
            }
            else if (num == 2)
            {
                ProjectileController.SpawnProjectileLocally(projectile3, owner, x, y, xSpeed, ySpeed, PlayerNum);
            }

        }
    }
}
