

using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Collections.Generic;
using System;
using System.Threading;
using VContainer.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RewardListPresenter : IAsyncStartable, IDisposable
{
    readonly RewardUIList awardList;
    readonly IAsyncSubscriber<GetRewardsEvent> subscriber;
    Queue<IEnumerable<Item>> awardQueue = new();
    IDisposable subscriptions;
    CancellationTokenSource lifeCts = new();
    public RewardListPresenter(
        RewardUIList awards,
        IAsyncSubscriber<GetRewardsEvent> subscriber)
    {
        this.awardList = awards;
        this.subscriber = subscriber;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        ProcessQueue().Forget();
        var bag = DisposableBag.CreateBuilder();
        subscriber.Subscribe(async (e, ct) =>
        {
            awardQueue.Enqueue(e.items);
        }).AddTo(bag);
        subscriptions = bag.Build();
    }

    async UniTask ProcessQueue()
    {
        while (true)
        {
            await UniTask.NextFrame(lifeCts.Token);
            if (awardQueue.Count == 0)
            {
                continue;
            }

            //add elements
            var rewards = awardQueue.Dequeue();
            foreach (var reward in rewards)
            {
                var child = awardList.NewChild();
                var itemSprite = await SpriteLoadUtil.LoadItemSpriteAsync(reward.BaseItemCode);
                child.OnDispose += () => Addressables.Release(itemSprite);
                child.ItemSprite = itemSprite;
                child.ItemText = $"{reward.Name} x {reward.Count}";
            }

            //elements Animation
            List<UniTask> tasks = new();
            foreach (var child in awardList.children)
            {
                tasks.Add(child.EnableAnimation(2.0f));
                child.gameObject.SetActive(true);
                await UniTask.WaitForSeconds(0.1f);
            }

            await UniTask.WhenAll(tasks);
            await UniTask.WaitForSeconds(1);
            awardList.Clear();
        }
    }

    public void Dispose()
    {
        if (!lifeCts.IsCancellationRequested)
        {
            lifeCts.Cancel();
        }
        lifeCts.Dispose();
        subscriptions?.Dispose();
    }
}