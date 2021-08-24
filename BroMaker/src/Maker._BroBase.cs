using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using System.IO;
using BroMakerLoadMod;
namespace BroMaker
{
    public partial class Maker
    {
        public class _BroBase : BroBase
        {
            /// <summary>
            /// THis function is for setup the bro.
            /// </summary>
            /// <param name="attachSprite">The sprite of the bro.</param>
            /// <param name="attachPlayer"></param>
            /// <param name="attachplayerNum"></param>
            /// <param name="attachSoundHolder"></param>
            public virtual void SetupBro(SpriteSM attachSprite, Player attachPlayer, int attachplayerNum, SoundHolder attachSoundHolder)
            {
                /* In the override function.
                 * 
                 * sprite = attachSprite;
                 * player = attachPlayer;
                 * playerNum = attachplayerNum;
                 * soundHolder = attachSoundHolder;
                 * this.SpecialAmmo = 5;
                 * this.originalSpecialAmmo = 5;
                 * this.health = 1; // The number of hit the bro can take before dying.
                 */

            }

            /// <summary>
            /// This function is called when the bro is alive.
            /// </summary>
            protected override void Awake()
            {

                // The foolowing variable is some variable that you can use. Read the description of each for more information. They are basing on Rambro.

                // If you encounter glitch, try some of these variable.

                /* ||||The following variable are in comment because this is their default value.|||||
                 * -----------------------------------------------------------------------------------
                 * // Some action of the bro.
                 * this.canGib = true;          // Gib when explode. Already enabled.
                 * this.canWallClimb = true;    // All in the name, already enabled. If false, still climbing if you press jump and the animation of climbing is messed up.
                 * this.canPushBlocks = true;   // All in the name, already enabled.
                 * this.canDash = true;         // All in the name, already enabled.
                 * this.breakDoorsOpen = false; // Break door when tey are open.
                 *
                 * // Bro "information".
                 * this.JUMP_TIME = 0.123f;     // The time you can hold jump for jumping. Be carefull adding a small number can really increase time.
                 * this.speed = 110.0f;         // Speed of the bro.
                 * this.maxFallSpeed = -400f;   // The speed for falling when he is in the air.
                 * this.fireRate = 0.1f;        // The time between each bullet.
                 * this.fireDelay = 0.0f;       // the delay between press fire and the bro fire. Example on Brominator.
                 * this._jumpForce = 260f;      // Jump height.
                 * this.health = 1; // Helath of the bro. If the value is 0 or no value given, the value will be 3.
                 * 
                 * //Improve animation.
                 * this.useNewPushingFrames = false;            // Custom animation when pushing something. The gun stay reverse after pushing the block.
                 * this.useNewLadderClimbingFrames = false;     // Custom climbing frame that isn't use for bro. Some sprite are missing.
                 * this.useLadderClimbingTransition = false;    // Do nothing, still missing Enter and Exit frame on the ladder.
                 *
                 * this.useDashFrames = true;   // Alreday enabled. If false, your character will just be faster and look like it slide on the ground.
                 * this.useDuckingFrames = true; // Already enabled. If disable you will not see your bro ducking.
                 * this.canDoIndependentMeleeAnimation = false; // Animate custom melee. If you don't have one it will be buggy.
                 * 
                 * // Extra
                 * this.bloodColor = BloodColor.Red; // Change the blood color when hit or gib. Sewerage = "dirt" color.
                 * this.bloodCountAmount = 80; // ?
                 * this.deathSoundVolume = 0.4 // All in the name.
                 * 
                 * // ?
                 * this.canTouchLeftWalls = false; // ?
                 * this.canTouchRightWalls = false; // ?
                 * this.canLedgeGrapple = false; // ?
                 * this.useNewLedgeGrappleFrames = false; // Better animation who belong '.canLedgeGrapple', i guess.
                 * this.canCeilingHang = false; // ?
                 */

                // Some action of the bro.
                canChimneyFlip = true; // Do the things when you catch a ledge. !! Glitch if disable. !!
                doRollOnLand = true; // Do roll on land when it fall from far. !! Glitch if disable. !!

                // Improve animation.
                useDashFrames = true;
                useNewFrames = true; // I think it's for enable the following '.useNewXXXXFrame'. But change nothing when i disable it..
                useNewDuckingFrames = true; // Better animation.
                useNewThrowingFrames = true; // Better animation.
                useNewKnifingFrames = true; // Better animation, otherwise the sprite of knifing just slide on the ground.
                useNewKnifeClimbingFrames = true; // Better animation.
                canHear = true;


                // Extra.
                canBeCoveredInAcid = true; // All in the name. Don't know if it's already enabled..
                // End of the basic variable

                this.isHero = true; // Don't forget to add this.
                base.Awake();
            }

            protected override void CheckRescues()
            {
                if (HeroController.CheckRescueBros(base.playerNum, base.X, base.Y, 12f))
                {
                    this.DestroyUnit();
                }
                base.CheckRescues();
            }

            public virtual void SetTotalSpecialAmmo(int TotalSpecialAmmo)
            {
                if (TotalSpecialAmmo >= 6) TotalSpecialAmmo = 6;
                this.SpecialAmmo = TotalSpecialAmmo;
                this.originalSpecialAmmo = TotalSpecialAmmo;
                this._specialAmmo = TotalSpecialAmmo;
            }

            public Material materialNormal;
            public Material materialArmless;

            public Shrapnel bulletShell;
        }
    }
}