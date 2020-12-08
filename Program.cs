using System;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace ConsoleApp1
{
    class Program
    {
        public static ActivitySource ActivitySource = new("foo");
        static void Main()
        {
            Console.WriteLine("Hello World!");

            var agentHost = Environment.GetEnvironmentVariable("JAEGER_AGENTHOST") ?? "localhost";
            var agentPort = int.Parse(Environment.GetEnvironmentVariable("JAEGER_AGENTPORT")?? "6831");

            var loopCount = int.Parse(Environment.GetEnvironmentVariable("DEMOCLIENT_LOOPCOUNT")?? "500");

            System.Console.WriteLine($"Using agenthost '{agentHost}:{agentPort}'");
            System.Console.WriteLine($"Running  '{loopCount}' loops");

            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource(ActivitySource.Name)
                .SetSampler(new AlwaysOnSampler())
                .AddJaegerExporter(o => {
                    o.AgentHost = agentHost;
                    o.AgentPort = agentPort;
                })
                .Build();

            Console.WriteLine("Starting loop");

            for (var i = 0; i < loopCount; i++)
            {
                using(var activity = ActivitySource.StartActivity(i.ToString()))
                {
                    activity.AddEvent(new ActivityEvent("Event"));
                    activity.AddTag("foo", "bar");

                    using(var childActivity = ActivitySource.StartActivity($"{i} child", ActivityKind.Internal))
                    {
                        childActivity.AddEvent(new ActivityEvent("Child Event"));
                        childActivity.AddTag("bar", "baz");

                        if(i % 50 == 0)
                        {
                            System.Threading.Thread.Sleep(50);
                        }
                    }
                }
            }

            Console.WriteLine("Loop done, wait 5 seconds because it seems not everything gets flushed");
            System.Threading.Thread.Sleep(5000);
        }
    }
}