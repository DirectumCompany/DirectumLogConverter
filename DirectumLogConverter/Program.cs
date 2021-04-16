using CommandLine;
using System;

namespace LogConverter
{
  /// <summary>
  /// Точка входа.
  /// </summary>
  class Program
  {
    #region Вложенные типы

    /// <summary>
    /// Статусы, с которыми может завершаться программа.
    /// </summary>
    private enum ExitStatus
    {
      /// <summary>
      /// Статус успешного завершения программы.
      /// </summary>
      Success = 0,

      /// <summary>
      /// Статус завершения программы с ошибкой.
      /// </summary>
      Error = -1
    }

    #endregion

    #region

    /// <summary>
    /// Параметры командной строки.
    /// </summary>
    private static CommandLineOptions commandLineOptions;

    #endregion

    #region Методы

    /// <summary>
    /// Стандартная точка входа в приложение.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    /// <returns>Код, с которым завершилась работа приложения.</returns>
    public static int Main(string[] args)
    {
      var startTime = DateTime.Now;
      var exitStatus = ExitStatus.Success;
      try
      {
        ProcessCommandLineParameters(args);
        Converter.ConvertJsonToTsv(commandLineOptions.Source, commandLineOptions.Destination);
      }
      catch (InvalidCommandLineOptionsException)
      {
        exitStatus = ExitStatus.Error;
        Console.WriteLine("Command line parameters is invalid, please retry.");
      }
      catch (Exception ex)
      {
        exitStatus = ExitStatus.Error;
        Console.WriteLine(ex.ToString());
      }

      var executionTime = DateTime.Now - startTime;
      if (exitStatus == ExitStatus.Success)
      {
        Console.WriteLine($"The conversion was successful in {executionTime}!");
      }
      if (exitStatus == ExitStatus.Error)
      {
        Console.WriteLine($"The conversion was failed in {executionTime}!");
        // TODO: добавить очистку созданных файлов и папок в случае неудачи.
      }

      return (int)exitStatus;
    }

    /// <summary>
    /// Обработать аргументы командной строки.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    private static void ProcessCommandLineParameters(string[] args)
    {
      void SetupParser(ParserSettings settings)
      {
        settings.AutoVersion = false;
        settings.EnableDashDash = true;
        settings.HelpWriter = null;
      }

      using (var commandLineParser = new Parser(SetupParser))
      {
        var parserResult = commandLineParser.ParseArguments<CommandLineOptions>(args);
        parserResult
          .WithParsed(options => commandLineOptions = options)
          .WithNotParsed(errors => throw new InvalidCommandLineOptionsException());
      }

      commandLineOptions.ValidateSource();
      commandLineOptions.ValidateOrCreateDestination();
    }

    #endregion
  }
}
