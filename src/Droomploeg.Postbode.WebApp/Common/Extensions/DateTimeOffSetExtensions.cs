namespace Droomploeg.Postbode.WebApp.Common.Extensions;

public static class DateTimeOffSetExtensions
{
    public static string GetTimeAgo(this DateTimeOffset dateTime)
    {
        var timeSpan = DateTimeOffset.UtcNow - dateTime;

        if (timeSpan.TotalSeconds < 60)
            return $"{timeSpan.Seconds} seconds ago";
        else if (timeSpan.TotalMinutes < 60)
            return $"{timeSpan.Minutes} minutes ago";
        else if (timeSpan.TotalHours < 24)
            return $"{timeSpan.Hours} hours ago";
        else if (timeSpan.TotalDays < 30)
            return $"{timeSpan.Days} days ago";
        else if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} months ago";
        else
            return $"{(int)(timeSpan.TotalDays / 365)} years ago";
    }
}
