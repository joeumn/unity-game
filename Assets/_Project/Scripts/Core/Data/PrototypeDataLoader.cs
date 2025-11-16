using System.Collections.Generic;
using System.Linq;
using Project.Core.Data;
using Project.Core.Missions;
using Project.UI;
using UnityEngine;

namespace Project.Core
{
    /// <summary>
    /// Offline bootstrapper that injects prototype ScriptableObject data
    /// into runtime systems so the phase-one scene can run without backend services.
    /// </summary>
    public class PrototypeDataLoader : MonoBehaviour
    {
        [Header("Data Sources")]
        [SerializeField]
        private PlayerProfile playerProfile;

        [SerializeField]
        private List<MissionNode> missionNodes = new List<MissionNode>();

        [SerializeField]
        private MissionNode forcedMission;

        [Header("Optional Overrides")]
        [SerializeField]
        private GameStateController controllerOverride;

        [SerializeField]
        private DebugStatePanel debugPanelOverride;

        [SerializeField]
        private ObjectiveController objectiveControllerOverride;

        private GameStateController controller;
        private DebugStatePanel debugPanel;
        private ObjectiveController objectiveController;

        private void Awake()
        {
            controller = controllerOverride ?? GameStateController.Instance ?? FindObjectOfType<GameStateController>();
            debugPanel = debugPanelOverride ?? FindObjectOfType<DebugStatePanel>();
            objectiveController = objectiveControllerOverride ?? FindObjectOfType<ObjectiveController>();

            RegisterProfile();
            var activeMission = RegisterMissionGraph();
            RegisterObjectives(activeMission);
        }

        private void RegisterProfile()
        {
            if (controller != null)
            {
                controller.SetPlayerProfile(playerProfile);
            }

            if (debugPanel != null)
            {
                debugPanel.SetPlayerProfile(playerProfile);
            }
        }

        private MissionNode RegisterMissionGraph()
        {
            var cleanedNodes = missionNodes?.Where(node => node != null).ToList() ?? new List<MissionNode>();
            MissionNode featured = forcedMission != null && cleanedNodes.Contains(forcedMission)
                ? forcedMission
                : cleanedNodes.FirstOrDefault();

            if (controller != null)
            {
                controller.SetMissionNodes(cleanedNodes);
            }

            if (debugPanel != null)
            {
                if (featured == null && controller != null)
                {
                    featured = controller.MissionNodes?.FirstOrDefault();
                }

                debugPanel.SetFeaturedMission(featured);
            }

            return featured;
        }

        private void RegisterObjectives(MissionNode mission)
        {
            if (objectiveController == null)
            {
                return;
            }

            var objectiveData = mission?.Objectives;
            objectiveController.InitializeObjectives(objectiveData);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (missionNodes == null)
            {
                missionNodes = new List<MissionNode>();
            }
        }
#endif
    }
}
