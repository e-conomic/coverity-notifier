public class BuildParameters
{
    public const string EnvironmentCoverityUsername = "COVERITY_USERNAME";
    public const string EnvironmentCoverityPassword = "COVERITY_PASSWORD";
    public const string EnvironmentCoverityUri = "COVERITY_URI";
    public const string EnvironmentSlackWebHookUri = "SLACK_WEBHOOK_URI";
    public const string EnvironmentSlackChannel = "SLACK_CHANNEL";

    public string Target { get; private set; }

    public string CoverityProjectId { get; private set; }
    public string CoverityViewId { get; private set; }
    public string CoverityUsername { get; private set; }
    public string CoverityPassword { get; private set; }
    public Uri CoverityUri { get; private set; }
    public Uri SlackWebHookUri { get; private set; }
    public string SlackChannel { get; private set; }

    public CoverityIssues CoverityIssues { get; private set; }

    public void SetIssues(CoverityIssues issues)
    {
        CoverityIssues = issues;
    }

    public void PrintParameters(ICakeContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.Information("Printing Build Parameters...");
        context.Information("Target: {0}", Target);

        context.Information("CoverityProjectId: {0}", CoverityProjectId);
        context.Information("CoverityViewId: {0}", CoverityViewId);
        context.Information("CoverityUri: {0}", CoverityUri);
        context.Information("SlackWebHookUri: {0}", SlackWebHookUri);
        context.Information("SlackChannel: {0}", SlackChannel);
        context.Information("Coverity Username is set: {0}", !string.IsNullOrEmpty(CoverityUsername));
        context.Information("Coverity Password is set: {0}", !string.IsNullOrEmpty(CoverityPassword));
    }

    public BuildParameters(ICakeContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        Target = context.Argument("target", "Default");
        CoverityProjectId = context.Argument("coverityprojectid", "");
        CoverityViewId = context.Argument("coverityviewid", "Outstanding Issues");
        CoverityUsername = context.EnvironmentVariable(EnvironmentCoverityUsername);
        CoverityPassword = context.EnvironmentVariable(EnvironmentCoverityPassword);

        if(string.IsNullOrEmpty(CoverityViewId))
        {
            throw new ArgumentNullException(nameof(CoverityViewId), "Coverity view id is empty or not set - please set it with the commandline argument");
        }

        if(string.IsNullOrEmpty(CoverityProjectId))
        {
            throw new ArgumentNullException(nameof(CoverityProjectId), "Coverity project id is empty or not set - please set it with the commandline argument");
        }

        if(string.IsNullOrEmpty(CoverityUsername) || string.IsNullOrEmpty(CoverityPassword))
        {
            throw new Exception($"Coverity Username or Password is not set. Please set {EnvironmentCoverityUsername} and {EnvironmentCoverityPassword} environment variables");
        }

        var coverityUriString  = context.EnvironmentVariable(EnvironmentCoverityUri);
        if(string.IsNullOrEmpty(coverityUriString))
        {
            // new Uri() would throw if we don't
            throw new ArgumentNullException(nameof(CoverityUri), $"{nameof(CoverityUri)} can't be empty or null, please set environment variable {EnvironmentCoverityUri}");
        }

        CoverityUri = new Uri(coverityUriString);

        SlackChannel = context.EnvironmentVariable(EnvironmentSlackChannel);
        if(string.IsNullOrEmpty(SlackChannel))
        {
            throw new ArgumentNullException(nameof(SlackChannel), $"{nameof(SlackChannel)} can't be empty or null, please set environment variable {EnvironmentSlackChannel}");
        }

        var slackWebHookUriString = context.EnvironmentVariable(EnvironmentSlackWebHookUri);
        if(string.IsNullOrEmpty(slackWebHookUriString))
        {
            // new Uri() would throw if we don't
            throw new ArgumentNullException(nameof(SlackWebHookUri), $"{nameof(SlackWebHookUri)} can't be empty or null, please set environment variable {EnvironmentSlackWebHookUri}");
        }

        SlackWebHookUri = new Uri(slackWebHookUriString);
    }
}