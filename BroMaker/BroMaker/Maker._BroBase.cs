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

                // The foolowing variable is some variable that you can use. Read the description of each for more information.

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
                 * this.maxFallSpeed = -300f;   // The speed for falling when he is in the air.
                 * this.fireRate = 0.1f;        // The time between each bullet.
                 * this.fireDelay = 0.0f;       // the delay between press fire and the bro fire. Example on Brominator.
                 * this._jumpForce = 260f;      // Jump height.
                 * 
                 * //Improve animation.
                 * this.useNewPushingFrames = false;            // Cutom animation when pushing something. The gun stay reverse after pushing the block.
                 * this.useNewLadderClimbingFrames = false;     // Custom climbing frame that isn't use for bro. Some sprite are missing.
                 * this.useLadderClimbingTransition = false;    // Do nothing, still missing Enter and Exit frame on the ladder.
                 *
                 * this.useDashFrames = true;   // Alreday enabled. If false, your character will just be faster and look like it slide on the ground.
                 * this.useDuckingFrames = true; // Already enabled. If disable you will not see your bro ducking.
                 * this.canDoIndependentMeleeAnimation = false; // Animate custom melee. If you don't have one it will be buggy.
                 * 
                 * // Extra
                 * this.bloodColor = BloodColor.Red; // Change the blood color when hit or gib. Sewerage = "dirt" color.
                 * 
                 * // ?
                 * this.canTouchLeftWalls = false; // ?
                 * this.canTouchRightWalls = false; // ?
                 * this.canLedgeGrapple = false; // ?
                 * this.useNewLedgeGrappleFrames = false; // Better animation who belong '.canLedgeGrapple', i guess.
                 * this.canCeilingHang = false; // ?
                 */

                // Some action of the bro.
                this.canChimneyFlip = _canChimneyFlip; // Do the things when you catch a ledge. !! Glitch if disable. !!
                this.doRollOnLand = _doRollOnLand; // Do roll on land when it fall from far. !! Glitch if disable. !!

                // Improve animation.
                this.useNewFrames = _useNewFrames; // I think it's for enable the following '.useNewXXXXFrame'. But change nothing when i disable it..
                this.useNewDuckingFrames = _useNewDuckingFrames; // Better animation.
                this.useNewThrowingFrames = _useNewThrowingFrames; // Better animation.
                this.useNewKnifingFrames = _useNewKnifingFrames; // Better animation, otherwise the sprite of knifing just slide on the ground.
                this.useNewKnifeClimbingFrames = _useNewKnifeClimbingFrames; // Better animation.

                // Extra.
                this.canBeCoveredInAcid = _canBeCoveredInAcid; // All in the name. Don't know if it's already enabled..

                // End of the following variable

                this.isHero = true; // Don't forget to add this.
                base.Awake();
            }

            protected bool _canChimneyFlip = true;          // Do the things when you catch a ledge. !! Glitch if disable. !!
            protected bool _doRollOnLand = true;            // Do roll on land when it fall from far. !! Glitch if disable. !!
            
            // Improve animation.
            protected bool _useNewFrames = true;            // I think it's for enable the following '.useNewXXXXFrame'. But change nothing when i disable it..
            protected bool _useNewDuckingFrames = true;     // Better animation.
            protected bool _useNewThrowingFrames = true;    // Better animation.
            protected bool _useNewKnifingFrames = true;     // Better animation, otherwise the sprite when knifing just slide on the ground.
            protected bool _useNewKnifeClimbingFrames = true; // Better animation.

            //Some extra.
            protected bool _canBeCoveredInAcid = true;      // All in the name.


            public Material materialNormal;
            public Material materialArmless;
        }
    }
}