using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Core.Data
{
    [CreateAssetMenu(menuName = "Project/Profiles/Player Profile", fileName = "PlayerProfile")]
    public class PlayerProfile : ScriptableObject
    {
        [SerializeField]
        private string callsign = "Operator";

        [SerializeField]
        private MissionRoleFocus preferredRole = MissionRoleFocus.Both;

        [SerializeField]
        private List<MissionNode> unlockedNodes = new List<MissionNode>();

        [SerializeField, Min(1)]
        private int level = 1;

        [SerializeField, Min(0)]
        private int totalExperience = 0;

        [SerializeField, Min(0)]
        private int intelBalance = 0;

        [SerializeField, Min(0)]
        private int missionsCompleted = 0;

        [SerializeField]
        private LoadoutStub loadout = new LoadoutStub();

        public string Callsign => callsign;
        public MissionRoleFocus PreferredRole => preferredRole;
        public IReadOnlyList<MissionNode> UnlockedNodes => unlockedNodes;
        public int Level => level;
        public int TotalExperience => totalExperience;
        public int IntelBalance => intelBalance;
        public int MissionsCompleted => missionsCompleted;
        public LoadoutStub Loadout => loadout;

        [Serializable]
        public class LoadoutStub
        {
            public string primaryTool = "Shock Dagger";
            public string secondaryTool = "Pulse Pistol";
            public string supportGadget = "Drone Swarm";
            public string defensiveModule = "Firewall Weave";
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(callsign))
            {
                callsign = name;
            }

            if (level < 1)
            {
                level = 1;
            }

            if (totalExperience < 0)
            {
                totalExperience = 0;
            }

            if (intelBalance < 0)
            {
                intelBalance = 0;
            }

            if (missionsCompleted < 0)
            {
                missionsCompleted = 0;
            }

            if (loadout == null)
            {
                loadout = new LoadoutStub();
            }

            for (int i = unlockedNodes.Count - 1; i >= 0; i--)
            {
                if (unlockedNodes[i] == null)
                {
                    unlockedNodes.RemoveAt(i);
                }
            }
        }
#endif
    }
}
