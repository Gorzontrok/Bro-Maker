using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BroMakerLib.CustomObjects.Bros;

namespace BroMakerLib
{
    public static class BroMakerUtilities
    {
        /// <summary>
        /// Determines if the specified unit is a boss
        /// </summary>
        /// <param name="unit">Unit to check</param>
        /// <returns>True if unit is a boss</returns>
        public static bool IsBoss(Unit unit)
        {
            return unit.CompareTag("Boss") || unit is DolphLundrenSoldier || unit is SatanMiniboss || (unit is AlienSandWorm && !(unit is AlienWormFacehuggerLauncher)) || 
                unit is TankBig || unit is Mookopter || unit is GoliathMech;
        }

        /// <summary>
        /// Changes the specified players special materials
        /// </summary>
        /// <param name="playerNum"></param>
        /// <param name="specialMaterials">List of materials to set special to, if only one is specified, all icons will be set to it</param>
        /// <param name="offset">Offset to move all special icons horizontally or vertically</param>
        /// <param name="spacing">Spacing between special icons</param>
        public static void SetSpecialMaterials(int playerNum, List<Material> specialMaterials, Vector2 offset, float spacing )
        {
            PlayerHUD hud = HeroController.players[playerNum].hud;
            for (int i = 0; i < hud.grenadeIcons.Length; i++)
            {
                if (playerNum % 2 == 0)
                {
                    hud.grenadeIcons[i].SetOffset(new Vector3(offset.x + i * spacing, offset.y, 0f));
                }
                else
                {
                    hud.grenadeIcons[i].SetOffset(new Vector3(-1 * (offset.x + i * spacing), offset.y, 0f));
                }
            }

            if (specialMaterials != null)
            {
                List<Material> specialIcons = specialMaterials;
                if (specialIcons.Count() > 1)
                {
                    for (int i = 0; i < specialIcons.Count(); ++i)
                    {
                        hud.grenadeIcons[i].GetComponent<Renderer>().material = specialIcons[i];
                    }
                }
                else if (specialIcons.Count() > 0)
                {
                    for (int i = 0; i < hud.grenadeIcons.Count(); ++i)
                    {
                        hud.grenadeIcons[i].GetComponent<Renderer>().material = specialIcons[0];
                    }
                }
            }
        }

        /// <summary>
        /// Changes the specified players special materials
        /// </summary>
        /// <param name="playerNum"></param>
        /// <param name="specialMaterial">Material to set special to, all icons will be set to it</param>
        /// <param name="offset">Offset to move all special icons horizontally or vertically</param>
        /// <param name="spacing">Spacing between special icons</param>
        public static void SetSpecialMaterials(int playerNum, Material specialMaterial, Vector2 offset, float spacing)
        {
            PlayerHUD hud = HeroController.players[playerNum].hud;
            for (int i = 0; i < hud.grenadeIcons.Length; i++)
            {
                if (playerNum % 2 == 0)
                {
                    hud.grenadeIcons[i].SetOffset(new Vector3(offset.x + i * spacing, offset.y, 0f));
                }
                else
                {
                    hud.grenadeIcons[i].SetOffset(new Vector3(-1 * (offset.x + i * spacing), offset.y, 0f));
                }
            }

            if (specialMaterial != null)
            {
                for (int i = 0; i < hud.grenadeIcons.Count(); ++i)
                {
                    hud.grenadeIcons[i].GetComponent<Renderer>().material = specialMaterial;
                }
            }
        }
    }
}
