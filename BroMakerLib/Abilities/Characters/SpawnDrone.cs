using System;
using System.Collections.Generic;
using BroMakerLib.Attributes;
using BroMakerLib.Loggers;
using Newtonsoft.Json;
using RocketLib.JsonConverters;
using UnityEngine;

namespace BroMakerLib.Abilities.Characters
{
    /// <summary>
    /// Spawn a drone. DON'T USE, drones are bugged and crashes could happen
    /// </summary>
    [AbilityPreset("SpawnDrone")]
    public class SpawnDrone : CharacterAbility
    {
        public int droneType = 0;
        public int droneMaxCount = 1;
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 spawnPositionOffset = new Vector2(0, 3f);
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 spawnI = Vector2.zero;

        protected bool _characterIsNotBro = false;
        protected List<Drone> _spawnedDrone = new List<Drone>();

        public override void All(string calledFromMethod, params object[] objects)
        {
            if (_characterIsNotBro || IsUnityMethod(calledFromMethod) || _spawnedDrone.Count < droneMaxCount)
                return;

            TrySpawnDrone();
        }

        public void TrySpawnDrone()
        {
            if (_characterIsNotBro || _spawnedDrone.Count > droneMaxCount)
                return;

            if (droneType < 0 || droneType >= Map.Instance.sharedObjectsReference.Asset.drones.Length)
            {
                throw new IndexOutOfRangeException($"{nameof(droneType)} should be greater or equal to 0 and smaller than {Map.Instance.sharedObjectsReference.Asset.drones.Length}");
            }
            Drone drone = null;
            try
            {
                drone = MapController.SpawnDrone_Networked(Map.Instance.sharedObjectsReference.Asset.drones[droneType],
                owner as BroBase,
                owner.X + OwnerDirection * spawnPositionOffset.x,
                owner.Y + spawnPositionOffset.y,
                spawnI.x,
                spawnI.y
                );
                if (drone == null)
                {
                    throw new Exception("drone has not been instantiated");
                }
                owner.SetFieldValue("currentDrone", drone);
                _spawnedDrone.Add(drone);
            }
            catch (Exception ex)
            {
                BMLogger.Error(ex);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _characterIsNotBro = owner as BroBase == null;
            _spawnedDrone = new List<Drone>();
        }

        protected override void Update()
        {
            base.Update();
            if (_characterIsNotBro || _spawnedDrone.Count == 0)
                return;

            var temp = new List<Drone>();
            foreach (Drone drone in _spawnedDrone)
            {
                if (drone != null || drone.IsAlive())
                {
                    temp.Add(drone);
                }
            }
            _spawnedDrone = temp;
        }
    }
}
