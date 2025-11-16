using System.Collections.Generic;
using System.Linq;
using System.Text;
using Project.Core;
using Project.Core.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.UI
{
    /// <summary>
    /// Lightweight debug UI that exposes the prototype GameState flow
    /// and a peek at sample profile/mission data.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class DebugStatePanel : MonoBehaviour
    {
        [SerializeField]
        private VisualTreeAsset panelAsset;

        [SerializeField]
        private PlayerProfile playerProfile;

        [SerializeField]
        private MissionNode featuredMission;

        private UIDocument document;
        private Label stateLabel;
        private Label profileInfoLabel;
        private Label missionInfoLabel;
        private GameStateController controller;

        private void Awake()
        {
            EnsureDocument();
            ApplyVisualTreeAsset();
        }

        private void OnEnable()
        {
            EnsureDocument();
            ApplyVisualTreeAsset();
            CacheVisualElements();
            WireButtons();
            AttachController();
            SyncInitialData();
            RefreshStateView(controller != null ? controller.CurrentState : GameState.Lobby);
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.OnStateChanged -= HandleStateChanged;
                controller.OnProfileAssigned -= HandleProfileAssigned;
                controller.OnMissionGraphAssigned -= HandleMissionGraphAssigned;
                controller = null;
            }
        }

        public void SetPlayerProfile(PlayerProfile profile)
        {
            playerProfile = profile;
            RefreshProfileView();
        }

        public void SetFeaturedMission(MissionNode mission)
        {
            featuredMission = mission;
            RefreshMissionView();
        }

        private void EnsureDocument()
        {
            if (document != null)
            {
                return;
            }

            document = GetComponent<UIDocument>();
            if (document == null)
            {
                document = gameObject.AddComponent<UIDocument>();
            }
        }

        private void ApplyVisualTreeAsset()
        {
            if (document == null || panelAsset == null)
            {
                return;
            }

            if (document.visualTreeAsset != panelAsset)
            {
                document.visualTreeAsset = panelAsset;
            }
        }

        private void CacheVisualElements()
        {
            if (document == null)
            {
                return;
            }

            var root = document.rootVisualElement;
            if (root == null)
            {
                Debug.LogWarning("DebugStatePanel: Visual tree not ready.");
                return;
            }

            stateLabel = root.Q<Label>("state-label");
            profileInfoLabel = root.Q<Label>("profile-info");
            missionInfoLabel = root.Q<Label>("mission-info");
        }

        private void WireButtons()
        {
            if (document?.rootVisualElement == null)
            {
                return;
            }

            RegisterButton("btn-lobby", () => controller?.GoToLobby());
            RegisterButton("btn-briefing", () => controller?.GoToBriefing());
            RegisterButton("btn-mission", () => controller?.GoToMission());
            RegisterButton("btn-debrief", () => controller?.GoToDebrief());
        }

        private void RegisterButton(string buttonName, System.Action callback)
        {
            var button = document.rootVisualElement.Q<Button>(buttonName);
            if (button == null)
            {
                return;
            }

            button.clicked -= callback;
            button.clicked += callback;
        }

        private void AttachController()
        {
            if (controller != null)
            {
                controller.OnStateChanged -= HandleStateChanged;
                controller.OnProfileAssigned -= HandleProfileAssigned;
                controller.OnMissionGraphAssigned -= HandleMissionGraphAssigned;
            }

            controller = GameStateController.Instance ?? FindObjectOfType<GameStateController>();
            if (controller != null)
            {
                controller.OnStateChanged += HandleStateChanged;
                controller.OnProfileAssigned += HandleProfileAssigned;
                controller.OnMissionGraphAssigned += HandleMissionGraphAssigned;
            }
        }

        private void SyncInitialData()
        {
            if (controller != null)
            {
                if (playerProfile == null && controller.ActiveProfile != null)
                {
                    playerProfile = controller.ActiveProfile;
                }

                if (featuredMission == null && controller.MissionNodes != null)
                {
                    featuredMission = controller.MissionNodes.FirstOrDefault(node => node != null);
                }
            }

            RefreshProfileView();
            RefreshMissionView();
        }

        private void HandleStateChanged(GameState previous, GameState current)
        {
            RefreshStateView(current);
        }

        private void HandleProfileAssigned(PlayerProfile profile)
        {
            SetPlayerProfile(profile);
        }

        private void HandleMissionGraphAssigned(IReadOnlyList<MissionNode> nodes)
        {
            MissionNode chosen = featuredMission;
            if (nodes != null)
            {
                bool containsCurrent = nodes.Any(node => node == featuredMission && node != null);
                if (!containsCurrent)
                {
                    chosen = nodes.FirstOrDefault(node => node != null);
                }
            }

            SetFeaturedMission(chosen);
        }

        private void RefreshStateView(GameState current)
        {
            if (stateLabel == null)
            {
                return;
            }

            stateLabel.text = $"Current State: {current}";
        }

        private void RefreshProfileView()
        {
            if (profileInfoLabel == null)
            {
                return;
            }

            if (playerProfile == null)
            {
                profileInfoLabel.text = "No profile assigned.";
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Callsign: {playerProfile.Callsign}");
            builder.AppendLine($"Preferred Role: {playerProfile.PreferredRole}");
            builder.AppendLine($"Level: {playerProfile.Level} | XP: {playerProfile.TotalExperience}");
            builder.AppendLine($"Intel: {playerProfile.IntelBalance} | Missions: {playerProfile.MissionsCompleted}");

            var unlockedNames = playerProfile.UnlockedNodes
                .Where(n => n != null)
                .Select(n => n.DisplayName)
                .ToList();

            builder.AppendLine($"Unlocked Nodes: {unlockedNames.Count}");
            if (unlockedNames.Count > 0)
            {
                builder.AppendLine(string.Join(", ", unlockedNames));
            }

            profileInfoLabel.text = builder.ToString().Trim();
        }

        private void RefreshMissionView()
        {
            if (missionInfoLabel == null)
            {
                return;
            }

            if (featuredMission == null)
            {
                missionInfoLabel.text = "No mission assigned.";
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine($"ID: {featuredMission.Id}");
            builder.AppendLine($"Title: {featuredMission.DisplayName}");
            builder.AppendLine($"Role Focus: {featuredMission.RecommendedRole}");
            builder.AppendLine($"Difficulty: {featuredMission.DifficultyRating:0.0}");
            builder.AppendLine($"Environment: {featuredMission.EnvironmentTag}");
            missionInfoLabel.text = builder.ToString().Trim();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                RefreshProfileView();
                RefreshMissionView();
            }
        }
#endif
    }
}
