
using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using MagicOnion;
using MessagePipe;
using System.Threading;
using VContainer.Unity;
using Shared.Data;
using Shared.Server.GameServer.Interface.Services;
using UnityEngine;
using System;

public class LoginScenePresenter : IAsyncStartable, IDisposable
{
    readonly LoginManager loginManager;
    readonly GameWorldManager gameWorldManager;
    readonly IAsyncSubscriber<LoginSuccessedEvent> loginSuccessedsub;
    readonly IAsyncSubscriber<LoginFailedEvent> loginFailedsub;
    readonly LoginForm loginForm;
    readonly RegisterForm registerForm;
    readonly PopupMessage popupMessage;
    readonly GameInitializeSettings initializeSettings;
    IDisposable subscription;
    public LoginScenePresenter(
        LoginManager loginManager,
        GameWorldManager gameWorldManager,
        LoginForm loginForm,
        RegisterForm registerFormController,
        IAsyncSubscriber<LoginSuccessedEvent> loginSuccessedsub,
        IAsyncSubscriber<LoginFailedEvent> loginFailedsub,
        PopupMessage popupMessage,
        GameInitializeSettings initializeSettings)
    {
        this.loginManager = loginManager;
        this.gameWorldManager = gameWorldManager;
        this.loginForm = loginForm;
        this.loginSuccessedsub = loginSuccessedsub;
        this.loginFailedsub = loginFailedsub;
        this.registerForm = registerFormController;
        this.popupMessage = popupMessage;
        this.initializeSettings = initializeSettings;
    }

    public void Dispose()
    {
        subscription?.Dispose();
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        try
        {
            await initializeSettings.InitializeAsync(cancellation);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.Log("초기화실패");
            return;
        }

        var bag = DisposableBag.CreateBuilder();

        loginSuccessedsub.Subscribe(async (e, ct) =>
        {
            await gameWorldManager.LoadMyWorldAsync();
        }).AddTo(bag);

        loginFailedsub.Subscribe(async (e, ct) =>
        {
            loginForm.LoginFailed();
        }).AddTo(bag);
        subscription = bag.Build();

        loginForm.RegisterButtonClicked += () =>
        {
            registerForm.Open();
            loginForm.Enable(false);
        };

        loginForm.LoginButtonClicked += () =>
        {
            Debug.Log("LoginButtonClicked");
            loginForm.LoginWaiting().Forget();
            loginManager.LoginAsync(loginForm.ID, loginForm.Password).Forget();
        };

        registerForm.OnBack += () =>
        {
            loginForm.Enable(true);
        };

        registerForm.RegisterButtonClicked += () =>
        {
            UniTask.Action(async () =>
            {
                var channel = GrpcChannelx.ForAddress(ServerAddresses.LoginAndMatchServer);
                var client = MagicOnionClient.Create<IAccountRegisterService>(channel);
                bool result = await client.RegisterAccount(registerForm.ID, registerForm.Password);
                Debug.Log($"RegisterResult : {result}");
                if (result)
                {
                    popupMessage.ShowAsync("회원가입 성공", 3.0f).Forget();
                    registerForm.Back();
                }
            }).Invoke();
        };
    }
}