using System;
using System.Diagnostics;
using DirectumLogConverter.Properties;

namespace DirectumLogConverter
{
  /// <summary>
  /// Точка входа.
  /// </summary>
  class Program
  {
    /// <summary>
    /// Стандартная точка входа в приложение.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    /// <returns>Код, с которым завершилась работа приложения.</returns>
    public static int Main(string[] args)
    {
      var options = ConvertOptions.GetFromArgs(args);
      var stopwatch = new Stopwatch();
      try
      {
        stopwatch.Start();
        if (options.BatchConvert)
          Converter.BatchConvert(options);
        else
          Converter.Convert(options);
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
  }
}
