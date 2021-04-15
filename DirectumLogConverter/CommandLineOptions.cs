using System;
using System.IO;
using CommandLine;

namespace LogConverter
{
  internal class CommandLineOptions
  {
    [Option('s', "source", Required = true, HelpText = "Source file.")]
    public string Source { get; set; }

    [Option('d', "destination", Required = true, HelpText = "Destination.")]
    public string Destination { get; set; }

    #region Методы

    public void ValidateSource()
    {
      if (!File.Exists(this.Source)) 
      {
        throw new FileNotFoundException("File is not exist.");
      }
    }

    public void ValidateOrCreateDestination()
    {
      if (Directory.Exists(this.Destination))
      {
        throw new ArgumentException("Destination must be a file.");
      }
      if (!File.Exists(this.Destination))
      {
        try
        {
          File.Create(this.Destination).Dispose();
        }
        catch
        {
          throw new Exception("Cannot create file in this path.");
        }
      }
    }

    #endregion

  }

  /// <summary>
  /// Исключение о недопустимых параметрах командной строки.
  /// </summary>
  public sealed class InvalidCommandLineOptionsException : Exception
  {
  }
}