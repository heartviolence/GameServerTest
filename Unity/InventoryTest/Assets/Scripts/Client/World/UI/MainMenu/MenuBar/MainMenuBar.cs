
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuBar : MonoBehaviour
{
    public ClickableUI inventoryIcon;
    public ClickableUI MultiplayIcon;
    public ClickableUI ChangeNameIcon;
    public NameField nameChangeField;

    public EnableAnimation enableAnimation;
    public DisableAnimation disableAnimation;
    public async UniTask EnableAnimation()
    {
        await enableAnimation.StartAsync();
    }

    public async UniTask DisableAnimation()
    {
        await disableAnimation.StartAsync();
    }
}