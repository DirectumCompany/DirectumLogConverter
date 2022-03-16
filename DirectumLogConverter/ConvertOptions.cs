using System;
using CommandLine;
using DirectumLogConverter.Properties;

namespace DirectumLogConverter
{
  /// <summary>
  /// Опции конвертации.
  /// </summary>
  internal sealed class ConvertOptions
  {
    #region Поля и свойства

    [Value(0, MetaName = "Source")]
    public string InputPath { get; set; }

    [Value(1, MetaName = "Destination")]
    public string OutputPath { get; set; }

    [Option('c', "csv", Default = false)]
    public bool CsvFormat { get; set; }

		[Option('b', "batch", Default = false)]
		public bool BatchConvert { get; set; }

    [Option('f', "folder", Default = "")]
    public string FolderPath { get; set; }

    #endregion

    #region Методы

    /// <summary>
    /// Показать справку и выйти.
    /// </summary>
    private static void ShowUsageAndExit()
    {
      Console.WriteLine(Resources.Usage, AppDomain.CurrentDomain.FriendlyName, Converter.ConvertedFilenamePostfix);
      Environment.Exit((int)ExitCode.Success);
    }

    /// <summary>
    /// Получить опции конвертации из аргументов командной строки.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    /// <returns>Опции конвертации.</returns>
    public static ConvertOptions GetFromArgs(string[] args)
    {
      var parser = new Parser(settings =>
      {
        settings.AutoHelp = false;
        settings.AutoVersion = false;
        settings.CaseSensitive = false;
        settings.EnableDashDash = true;
      });

      var parsedArguments = parser.ParseArguments<ConvertOptions>(args);
      var result = default(ConvertOptions);
      parsedArguments.WithParsed(options => result = options)
        .WithNotParsed(errors =>
        {
          foreach (var error in errors)
          {
            if (error.Tag == ErrorType.UnknownOptionError)
            {
              Console.Error.WriteLine(Resources.ResourceManager.GetString(nameof(ErrorType.UnknownOptionError)), ((UnknownOptionError)error).Token);
              Environment.Exit((int)ExitCode.Error);
            }
          }

          ShowUsageAndExit();
        });
      return result;
    }

    #endregion
  }
}