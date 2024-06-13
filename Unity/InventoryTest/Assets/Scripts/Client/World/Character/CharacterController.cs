using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer.Unity;
using MessagePipe;

public class CharacterController : IAsyncStartable, IDisposable
{
    public readonly ControllerState state;
    readonly MainCamera camera;
    readonly ControlMode controlMode;
    readonly IAsyncPublisher<MyCharacterDirectionChangedEvent> myCharacterdirectionChanged;
    readonly IAsyncSubscriber<OtherCharacterDirectionChangedEvent> otherCharacterDirectionChanged;
    readonly Guid accountId;
    readonly bool isMyController;
    public CharacterObject Character;


    float cameraRotateSensitivityX = 6.0f;
    float cameraRotateSensitivityY = 6.0f;
    float wheelSpeed = 0.1f;
    float zoomSpeed = 12.0f;
    float cameraDistance = 2.0f;
    float moveSpeed = 5.0f;

    Vector3 characterMoveDirection = Vector3.zero;
    IDisposable subscription;

    CancellationTokenSource lifeCts = new();

    public CharacterController(
        Guid accountId,
        bool isMyController,
        ControllerState state,
        ControlMode controlMode,
        IAsyncPublisher<MyCharacterDirectionChangedEvent> myCharacterdirectionChanged,
        IAsyncSubscriber<OtherCharacterDirectionChangedEvent> otherCharacterDirectionChanged,
        MainCamera camera)
    {
        this.accountId = accountId;
        this.isMyController = isMyController;
        this.state = state;
        this.camera = camera;
        this.controlMode = controlMode;
        this.myCharacterdirectionChanged = myCharacterdirectionChanged;
        this.otherCharacterDirectionChanged = otherCharacterDirectionChanged;
    }
    public async UniTask StartAsync(CancellationToken cancellation)
    {
        var bag = DisposableBag.CreateBuilder();
        otherCharacterDirectionChanged.Subscribe(async (e, ct) =>
        {
            if (accountId != e.accountId)
            {
                return;
            }
            Character.transform.position = e.currentPosition;
            characterMoveDirection = e.currentDirection;
            Character.animator.SetFloat("Speed", e.currentDirection.sqrMagnitude > 0 ? 1 : 0);
        }).AddTo(bag);
        subscription = bag.Build();

        Cursor.lockState = CursorLockMode.Locked;
        if (isMyController)
        {
            CameraRotate(lifeCts.Token).Forget();
            CameraZoom(lifeCts.Token).Forget();
            CharacterMoveInput(lifeCts.Token).Forget();
            CharacterInput(lifeCts.Token).Forget();
        }
        CharacterMove(lifeCts.Token).Forget();
    }

    public async UniTask LoadCharacter(Vector3 position, string characterCode = "", CancellationToken ct = default(CancellationToken))
    {
        try
        {
            var loadedCharacter = await Addressables.InstantiateAsync("Assets/3D Chest Object/unitychan.prefab").WithCancellation(ct);
            loadedCharacter.transform.position = position;
            if (isMyController)
            {
                loadedCharacter.tag = Tags.PlayerCharacter;
            }
            var characterComponent = loadedCharacter.GetComponent<CharacterObject>();
            this.Character = characterComponent;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }


    public async UniTask Jump()
    {
        Character.animator.SetBool("Jump", true);
        await UniTask.NextFrame();
        Character.animator.SetBool("Jump", false);
    }
    async UniTask CharacterInput(CancellationToken ct)
    {
        while (true)
        {
            await UniTask.NextFrame(ct);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                await Jump();
            }
        }
    }

    async UniTask CharacterMoveInput(CancellationToken ct)
    {
        await UniTask.WaitUntil(() => Character != null, cancellationToken: ct);
        while (true)
        {
            await UniTask.WaitForFixedUpdate(ct);
            if (controlMode.CurrentState != ControlMode.ControlState.Character)
            {
                await UniTask.WaitUntil(() => controlMode.CurrentState == ControlMode.ControlState.Character);
            }
            if (Character == null || camera == null)
            {
                continue;
            }
            Vector2 moveInput = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            var cameraForward = new Vector3(camera.CameraHolder.forward.x, 0, camera.CameraHolder.forward.z).normalized;
            var cameraRight = new Vector3(camera.CameraHolder.right.x, 0, camera.CameraHolder.right.z).normalized;
            var nextMoveDir = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

            if ((characterMoveDirection - nextMoveDir).sqrMagnitude > 0.001f)
            {
                myCharacterdirectionChanged.Publish(new MyCharacterDirectionChangedEvent()
                {
                    currentPosition = this.Character.transform.position,
                    currentDirection = nextMoveDir
                });
            }
            characterMoveDirection = nextMoveDir;
        }
    }

    async UniTask CharacterMove(CancellationToken ct)
    {
        await UniTask.WaitUntil(() => Character != null, cancellationToken: ct);
        while (true)
        {
            await UniTask.WaitForFixedUpdate(ct);
            if (controlMode.CurrentState != ControlMode.ControlState.Character)
            {
                Character.animator.speed = 0;
                await UniTask.WaitUntil(() => controlMode.CurrentState == ControlMode.ControlState.Character);
                Character.animator.speed = 1;
            }
            if (Character == null)
            {
                continue;
            }
            var finalMoveVector = characterMoveDirection * Time.deltaTime * moveSpeed;

            Character.transform.position += finalMoveVector;
            if (finalMoveVector.sqrMagnitude > 0)
            {
                Character.transform.forward = characterMoveDirection;
            }
            Character.animator.SetFloat("Speed", finalMoveVector.sqrMagnitude > 0 ? 1 : 0);
        }
    }

    async UniTask CameraRotate(CancellationToken cancellationToken)
    {
        while (true)
        {
            await UniTask.NextFrame(cancellationToken);
            if (controlMode.CurrentState != ControlMode.ControlState.Character)
            {
                await UniTask.WaitUntil(() => controlMode.CurrentState == ControlMode.ControlState.Character);
            }
            if (camera == null)
            {
                continue;
            }

            var cameraAngle = camera.CameraHolder.rotation.eulerAngles;

            var xRotate = cameraAngle.x - (Input.GetAxis("Mouse Y") * this.cameraRotateSensitivityX);
            var yRotate = cameraAngle.y + (Input.GetAxis("Mouse X") * this.cameraRotateSensitivityY);

            if (xRotate > 180)
            {
                xRotate = Mathf.Clamp(xRotate, 315.0f, 361.0f);
            }
            else
            {
                xRotate = Mathf.Clamp(xRotate, -1.0f, 45.0f);
            }

            camera.CameraHolder.rotation = Quaternion.Euler(xRotate, yRotate, cameraAngle.z);
        }
    }

    async UniTask CameraZoom(CancellationToken cancellationToken)
    {
        RaycastHit hit;
        int raycastlayer = ~(1 << LayerMask.NameToLayer("Character"));

        while (true)
        {
            await UniTask.NextFrame(cancellationToken);
            if (controlMode.CurrentState != ControlMode.ControlState.Character)
            {
                await UniTask.WaitUntil(() => controlMode.CurrentState == ControlMode.ControlState.Character);
            }
            if (camera == null)
            {
                continue;
            }

            var mouseDelta = Input.mouseScrollDelta.y;

            this.cameraDistance -= wheelSpeed * mouseDelta;
            this.cameraDistance = Mathf.Clamp(this.cameraDistance, 1.5f, 4.0f);
            //시간에따라 보간
            float lerpZ = Mathf.Lerp(camera.Transform.localPosition.z, -this.cameraDistance, Mathf.Clamp01(zoomSpeed * Time.fixedDeltaTime));

            var isHit = Physics.Raycast(camera.CameraHolder.position, camera.CameraHolder.transform.TransformDirection(Vector3.back), out hit, this.cameraDistance, raycastlayer);

            Vector3 currentVector = camera.Transform.localPosition;
            currentVector.z = lerpZ;
            //지형지물에 부딪혔다면 길이감소
            if (isHit)
            {
                currentVector.z = -hit.distance;
            }

            currentVector.z += 0.01f;
            camera.Transform.localPosition = currentVector;
        }
    }

    public void Dispose()
    {
        subscription?.Dispose();
        lifeCts?.Cancel();
        lifeCts?.Dispose();
        Addressables.Release(Character.gameObject);
    }
}
