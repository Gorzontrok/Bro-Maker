using System.Collections.Generic;
using UnityEngine;

namespace BroMakerLib.CustomObjects.Projectiles
{
    public abstract class CustomPockettedSpecial
    {
        public static List<List<CustomPockettedSpecial>> pockettedSpecials = new List<List<CustomPockettedSpecial>> { new List<CustomPockettedSpecial>(), new List<CustomPockettedSpecial>(), new List<CustomPockettedSpecial>(), new List<CustomPockettedSpecial>() };

        /// <summary>
        /// Gives a custom pocketted special to a bro
        /// </summary>
        /// <param name="character">Bro to receive the special</param>
        /// <param name="special">Instance of your custom pocketted special</param>
        public static void AddPockettedSpecial( TestVanDammeAnim character, CustomPockettedSpecial special )
        {
            if ( character is BroBase bro )
            {
                pockettedSpecials[bro.playerNum].Add( special );
                bro.pockettedSpecialAmmo.Add( PockettedSpecialAmmoType.None );
                special.SetSpecialMaterials( bro );
                bro.player.hud.SetGrenades( 1 );
            }
            
        }

        /// <summary>
        /// Clears all custom pocketted specials for a specific player
        /// </summary>
        /// <param name="playerNum"></param>
        public static void ClearPockettedSpecials( int playerNum )
        {
            pockettedSpecials[playerNum].Clear();
        }

        /// <summary>
        /// Override this to perform whatever action your pocketted special does, whether that be spawning a projectile or something else.
        /// </summary>
        /// <param name="bro">The bro who's using the pocketted special</param>
        public abstract void UseSpecial( BroBase bro );

        /// <summary>
        /// Override this to set the special material to the material of your custom pocketted special.
        /// I recommend using the SetSpecialMaterials method from BroMakerUtilities
        /// </summary>
        /// <param name="bro">The bro who has the pocketted special</param>
        public abstract void SetSpecialMaterials( BroBase bro );

        /// <summary>
        /// Override this to set whether the bro will have their special ammo reset to full after using this pocketted special
        /// </summary>
        /// <returns>True to have the ammo refreshed</returns>
        public virtual bool RefreshAmmo()
        {
            return true;
        }

        /// <summary>
        /// Override this to set what animation the bro will use while activating your pocketted special.
        /// </summary>
        /// <returns>If true, it will be the throwing animation, if false, it will be the flex animation</returns>
        public virtual bool UseThrowingAnimation()
        {
            return true;
        }
    }
}
