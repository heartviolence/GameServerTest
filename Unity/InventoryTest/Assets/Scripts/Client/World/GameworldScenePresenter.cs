
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using static ControlMode;

public class GameworldScenePresenter : IAsyncStartable, IDisposable
{
    readonly GameworldUIPresenter uIPresenter;
    readonly AchievementPresenter achievementPresenter;
    readonly DropItemPresenter dropItemPresenter;
    readonly ControlMode controlMode;

    CancellationTokenSource lifeCts = new();

    public GameworldScenePresenter(
        GameworldUIPresenter uIPresenter,
        AchievementPresenter achievementPresenter,
        DropItemPresenter dropItemPresenter,
        ControlMode controlMode)
    {
        this.uIPresenter = uIPresenter;
        this.achievementPresenter = achievementPresenter;
        this.dropItemPresenter = dropItemPresenter;
        this.controlMode = controlMode;
    }

    public void Dispose()
    {
        lifeCts.Cancel();
        lifeCts.Dispose();
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        await UniTask.WhenAll(
            uIPresenter.StartAsync(cancellation),
            achievementPresenter.StartAsync(cancellation),
            dropItemPresenter.StartAsync(cancellation));
        CursorLockStateUpdate(lifeCts.Token).Forget();
    }

    async UniTask CursorLockStateUpdate(CancellationToken cancellation)
    {
        while (true)
        {
            await UniTask.NextFrame(cancellation);
            if (controlMode.CurrentState == ControlState.Character)
            {
                if (Input.GetKeyDown(KeyCode.LeftAlt))
                {
                    Cursor.lockState = CursorLockMode.Confined;
                }

                if (Input.GetKeyUp(KeyCode.LeftAlt))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }
}