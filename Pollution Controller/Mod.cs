using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.Serialization.Entities;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.Simulation;
using Unity.Entities;
using System.Threading.Tasks;
using Game.Prefabs;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Game.Objects;
using System.Collections;
using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Pollution_Controller
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(Pollution_Controller)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        private Setting m_Setting;
        public PollutionSystem _pollutionSystem;

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));

            AssetDatabase.global.LoadSettings(nameof(Pollution_Controller), m_Setting, new Setting(this));

            if (_pollutionSystem == null)
            {
                _pollutionSystem = new PollutionSystem(this);
            }
            World.DefaultGameObjectInjectionWorld.AddSystemManaged(_pollutionSystem);


            
            //updateSystem.UpdateAfter<PollutionSystem>(SystemUpdatePhase.PrefabReferences);
            updateSystem.UpdateBefore<PollutionSystem>(SystemUpdatePhase.Rendering);
            log.Info($"Before Rendering Ran");
            updateSystem.UpdateAt<PollutionSystem>(SystemUpdatePhase.Rendering);
            log.Info($"At Rendering Ran");
            updateSystem.UpdateAt<PollutionSystem>(SystemUpdatePhase.MainLoop);
            log.Info($"At Main Loop Ran");






        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            if (m_Setting != null)
            {
                m_Setting.UnregisterInOptionsUI();
                m_Setting = null;
            }
        }
    }



    public partial class PollutionSystem : GameSystemBase
    {
        public Mod _mod;
        //public PollutionPrefab pollutionPrefab;
        public PrefabSystem _prefabSystem;
        public FieldInfo m_Prefabs;



        public PollutionSystem(Mod mod)
        {
            _mod = mod;
        }
        protected override void OnCreate()
        {
            base.OnCreate();
            //_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            _prefabSystem = World.GetExistingSystemManaged<PrefabSystem>();


        }


        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            if (!mode.IsGameOrEditor())

                return;

            UpdatePrefabs();




        }

        public void UpdatePrefabs()
        {
            FieldInfo m_Prefabs = typeof(PrefabSystem).GetField("m_Prefabs", BindingFlags.Instance | BindingFlags.NonPublic);
            List<PrefabBase> prefabs = (List<PrefabBase>)m_Prefabs.GetValue(_prefabSystem);

            foreach (PrefabBase prefab in prefabs)
            {
                if (prefab is PollutionPrefab)
                {
                    PollutionPrefab pollutionPrefab = (PollutionPrefab)prefab; // Cast to PollutionPrefab
                    Mod.log.Info($"Found PollutionPrefab: {pollutionPrefab.name}");

                    // Access fields of PollutionPrefab
                    FieldInfo[] prefabFields = typeof(PollutionPrefab).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    foreach (FieldInfo field in prefabFields)
                    {
                        // Access field value
                        object fieldValue = field.GetValue(pollutionPrefab);
                        Mod.log.Info($"Field '{field.Name}': {fieldValue}");
                    }
                    pollutionPrefab.m_AirMultiplier = 0f;
                    pollutionPrefab.m_GroundMultiplier = 0f;
                    pollutionPrefab.m_NoiseMultiplier = 0f;

                    Mod.log.Info($"Field active set to: {pollutionPrefab.active}");
                    Mod.log.Info($"Field m_AirMultiplier set to: {pollutionPrefab.m_AirMultiplier}");
                    Mod.log.Info($"Field m_GroundMultiplier set to: {pollutionPrefab.m_GroundMultiplier}");
                    Mod.log.Info($"Field m_NoiseMultiplier set to: {pollutionPrefab.m_NoiseMultiplier}");

                    _prefabSystem.Update();

                    Mod.log.Info($"AFTER PREFAB UPDATE Field active set to: {pollutionPrefab.active}");
                    Mod.log.Info($"AFTER PREFAB UPDATE Field m_AirMultiplier set to: {pollutionPrefab.m_AirMultiplier}");
                    Mod.log.Info($"AFTER PREFAB UPDATE Field m_GroundMultiplier set to: {pollutionPrefab.m_GroundMultiplier}");
                    Mod.log.Info($"AFTER PREFAB UPDATE Field m_NoiseMultiplier set to: {pollutionPrefab.m_NoiseMultiplier}");

                    
                }
            }
        }

        protected override void OnUpdate()
        {
            

        }


        public void OnGameExit()
        {

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }

}
