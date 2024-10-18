using Droomploeg.DreamOps.Core.Models;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class OverviewPage
{
    private const string NoMessageColor = "rgb(250, 250, 250)";
    private const string ScheduledColor = "rgb(200, 215, 0)";
    private const string ActiveColor = "rgb(62, 191, 0)";
    private const string DeadletterColor = "rgb(255, 27, 27)";

    private readonly List<Queue> _queues = [];
    private readonly List<Topic> _topics = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var entities = await ServiceBusService.GetAllAsync<IEntity>();

            _queues.Clear();
            _topics.Clear();

            _queues.AddRange(entities.OfType<Queue>());
            _topics.AddRange(entities.OfType<Topic>());

            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private long QueueCount => _queues.Count;
    private long QueueTotalMessages => QueueActiveMessageCount + QueueScheduledMessageCount + QueueDeadLetterMessageCount;
    private long QueueActiveMessageCount => _queues.Sum(q => q.RuntimeInfo.ActiveMessageCount);
    private long QueueDeadLetterMessageCount => _queues.Sum(q => q.RuntimeInfo.DeadLetterMessageCount);
    private long QueueScheduledMessageCount => _queues.Sum(q => q.RuntimeInfo.ScheduleMessageCount);

    private float QueueActiveMessagePercentages => QueueTotalMessages > 0f ? (float)QueueActiveMessageCount / (float)QueueTotalMessages * 100f : 0f;
    private float QueueDeadLetterMessagePercentages => QueueTotalMessages > 0f ? (float)QueueDeadLetterMessageCount / (float)QueueTotalMessages * 100f : 0f;
    private float QueueScheduledMessagePercentages => QueueTotalMessages > 0f ? (float)QueueScheduledMessageCount / (float)QueueTotalMessages * 100f : 0f;

    private long SubscriptionCount => _topics.Sum(t => t.Subscriptions.Length);

    private long SubscriptionTotalMessages => SubscriptionActiveMessageCount + SubscriptionScheduledMessageCount + SubscriptionDeadLetterMessageCount;
    private long SubscriptionActiveMessageCount => _topics.Sum(t => t.Subscriptions.Sum(s => s.RuntimeInfo.ActiveMessageCount));
    private long SubscriptionDeadLetterMessageCount => _topics.Sum(t => t.Subscriptions.Sum(s => s.RuntimeInfo.DeadLetterMessageCount));
    private long SubscriptionScheduledMessageCount => _topics.Sum(t => t.Subscriptions.Sum(s => s.RuntimeInfo.ScheduleMessageCount));

    private float SubscriptionActiveMessagePercentages => SubscriptionTotalMessages > 0f ? (float)SubscriptionActiveMessageCount / (float)SubscriptionTotalMessages * 100f : 0f;
    private float SubscriptionDeadLetterMessagePercentages => SubscriptionTotalMessages > 0f ? (float)SubscriptionDeadLetterMessageCount / (float)SubscriptionTotalMessages * 100f : 0f;
    private float SubscriptionScheduledMessagePercentages => SubscriptionTotalMessages > 0f ? (float)SubscriptionScheduledMessageCount / (float)SubscriptionTotalMessages * 100f : 0f;

    private string GetQueueGradient()
    {
        int begin = 0;
        int end = 100;
        var color = NoMessageColor;
        if (QueueTotalMessages == 0)
        {
            return $"{NoMessageColor} {begin}% {end}%";
        }

        var result = "";
        if (QueueScheduledMessagePercentages > 0.0f)
        {
            begin = (int)QueueScheduledMessagePercentages;
            color = ScheduledColor;
        }

        if (QueueActiveMessagePercentages > 0.0f)
        {
            end = (int)QueueActiveMessagePercentages;
            if (begin > 0)
            {
                result += $"{color} {begin}% {end}%,";
            }
            begin = end;
            color = ActiveColor;
        }

        if (QueueDeadLetterMessagePercentages > 0.0f)
        {
            end = (int)QueueDeadLetterMessagePercentages;
            if (begin > 0)
            {
                result += $"{color} {begin}% {end}%,";
            }
            begin = end;
            color = DeadletterColor;
        }

        result += $"{color} {begin}% 100%";
        return result;
    }

    private string GetSubscriptionGradient()
    {
        int begin = 0;
        int end = 100;
        var color = NoMessageColor;
        if (SubscriptionTotalMessages == 0)
        {
            return $"{NoMessageColor} {begin}% {end}%";
        }

        var result = "";
        if (SubscriptionScheduledMessagePercentages > 0.0f)
        {
            begin = (int)SubscriptionScheduledMessagePercentages;
            color = ScheduledColor; 
        }

        if (SubscriptionActiveMessagePercentages > 0.0f)
        {
            end = (int)SubscriptionActiveMessagePercentages;
            if (begin > 0)
            {
                result += $"{color} {begin}% {end}%,";
            }
            begin = end;
            color = ActiveColor;
        }

        if (SubscriptionDeadLetterMessagePercentages > 0.0f)
        {
            end = (int)SubscriptionDeadLetterMessagePercentages;
            if (begin > 0)
            {
                result += $"{color} {begin}% {end}%,";
            }
            begin = end;
            color = DeadletterColor;
        }

        result += $"{color} {begin}% 100%";
        return result;
    }


}
