using System;
using System.Collections.Generic;
using Project.Core.Data;
using UnityEngine;

namespace Project.Core
{
    /// <summary>
    /// Central authority for the lightweight prototype game flow.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameStateController : MonoBehaviour
    {
        [SerializeField]
        private GameState startingState = GameState.Lobby;

        private PlayerProfile activeProfile;
        private IReadOnlyList<MissionNode> missionNodes = Array.Empty<MissionNode>();

        public static GameStateController Instance { get; private set; }

        public GameState CurrentState { get; private set; }
        public PlayerProfile ActiveProfile => activeProfile;
        public IReadOnlyList<MissionNode> MissionNodes => missionNodes;

        public event Action<GameState, GameState> OnStateChanged;
        public event Action OnLobbyEntered;
        public event Action OnBriefingEntered;
        public event Action OnMissionEntered;
        public event Action OnDebriefEntered;
        public event Action<PlayerProfile> OnProfileAssigned;
        public event Action<IReadOnlyList<MissionNode>> OnMissionGraphAssigned;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Duplicate GameStateController detected. Destroying the new instance.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetState(startingState, true);
        }

        private void Update()
        {
            HandleDebugInput();
        }

        public void GoToLobby() => SetState(GameState.Lobby);
        public void GoToBriefing() => SetState(GameState.Briefing);
        public void GoToMission() => SetState(GameState.Mission);
        public void GoToDebrief() => SetState(GameState.Debrief);

        public void SetPlayerProfile(PlayerProfile profile)
        {
            if (activeProfile == profile)
            {
                return;
            }

            activeProfile = profile;
            OnProfileAssigned?.Invoke(activeProfile);
        }

        public void SetMissionNodes(IReadOnlyList<MissionNode> nodes)
        {
            missionNodes = nodes ?? Array.Empty<MissionNode>();
            OnMissionGraphAssigned?.Invoke(missionNodes);
        }

        public void CycleForward()
        {
            var next = CurrentState switch
            {
                GameState.Lobby => GameState.Briefing,
                GameState.Briefing => GameState.Mission,
                GameState.Mission => GameState.Debrief,
                _ => GameState.Lobby
            };

            SetState(next);
        }

        public void CycleBackward()
        {
            var previous = CurrentState switch
            {
                GameState.Lobby => GameState.Debrief,
                GameState.Briefing => GameState.Lobby,
                GameState.Mission => GameState.Briefing,
                _ => GameState.Mission
            };

            SetState(previous);
        }

        [ContextMenu("Go To Lobby")]
        private void ContextGoToLobby() => GoToLobby();

        [ContextMenu("Go To Briefing")]
        private void ContextGoToBriefing() => GoToBriefing();

        [ContextMenu("Go To Mission")]
        private void ContextGoToMission() => GoToMission();

        [ContextMenu("Go To Debrief")]
        private void ContextGoToDebrief() => GoToDebrief();

        private void HandleDebugInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                GoToLobby();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GoToBriefing();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                GoToMission();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                GoToDebrief();
            }

            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                CycleForward();
            }
            else if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                CycleBackward();
            }
        }

        private void SetState(GameState newState, bool force = false)
        {
            if (!force && newState == CurrentState)
            {
                return;
            }

            var previousState = CurrentState;
            CurrentState = newState;
            Debug.Log($"GameState changed: {previousState} -> {CurrentState}");
            OnStateChanged?.Invoke(previousState, CurrentState);

            switch (CurrentState)
            {
                case GameState.Lobby:
                    OnLobbyEntered?.Invoke();
                    break;
                case GameState.Briefing:
                    OnBriefingEntered?.Invoke();
                    break;
                case GameState.Mission:
                    OnMissionEntered?.Invoke();
                    break;
                case GameState.Debrief:
                    OnDebriefEntered?.Invoke();
                    break;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
