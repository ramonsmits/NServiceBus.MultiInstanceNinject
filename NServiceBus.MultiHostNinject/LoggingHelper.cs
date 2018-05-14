using System;
using System.Reflection;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using NServiceBus;

static class LoggingHelper
{
    public static void InitLogging()
    {
        var layout = new PatternLayout
        {
            ConversionPattern = "%date [%-3thread] %-5level %logger [%ndc] - %message%newline"
        };
        layout.ActivateOptions();
        var consoleAppender = new ManagedColoredConsoleAppender
        {
            Threshold = Level.Debug,
            Layout = layout,

        };
        consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors
        {
            ForeColor = ConsoleColor.Red,
            BackColor = ConsoleColor.White,
            Level = Level.Fatal
        });
        consoleAppender.AddMapping(
            new ManagedColoredConsoleAppender.LevelColors {ForeColor = ConsoleColor.Red, Level = Level.Error});
        consoleAppender.AddMapping(
            new ManagedColoredConsoleAppender.LevelColors {ForeColor = ConsoleColor.Yellow, Level = Level.Warn});
        consoleAppender.AddMapping(
            new ManagedColoredConsoleAppender.LevelColors {ForeColor = ConsoleColor.Green, Level = Level.Info});
        consoleAppender.AddMapping(
            new ManagedColoredConsoleAppender.LevelColors {ForeColor = ConsoleColor.White, Level = Level.Debug});
        consoleAppender.AddMapping(
            new ManagedColoredConsoleAppender.LevelColors {ForeColor = ConsoleColor.Gray, Level = Level.Verbose});

        consoleAppender.ActivateOptions();

        var executingAssembly = Assembly.GetExecutingAssembly();
        var repository = log4net.LogManager.GetRepository(executingAssembly);
        BasicConfigurator.Configure(repository, consoleAppender);

        NServiceBus.Logging.LogManager.Use<Log4NetFactory>();
    }
}