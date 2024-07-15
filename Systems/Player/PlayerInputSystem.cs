using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Transforms;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    public partial class PlayerInputSystem : SystemBase
    {
        PlayerInputActions playerInputActions;
        protected override void OnCreate()
        {
            playerInputActions = new PlayerInputActions();
        }

        protected override void OnStartRunning()
        {
            playerInputActions.Enable();
            playerInputActions.KeyboardMap.MouseLeftClick.performed += OnShootCannon;
            playerInputActions.KeyboardMap.MouseRightClick.performed += OnStartFiringCIWS;
            playerInputActions.KeyboardMap.MouseRightClick.canceled += OnStopFiringCIWS;
            playerInputActions.KeyboardMap.LaunchMissile.performed += OnFireMissile;
            playerInputActions.KeyboardMap.LaunchDrone.performed += OnSelectDrone;
            playerInputActions.KeyboardMap.DeployChaff.performed += OnDeployChaff;
            playerInputActions.KeyboardMap.ToggleMenu.performed += OnToggleMenu;
        }

        protected override void OnStopRunning()
        {
            playerInputActions.Disable();
            playerInputActions.KeyboardMap.MouseLeftClick.performed -= OnShootCannon;
            playerInputActions.KeyboardMap.MouseRightClick.performed -= OnStartFiringCIWS;
            playerInputActions.KeyboardMap.MouseRightClick.canceled -= OnStopFiringCIWS;
            playerInputActions.KeyboardMap.LaunchMissile.performed -= OnFireMissile;
            playerInputActions.KeyboardMap.LaunchDrone.performed -= OnSelectDrone;
            playerInputActions.KeyboardMap.DeployChaff.performed -= OnDeployChaff;
            playerInputActions.KeyboardMap.ToggleMenu.performed -= OnToggleMenu;
        }

        protected override void OnUpdate()
        {

            if (!SystemAPI.HasSingleton<PlayerTag>())
                return;

            if (!SystemAPI.HasSingleton<InputMoveDirection>())
                return;

            if (!SystemAPI.HasSingleton<InputMousePosition>())
                return;

            if (!SystemAPI.HasSingleton<InputMouseClick>())
                return;

            if (!SystemAPI.HasSingleton<InputKeyPress>())
                return;

            if (!SystemAPI.HasSingleton<TargetClosestToCenter>())
                return;

            if (!SystemAPI.HasSingleton<CameraPosition>())
                return;

            if (!SystemAPI.HasSingleton<GameConfigData>())
                return;

            if (!SystemAPI.HasSingleton<AudioConfigData>())
                return;

            var playerentity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var unit = SystemAPI.GetComponent<Unit>(playerentity);
            var gameConfigData = SystemAPI.GetSingleton<GameConfigData>();
            var audioConfigData = SystemAPI.GetSingleton<AudioConfigData>();

            if (gameConfigData.IsGameOver)
                return;

            // Move Input
            var moveInput = playerInputActions.KeyboardMap.Movement.ReadValue<Vector2>();
            SystemAPI.SetSingleton(new InputMoveDirection { Value = new float3(moveInput.x, 0f, moveInput.y) });

            // Mouse Position Input
            var mouseInput = playerInputActions.KeyboardMap.MousePosition.ReadValue<Vector2>();
            var ray = Camera.main.ScreenPointToRay(mouseInput);
            var plane = new Plane(Vector3.up, Vector3.zero);
            float distance;

            if (plane.Raycast(ray, out distance))
                SystemAPI.SetSingleton(new InputMousePosition
                {
                    OriginPosition = ray.origin,
                    EndPosition = ray.GetPoint(distance)
                });

            var reticlePosition = ray.GetPoint(distance);
            SystemAPI.GetComponentRW<TargetPosition>(playerentity).ValueRW.Value = reticlePosition;
            Debug.DrawLine(ray.origin, reticlePosition);

            // Update Camera Positions
            var cameraTransform = Camera.main.transform;
            SystemAPI.SetSingleton(new CameraPosition { Value = cameraTransform.position });
            var playerTransform = SystemAPI.GetComponent<LocalToWorld>(playerentity);
            cameraTransform.position = playerTransform.Position + new float3(0f, 25.0f, -25.0f);
            cameraTransform.LookAt(playerTransform.Position);

            // Update Targeting Reticle Positioins
            var isClosestTarget = !SystemAPI.GetSingleton<TargetClosestToCenter>().Entity.Equals(Entity.Null);
            UIManager.instance.UpdateTargetReticlePosition(reticlePosition);
            UIManager.instance.UpdateTargetReticleActiveColour(isClosestTarget);

            var currentTargetBuffer = SystemAPI.GetSingletonBuffer<CurrentTarget>();
            UIManager.instance.UpdateLockOnReticleRingPosition(reticlePosition);
            UIManager.instance.UpdateLockOnReticleRingColour(currentTargetBuffer.Length > 0);

            // Update HealthBar
            if (unit.IsHealthUpdated)
            {
                UIManager.instance.UpdatePlayerHealthSlider(math.clamp(unit.UnitCurrentHitPoints / unit.UnitMaxHitPoints, 0f, 1f));
                unit.IsHealthUpdated = false;
                SystemAPI.SetComponent(playerentity, unit);

                if (audioConfigData.ExplosionHealthCounter - unit.UnitCurrentHitPoints > 10f && gameConfigData.ExplosionSoundFXTimerInSeconds <= 0f)
                {
                    AudioManager.instance.PlayExplosionFX();
                    gameConfigData.ExplosionSoundFXTimerInSeconds = gameConfigData.ExplosionSoundFXCooldownInSeconds;
                    audioConfigData.ExplosionHealthCounter = unit.UnitCurrentHitPoints;
                }
                
                if (unit.UnitCurrentHitPoints <= 0f)
                {
                    gameConfigData.IsGameOver = true;
                    UIManager.instance.GameOverScreen(gameConfigData.Score);
                    AudioManager.instance.PlayGameOverBGM();
                }
                
            }

            // Update Scoreboard
            if (gameConfigData.IsScoreUpdated)
            {
                UIManager.instance.UpdateScore(gameConfigData.Score);
                gameConfigData.IsScoreUpdated = false;
            }

            // Play Audio
            if (gameConfigData.CannonShootSoundFXTimerInSeconds > 0f)
                gameConfigData.CannonShootSoundFXTimerInSeconds -= SystemAPI.Time.DeltaTime;

            else if (audioConfigData.IsPlayingCannonShootSoundFX)
            {
                AudioManager.instance.PlayCannonShootFX();
                audioConfigData.IsPlayingCannonShootSoundFX = false;
                gameConfigData.CannonShootSoundFXTimerInSeconds = gameConfigData.CannonShootSoundFXCooldownInSeconds; 
            }

            if (gameConfigData.MissileLaunchSoundFXTimerInSeconds > 0f)
                gameConfigData.MissileLaunchSoundFXTimerInSeconds -= SystemAPI.Time.DeltaTime;
            
            else if (audioConfigData.IsPlayingMissileLaunchSoundFX)
            {
                AudioManager.instance.PlayMissileLaunchFX();
                audioConfigData.IsPlayingMissileLaunchSoundFX = false;
                gameConfigData.MissileLaunchSoundFXTimerInSeconds = gameConfigData.MissileLaunchSoundFXCooldownInSeconds;
            }

            if (gameConfigData.CIWSFiringSoundFXTimerInSeconds > 0f)
                gameConfigData.CIWSFiringSoundFXTimerInSeconds -= SystemAPI.Time.DeltaTime;

            else if (audioConfigData.IsPlayingCIWSFiringSoundFX)
            {
                AudioManager.instance.PlayCIWSFiringFX();
                gameConfigData.CIWSFiringSoundFXTimerInSeconds = gameConfigData.CIWSFiringSoundFXCooldownInSeconds;
            }

            if (gameConfigData.ExplosionSoundFXTimerInSeconds > 0f)
                gameConfigData.ExplosionSoundFXTimerInSeconds -= SystemAPI.Time.DeltaTime;

            SystemAPI.SetSingleton(gameConfigData);
            SystemAPI.SetSingleton(audioConfigData);
        }

        void OnShootCannon(InputAction.CallbackContext context)
        {
            if (!SystemAPI.HasSingleton<PlayerTag>())
                return;

            if (!SystemAPI.HasSingleton<InputMouseClick>())
                return;

            var mouseInput = SystemAPI.GetSingleton<InputMouseClick>();
            mouseInput.IsLeftClick = true;
            SystemAPI.SetSingleton(mouseInput);

            var playerentity = SystemAPI.GetSingletonEntity<PlayerTag>();
            SystemAPI.SetComponent(playerentity, new CannonPlayerShootCommand
            {
                CooldownTimerInSeconds = 0f,
                MagazineCapacityCounter = 5,
                IsShootCommand = true,
            });
        }

        void OnStartFiringCIWS(InputAction.CallbackContext context)
        {
            if (!SystemAPI.HasSingleton<PlayerTag>())
                return;

            if (!SystemAPI.HasSingleton<InputMouseClick>())
                return;

            var mouseInput = SystemAPI.GetSingleton<InputMouseClick>();
            mouseInput.IsRightClick = true;
            SystemAPI.SetSingleton(mouseInput);

            var playerentity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var fireControl = SystemAPI.GetComponent<CIWSPlayerFireCommand>(playerentity);
            fireControl.IsFireCommand = true;
            fireControl.ROFTimerInSeconds = 0f;
            SystemAPI.SetComponent(playerentity, fireControl);

            var audioConfData = SystemAPI.GetSingleton<AudioConfigData>();
            audioConfData.IsPlayingCIWSFiringSoundFX = true;
            SystemAPI.SetSingleton(audioConfData);
        }

        void OnStopFiringCIWS(InputAction.CallbackContext context)
        {
            if (!SystemAPI.HasSingleton<PlayerTag>())
                return;

            var mouseInput = SystemAPI.GetSingleton<InputMouseClick>();
            mouseInput.IsRightClick = false;
            SystemAPI.SetSingleton(mouseInput);

            var playerentity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var fireControl = SystemAPI.GetComponent<CIWSPlayerFireCommand>(playerentity);
            fireControl.IsFireCommand = false;
            fireControl.ROFTimerInSeconds = 0f;
            SystemAPI.SetComponent(playerentity, fireControl);

            var audioConfData = SystemAPI.GetSingleton<AudioConfigData>();
            audioConfData.IsPlayingCIWSFiringSoundFX = false;
            SystemAPI.SetSingleton(audioConfData);
        }

        void OnFireMissile(InputAction.CallbackContext context)
        {
            if (!SystemAPI.HasSingleton<PlayerTag>())
                return;

            if (!SystemAPI.HasSingleton<InputKeyPress>())
                return;

            var keyboardInput = SystemAPI.GetSingleton<InputKeyPress>();
            keyboardInput.IsSelectMissile = true;
            SystemAPI.SetSingleton(keyboardInput);

            var playerentity = SystemAPI.GetSingletonEntity<PlayerTag>();
            SystemAPI.SetComponent(playerentity, new MissilePlayerLaunchCommand
            {
                CooldownTimerInSeconds = 0f,
                MagazineCapacityCounter = 5,
                IsLaunchCommand = true,
            });
        }

        void OnSelectDrone(InputAction.CallbackContext context)
        {
            if (!SystemAPI.HasSingleton<PlayerTag>())
                return;

            if (!SystemAPI.HasSingleton<InputKeyPress>())
                return;

            var keyboardInput = SystemAPI.GetSingleton<InputKeyPress>();
            keyboardInput.IsSelectDrone = true;
            SystemAPI.SetSingleton(keyboardInput);
        }

        void OnDeployChaff(InputAction.CallbackContext context)
        {
            if (!SystemAPI.HasSingleton<PlayerTag>())
                return;

            if (!SystemAPI.HasSingleton<InputKeyPress>())
                return;

            var keyboardInput = SystemAPI.GetSingleton<InputKeyPress>();
            keyboardInput.IsDeployChaff = true;
            SystemAPI.SetSingleton(keyboardInput);
        }

        void OnToggleMenu(InputAction.CallbackContext context)
        {
            if (!SystemAPI.HasSingleton<PlayerTag>())
                return;

            if (!SystemAPI.HasSingleton<InputKeyPress>())
                return;

            var keyboardInput = SystemAPI.GetSingleton<InputKeyPress>();
            keyboardInput.IsToggleMenu = true;
            SystemAPI.SetSingleton(keyboardInput);
        }
    }
}
