#r "NLog.dll"

using System.Net;
using System.Threading.Tasks;
using System.Configuration;
using NLog;
using NLog.Config;
using NLog.Targets;

[Target("AzureFunctionLog")]
public sealed class AzureFuctionLogTarget : TargetWithLayout
{
    public AzureFuctionLogTarget(TraceWriter azureLogTraceWriter)
    {
        AzureLogTraceWriter = azureLogTraceWriter;
    }
 
    [RequiredParameter]
    public TraceWriter AzureLogTraceWriter { get; set; }

    protected override void Write(LogEventInfo logEvent)
    {
        string logMessage = this.Layout.Render(logEvent);

        AzureLogTraceWriter.Info(logMessage);
    }
}

private static Logger HookNLogToAzureLog(TraceWriter log)
{
    // Configure NLog programmatically
    var config = new LoggingConfiguration();

    // Add the AzureFuctionLogTarget Target
    var azureTarget = new AzureFuctionLogTarget(log);
    config.AddTarget("azure", azureTarget);

    // Define the Layout
    azureTarget.Layout = @"${level:uppercase=true}|${threadid:padCharacter=0:padding=3}|${message}";

    // Create a rule so that all logging will be sent to AzureFuctionLogTarget
    var rule1 = new LoggingRule("*", LogLevel.Trace, azureTarget);
    config.LoggingRules.Add(rule1);

    // Assign the newly created configuration to the active LogManager NLog instance
    LogManager.Configuration = config;

    return LogManager.GetLogger("AzureFunction");
}

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    // Use the default logging infrastucture since NLog is not yet initialized
    log.Info("Received HTTP Request. Processing...");
 
    // Initialize NLog    
    Logger _nlog = HookNLogToAzureLog(log);

    // Now use NLog
    _nlog.Info("Hello from NLog");

    return req.CreateResponse(HttpStatusCode.OK);
}