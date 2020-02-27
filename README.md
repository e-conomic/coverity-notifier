# coverity-notifier

The functionality of this script is to alert on new issues in a coverity view.
But you are in charge off running this script after your normal coverity snapshot process.

It will report all of the issues in the view you give it - so filter the view in coverity to your desire amount of issues.

It will color the slack message attachment of security issues with shades of red.

## Required Environment Variables

|VariableName|Type|Example|
|---|---|---|
|COVERITY_URI|Uri|`http://coverity-server.com:8080/`|
|COVERITY_USERNAME|string|`ReadOnlyMachineUserName`|
|COVERITY_PASSWORD|string|`StrongPassword1234`|
|SLACK_WEBHOOK_URI|URI|`https://hooks.slack.com/services/XXXX/YYYY/zzzzz`|
|SLACK_CHANNEL|string|`#sig-coverity-alerts`|

## Required Commandline Arguments

|ArgumentName|Type|Example|Description|
|---|---|---|---|
|`-CoverityViewId`|int|`1234`|The numeric id of the Coverity View|
|`-CoverityProjectId`|string|`CoolProjectName`|

## Usage

```powershell
.\build.ps1 -CoverityViewId 1234 -CoverityProjectId CoolProjectName
```
