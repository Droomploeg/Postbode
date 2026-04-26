using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.WebApp.Components.Controls.Security;
using Microsoft.Identity.Web;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class OverviewPage
{
    private const string NoMessageColor = "rgb(250, 250, 250)";
    private const string ScheduledColor = "rgb(200, 215, 0)";
    private const string ActiveColor = "rgb(62, 191, 0)";
    private const string DeadLetterColor = "rgb(255, 27, 27)";

    private readonly List<Queue> _queues = [];
    private readonly List<Topic> _topics = [];

    private AuthorizationState _authorizationState = AuthorizationState.Loading;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _queues.Clear();
            _topics.Clear();

            try
            {
                var entities = await RuntimeInfoService.GetAllAsync<IEntity>();

                _queues.AddRange(entities.OfType<Queue>());
                _topics.AddRange(entities.OfType<Topic>());
                _authorizationState = AuthorizationState.Authorized;
            }
            catch (UnauthorizedAccessException)
            {
                _authorizationState = AuthorizationState.Unauthorized;
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _authorizationState = AuthorizationState.TokenExpired;
            }

            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private long QueueCount => _queues.Count;
    private long QueueTotalMessages => QueueActiveMessageCount + QueueScheduledMessageCount + QueueDeadLetterMessageCount;
    private long QueueActiveMessageCount => _queues.Sum(q => q.RuntimeInfo.ActiveMessageCount);
    private long QueueDeadLetterMessageCount => _queues.Sum(q => q.RuntimeInfo.DeadLetterMessageCount);
    private long QueueScheduledMessageCount => _queues.Sum(q => q.RuntimeInfo.ScheduleMessageCount);

    private float QueueActiveMessagePercentages => QueueTotalMessages > 0f ? QueueActiveMessageCount / (float)QueueTotalMessages * 100f : 0f;
    private float QueueDeadLetterMessagePercentages => QueueTotalMessages > 0f ? QueueDeadLetterMessageCount / (float)QueueTotalMessages * 100f : 0f;
    private float QueueScheduledMessagePercentages => QueueTotalMessages > 0f ? QueueScheduledMessageCount / (float)QueueTotalMessages * 100f : 0f;

    private long SubscriptionCount => _topics.Sum(t => t.Subscriptions.Length);
    private long SubscriptionTotalMessages => SubscriptionActiveMessageCount + SubscriptionScheduledMessageCount + SubscriptionDeadLetterMessageCount;
    private long SubscriptionActiveMessageCount => _topics.Sum(t => t.Subscriptions.Sum(s => s.RuntimeInfo.ActiveMessageCount));
    private long SubscriptionDeadLetterMessageCount => _topics.Sum(t => t.Subscriptions.Sum(s => s.RuntimeInfo.DeadLetterMessageCount));
    private long SubscriptionScheduledMessageCount => _topics.Sum(t => t.Subscriptions.Sum(s => s.RuntimeInfo.ScheduleMessageCount));

    private float SubscriptionActiveMessagePercentages => SubscriptionTotalMessages > 0f ? SubscriptionActiveMessageCount / (float)SubscriptionTotalMessages * 100f : 0f;
    private float SubscriptionDeadLetterMessagePercentages => SubscriptionTotalMessages > 0f ? SubscriptionDeadLetterMessageCount / (float)SubscriptionTotalMessages * 100f : 0f;
    private float SubscriptionScheduledMessagePercentages => SubscriptionTotalMessages > 0f ? SubscriptionScheduledMessageCount / (float)SubscriptionTotalMessages * 100f : 0f;

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
            color = DeadLetterColor;
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
            color = DeadLetterColor;
        }

        result += $"{color} {begin}% 100%";
        return result;
    }


}
