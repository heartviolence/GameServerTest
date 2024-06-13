
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class DisableAnimation : MonoBehaviour
{
    public Vector2 StartPos = Vector2.zero;
    public Vector2 EndPos = Vector2.zero;

    public float Duration = 0.7f;

    CanvasGroup canvasGroup = null;
    CanvasGroup CanvasGroup
    {
        get
        {
            if (this.canvasGroup == null)
            {
                this.canvasGroup = GetComponent<CanvasGroup>();
            }
            return this.canvasGroup;
        }
    }

    RectTransform rectTransform = null;
    RectTransform RectTransform
    {
        get
        {
            if (this.rectTransform == null)
            {
                this.rectTransform = GetComponent<RectTransform>();
            }
            return this.rectTransform;
        }
    }

    public async UniTask StartAsync()
    {
        await Disable(this.GetCancellationTokenOnDestroy());
    }

    public async UniTask Disable(CancellationToken ct)
    {
        this.CanvasGroup.blocksRaycasts = false;
        DOTween.Kill(this.CanvasGroup);
        DOTween.Kill(this.RectTransform);
        DOTween.To(() => this.CanvasGroup.alpha, x => this.CanvasGroup.alpha = x, 0.0f, Duration)
            .From(1.0f)
            .SetTarget(this.CanvasGroup)
            .WithCancellation(ct)
            .Forget();

        await this.RectTransform.DOAnchorPos(this.EndPos, Duration)
            .From(StartPos)
            .WithCancellation(ct);
        this.gameObject.SetActive(false);
    }
}