using CoreEngine;
using CoreEngine.Actor;
using CoreEngine.CameraSystem;
using CoreEngine.EventBus;
using CoreEngine.Pool;
using CoreEngine.Network.FishNetExtension;
using FishNet.Object.Synchronizing;
using Icarus.Camera;
using Icarus.Controller;
using System;
using UnityEngine;
using Icarus.Character.State;
using System.Collections;

namespace Icarus.Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class SharedActor : BaseNetworkActor, IActorHost, IPoolable, IFixedTickable
    {
        public FixedTickGroup FixedTickGroup => FixedTickGroup.Physics;
        protected override NetworkTickTarget networkTickTarget => NetworkTickTarget.ServerOnly;

        private readonly SyncDictionary<int, Vector2> _clientInputs = new SyncDictionary<int, Vector2>();
        public SyncDictionary<int, Vector2> ClientInputs => _clientInputs;

        public IPoolReleaser Releaser { get; set; }

        [Header("🪽 부품(Features) 조립")]
        [SerializeField] private MovementFeature _movementFeature = new();
        [SerializeField] private StateControlFeature _stateControlFeature = new();
        [SerializeField] private AnimationFeature _animationFeature = new();

        // 캐릭터가 도중에 바뀌든 카메라가 도중에 바뀌든 세팅하기 편하도록
        private RepeatEventProvider<SetCameraTargetEvent> _cameraTargetProvider;
        private RepeatEventProvider<SwitchCameraEvent> _cameraSwitchProvider;


        private CameraNetTarget _cameraTarget;
        private Rigidbody _rb;
        private bool _isSpawned;
        private bool _isFeatureInit = false;

        public override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody>();

            _cameraTarget = GetComponentInChildren<CameraNetTarget>();
            if (_cameraTarget != null)
            {
                Func<SetCameraTargetEvent> cameraTargetEventFunc = () => new SetCameraTargetEvent(_cameraTarget, typeof(ThirdPersonCameraController));
                _cameraTargetProvider = new RepeatEventProvider<SetCameraTargetEvent>(cameraTargetEventFunc);

                Func<SwitchCameraEvent> cameraSwitchEventFunc = () => new SwitchCameraEvent(typeof(ThirdPersonCameraController));
                _cameraSwitchProvider = new RepeatEventProvider<SwitchCameraEvent>(cameraSwitchEventFunc);
            }
        }

        protected override void OnSafeSpawn()
        {

            if (_cameraTarget != null)
            {
                _cameraTarget.DecoupleAndFollow(transform);
                _cameraTargetProvider.Bind();
                _cameraSwitchProvider.Bind();
            }

            if (!_isFeatureInit)
            {
                _movementFeature.Initialize(this);
                _stateControlFeature.Initialize(this);
                _animationFeature.Initialize(this);
                _isFeatureInit = true;
            }

            _stateControlFeature.StartState();
            _isSpawned = true;
        }

        protected override void OnSafeDespawn()
        {
            _isSpawned = false;

            if (IsServerInitialized)
            {
                // Client가 탈출하면서 초기화 하는것 방지
                // 오로지 서버만 권한이 있음
                _clientInputs.Clear();
            }

            _stateControlFeature.StopState();

            if (_cameraTarget != null)
            {
                _cameraTarget.ReturnToParent(transform);
                _cameraTargetProvider.Unbind();
                _cameraSwitchProvider.Unbind();
            }

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            EventBus<SharedActorMoveEvent>.Subscribe(OnSharedActorMove);
            EventBus<SharedActorFlapEvent>.Subscribe(OnSharedActorWingFlap);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            EventBus<SharedActorMoveEvent>.Unsubscribe(OnSharedActorMove);
            EventBus<SharedActorFlapEvent>.Unsubscribe(OnSharedActorWingFlap);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsServerInitialized)
            {
                _rb.isKinematic = true;
            }
        }

        private void OnSharedActorMove(SharedActorMoveEvent evt) => _clientInputs[evt.ClientId] = evt.MoveVector;
        private void OnSharedActorWingFlap(SharedActorFlapEvent evt) => _movementFeature.ApplyFlap(evt.IsLeft);

        public void FixedTick(float fixedDeltaTime)
        {
            if (!this.IsServerInitialized || !_isSpawned) return;

            _stateControlFeature.FixedTick(fixedDeltaTime);
            _movementFeature.FixedTick(fixedDeltaTime);
        }

        public bool TryGetFeature<T>(out T feature) where T : class, IActorFeature
        {
            if (_movementFeature is T move) { feature = move; return true; }
            if (_stateControlFeature is T state) { feature = state; return true; }
            if (_animationFeature is T anim) { feature = anim; return true; }
            feature = null; return false;
        }
    }
}