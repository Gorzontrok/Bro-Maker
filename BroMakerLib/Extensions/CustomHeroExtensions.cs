using BroMakerLib.CustomObjects.Bros;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BroMakerLib
{
    public static class CustomHeroExtensions
    {
        public static void DisturbWildLife<T>(this T hero, float range) where T : CustomHero
        {
            Map.DisturbWildLife(hero.X, hero.Y, range, hero.playerNum);
        }

        public static void HurtWildLife<T>(this T hero, float range) where T : CustomHero
        {
            Map.HurtWildLife(hero.X, hero.Y, range);
        }

        public static bool HitUnits<T>(this T hero, DamageObjectE doe, List<Unit> hitUnits = null) where T : CustomHero
        {
            return hero.HitUnits(doe.damage, doe.damageType, doe.range, doe.Force, doe.penetrates, doe.knock, doe.canGib, doe.ignoreDeadUnit, doe.canHeadshot, hitUnits);
        }
        public static bool HitUnits<T>(this T hero, int damage, DamageType damageType, float range, Vector2 force, bool penetrates = false, bool knock = false, bool canGib = false, bool ignoreDeadUnit = false, bool canHeadshot = false, List<Unit> hitUnits = null) where T : CustomHero
        {
            return Map.HitUnits(hero, hero.playerNum, damage, damageType, range, hero.X, hero.Y, force.x, force.y, penetrates, knock, canGib, hitUnits, ignoreDeadUnit, canHeadshot);
        }
        public static bool HitUnits<T>(this T hero, DamageObjectE doe, out bool hitImpenetrableDoodad) where T : CustomHero
        {
            return hero.DamageDoodads(doe.damage, doe.damageType, doe.range, doe.Force, out hitImpenetrableDoodad);
        }
        public static bool DamageDoodads<T>(this T hero, int damage, DamageType damageType, float range, Vector2 force, out bool hitImpenetrableDoodad) where T : CustomHero
        {
            return Map.DamageDoodads(damage, damageType, hero.X, hero.Y, force.x, force.y, range, hero.playerNum, out hitImpenetrableDoodad, hero);
        }

        public static void AlertNearbyMook<T>(this T hero, Vector2 range) where T : CustomHero
        {
            Map.AlertNearbyMooks(hero.X, hero.Y, range.x, range.y, hero.playerNum);
        }

        public static void AttractAliens<T>(this T hero, Vector2 range) where T : CustomHero
        {
            Map.AttractAliens(hero.X, hero.Y, range.x, range.y);
        }
        public static void AttractMooks<T>(this T hero, Vector2 range) where T : CustomHero
        {
            Map.AttractMooks(hero.X, hero.Y, range.x, range.y);
        }
        public static void BlindUnits<T>(this T hero, float range, float blindTime = 9f) where T : CustomHero
        {
            Map.BlindUnits(hero.playerNum, hero.X, hero.Y, range, blindTime);
        }
        public static void BotherNearbyMooks<T>(this T hero, Vector2 range) where T : CustomHero
        {
            Map.BotherNearbyMooks(hero.X, hero.Y, range.x, range.y, hero.playerNum);
        }

        public static void BurnUnitsAround_Local<T>(this T hero, DamageObjectE doe, bool setGroundAlight) where T : CustomHero
        {
            hero.BurnUnitsAround_Local(doe.damage, doe.range, doe.penetrates, setGroundAlight);
        }
        public static void BurnUnitsAround_Local<T>(this T hero, int damage, float range, bool penetrates = false, bool setGroundAlight = false) where T : CustomHero
        {
            Map.BurnUnitsAround_Local(hero, hero.playerNum, damage, range, hero.X, hero.Y, penetrates, setGroundAlight);
        }
    }
}
