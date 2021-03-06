<Query Kind="Program">
  <NuGetReference>Microsoft.ApplicationInsights</NuGetReference>
  <NuGetReference>Microsoft.AspNetCore.Mvc</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Logging.Console</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Logging.Debug</NuGetReference>
  <NuGetReference>xunit.assert</NuGetReference>
  <Namespace>Microsoft.ApplicationInsights</Namespace>
  <Namespace>Microsoft.AspNetCore.Http</Namespace>
  <Namespace>Microsoft.AspNetCore.Http.Internal</Namespace>
  <Namespace>Microsoft.AspNetCore.Mvc</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection</Namespace>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <Namespace>Microsoft.Extensions.Logging.Console</Namespace>
  <Namespace>Microsoft.Extensions.Logging.Debug</Namespace>
  <Namespace>Microsoft.Extensions.Primitives</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Xunit</Namespace>
  <Namespace>Microsoft.ApplicationInsights.Extensibility</Namespace>
  <Namespace>Microsoft.ApplicationInsights.Channel</Namespace>
  <DisableMyExtensions>true</DisableMyExtensions>
</Query>

void Main()
{
    // If no parameters provided, go to docs
    Assert.IsType<RedirectResult>(Run());
    Assert.Equal("https://github.com/kzu/azdo#task-groups", Run<RedirectResult>().Url);

    Assert.IsType<RedirectResult>(Run("kzu"));
    Assert.IsType<RedirectResult>(Run("kzu", "oss"));

    Assert.Equal("https://dev.azure.com/kzu/oss/_taskgroups", Run<RedirectResult>("kzu", "oss").Url);
    Assert.Equal("https://dev.azure.com/DevDiv/DevDiv/_taskgroups", Run<RedirectResult>("DevDiv").Url);
    Assert.Equal("https://dev.azure.com/DevDiv/DevDiv/_taskgroups", Run<RedirectResult>("DevDiv", "DevDiv").Url);
}

static TActionResult Run<TActionResult>(string org = null, string project = null)
    => (TActionResult)Run(org, project);

static IActionResult Run(string org = null, string project = null)
    => Run(
        new DefaultHttpRequest(new DefaultHttpContext()), 
        new ServiceCollection().AddLogging(builder => builder.AddConsole()).BuildServiceProvider().GetService<ILoggerFactory>().CreateLogger("Console"), 
        org, project);

public static IActionResult Run(HttpRequest req, ILogger log, string org = null, string project = null)
{
    if (req.Host.Value == "tasks.devdiv.io")
        org = project = "DevDiv";

    if (string.IsNullOrEmpty(org) && string.IsNullOrEmpty(project))
    {
        new TelemetryClient(TelemetryConfiguration.Active).TrackEvent(
            "docs", new Dictionary<string, string>
            {
                { "url", req.Host + req.Path + req.QueryString },
                { "redirect", "https://github.com/kzu/azdo#task-groups" },
                { "org", org },
                { "project", project },
            });

        return new RedirectResult("https://github.com/kzu/azdo#task-groups");
    }

    if (string.IsNullOrEmpty(project))
        project = org;

    new TelemetryClient(TelemetryConfiguration.Active).TrackEvent(
        "redirect", new Dictionary<string, string>
        {
            { "url", req.Host + req.Path + req.QueryString },
            { "redirect", $"https://dev.azure.com/{org}/{project}/_taskgroups" },
            { "org", org },
            { "project", project },
        });

    return new RedirectResult($"https://dev.azure.com/{org}/{project}/_taskgroups");
}