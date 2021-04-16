using System;
using System.Diagnostics;
using System.IO;
using DirectumLogConverter.Properties;

namespace DirectumLogConverter
{
  /// <summary>
  /// Точка входа.
  /// </summary>
  class Program
  {
    #region Вложенные типы

    /// <summary>
    /// Коды, с которыми может завершаться программа.
    /// </summary>
    internal enum ExitCode
    {
      /// <summary>
      /// Код успешного завершения программы.
      /// </summary>
      Success = 0,

      /// <summary>
      /// Код завершения программы с ошибкой.
      /// </summary>
      Error = 1
    }

    #endregion

    #region Константы

    /// <summary>
    /// Постфикс имени сконвертированого файла.
    /// </summary>
    public const string ConvertedFilenamePostfix = "_converted";

    /// <summary>
    /// Расширение сконвертированого в csv файла.
    /// </summary>
    private const string CsvFilenameExtension = ".csv";

    #endregion

    #region Методы

    /// <summary>
    /// Получить подтверждение пользователя на действие.
    /// </summary>
    /// <param name="message">Сообщение о .</param>
    /// <returns></returns>
    private static bool GetUserConfirmation(string message)
    {
      while (true)
      {
        Console.Write(Resources.UserConfirmationTemplate, message);
        var response = Console.ReadLine()?.ToLowerInvariant();
        switch (response)
        {
          case "y" or "yes":
            return true;
          case "n" or "no":
            return false;
          default:
            Console.WriteLine(Resources.UnrecognizedInput, response);
            break;
        }
      }
    }

    /// <summary>
    /// Стандартная точка входа в приложение.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    /// <returns>Код, с которым завершилась работа приложения.</returns>
    public static int Main(string[] args)
    {
      var options = ConvertOptions.GetFromArgs(args);

      if (string.IsNullOrEmpty(options.OutputPath))
      {
        var extension = Path.GetExtension(options.InputPath);
        var newExtension = options.CsvFormat ? CsvFilenameExtension : extension;
        options.OutputPath = options.InputPath.Substring(0, options.InputPath.Length - extension?.Length ?? 0) + ConvertedFilenamePostfix + newExtension;
      }

      if (File.Exists(options.OutputPath) && !GetUserConfirmation(string.Format(Resources.FileOverwriteConfirmation, options.OutputPath)))
        Environment.Exit((int)ExitCode.Success);

      var stopwatch = new Stopwatch();
      try
      {
        Console.WriteLine(Resources.ConversionStarted, options.InputPath, options.OutputPath);
        stopwatch.Start();
        Converter.ConvertJson(options);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        Environment.Exit((int)ExitCode.Error);
      }
      finally
      {
        stopwatch.Stop();
      }

      Console.WriteLine(Resources.ConversionDone, stopwatch.Elapsed.TotalSeconds);
      return (int)ExitCode.Success;
    }

    #endregion
  }
}
