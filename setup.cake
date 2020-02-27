#addin nuget:?package=RestSharp&version=106.6.9.0
#addin nuget:?package=Cake.Rest&version=0.1.2
#addin nuget:?package=Cake.Slack&version=0.13.0
#load "parameters.cake"
#load "coverity.cake"
///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup<BuildParameters>(ctx =>
{
   return new BuildParameters(ctx);
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
.IsDependentOn("SendResultsToSlack");

Task("SendResultsToSlack")
.IsDependentOn("PrintParameters")
.IsDependentOn("FetchCoverityIssues")
.Does<BuildParameters>((ctx, buildParameters) => {
   var slackChatMessageAttachments = new List<SlackChatMessageAttachment>();
   var typeDictionary = new Dictionary<string, IList<SlackChatMessageAttachmentField>>();
   var typeToCategoryDictionary = new Dictionary<string, string>();

   var issues = buildParameters.CoverityIssues.viewContentsV1.rows.OrderBy(x => x.displayType);

   foreach(var issue in issues)
   {
      if(!typeDictionary.ContainsKey(issue.displayType))
      {
         typeDictionary.Add(issue.displayType, new List<SlackChatMessageAttachmentField>());
      }
      if(!typeToCategoryDictionary.ContainsKey(issue.displayType))
      {
         typeToCategoryDictionary.Add(issue.displayType, issue.displayCategory);
      }

      var list = typeDictionary[issue.displayType];

      Information(issue.displayType);

      var field = new SlackChatMessageAttachmentField
      {
         Value =  issue.displayFile
      };
      if(list.Count == 0){
         field.Title = issue.displayType;
      }
      list.Add(field);
   }

   var categoryColors = new Dictionary<string, string>
   {
      { "High impact security", "#F51616" },
      { "Medium impact security", "#F57316" },
      { "Low impact security", "#F5D016" },
      { "unknown", "#67A0E1" }
   };

   Func<string, string> typeToCategory = issueType => {
      string fallback = "unknown";
      try{
         return typeToCategoryDictionary[issueType] ?? fallback;
      }
      catch(KeyNotFoundException){
         return fallback;
      }
   };
   Func<string, string> categoryToColor = category => {
      string fallback = "#67A0E1";
      try{
         return categoryColors[typeToCategory(category)] ?? fallback;
      }
      catch(KeyNotFoundException){
         return fallback;
      }
   };
   Func<int, string> plural = count => (count > 0) ? "s" : "";

   foreach(var issuekvp in typeDictionary)
   {
      slackChatMessageAttachments.Add(new SlackChatMessageAttachment
      {
            Color = categoryToColor(issuekvp.Key),
            Fields = issuekvp.Value,
            Footer = $"{issuekvp.Value.Count} issue{plural(issuekvp.Value.Count)} of type: {issuekvp.Key}",
            Mrkdwn_In = new [] {"fields"}
      });
   }
   
   SlackChatMessageResult postMessageResult;
   if(slackChatMessageAttachments.Count > 0)
   {
      postMessageResult = Slack.Chat.PostMessage(
         channel:buildParameters.SlackChannel,
         text:$"Coverity issues in view id:{buildParameters.CoverityViewId}",
         messageAttachments: slackChatMessageAttachments,
         messageSettings:new SlackChatMessageSettings { IncomingWebHookUrl = buildParameters.SlackWebHookUri.ToString() }
      );
   }
   else
   {
      postMessageResult = Slack.Chat.PostMessage(
         channel:buildParameters.SlackChannel,
         text:$"No coverity issues in view id:{buildParameters.CoverityViewId}",
         messageSettings:new SlackChatMessageSettings { IncomingWebHookUrl = buildParameters.SlackWebHookUri.ToString() }
      );
   }

   if (postMessageResult.Ok)
   {
      Information("Message successfully sent");
   }
   else
   {
      Error("Failed to send message: {0}", postMessageResult.Error);
   }
});

Task("PrintParameters")
.Does<BuildParameters>((ctx, buildParameters) => {
   buildParameters.PrintParameters(ctx);
});

Task("FetchCoverityIssues")
.Does<BuildParameters>((ctx, buildParameters) => {
   var base64auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{buildParameters.CoverityUsername}:{buildParameters.CoverityPassword}"));
   var headers = new Dictionary<string, string>
   {
      {"User-Agent", $"{buildParameters.CoverityProjectId}/coverity-notifier"},
      {"Authorization", $"Basic {base64auth}"},
      {"Content-Type", "application/json"}
   };
   var client = RestUtilities.GetClientInstance(buildParameters.CoverityUri.ToString(), false, false);
   var requestUriBuilder = new UriBuilder(new Uri(buildParameters.CoverityUri, $"api/viewContents/issues/v1/{Uri.EscapeUriString(buildParameters.CoverityViewId)}"));
   requestUriBuilder.Query = $"projectId={buildParameters.CoverityProjectId}&rowCount=-1";

   var request = RestUtilities.GetRequest("GET", requestUriBuilder.Uri, headers);

   IRestResponse<CoverityIssues> issues = client.Get<CoverityIssues>(request);

   if(issues.Data == null)
   {
      Error(issues.Content);
      throw new Exception();
   }

   foreach(var issue in issues.Data.viewContentsV1.rows)
   {
      Information(issue.displayType);
   }

   buildParameters.SetIssues(issues.Data);
});

RunTarget(target);