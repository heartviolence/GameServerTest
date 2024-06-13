
using System.Net.NetworkInformation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LoginSceneLifeTimeScope : LifetimeScope
{
    [SerializeField]
    LoginForm loginForm;
    [SerializeField]
    RegisterForm registerForm;
    [SerializeField]
    PopupMessage popupMessage;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(loginForm);
        builder.RegisterComponent(registerForm);
        builder.RegisterComponent(popupMessage);
        builder.RegisterEntryPoint<LoginScenePresenter>();
    }
}