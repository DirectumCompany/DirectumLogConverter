using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectumLogConverter.Properties;
using Newtonsoft.Json.Linq;

namespace DirectumLogConverter
{
  /// <summary>
  /// Класс, который конвертирует из одного формата в другой.
  /// </summary>
  internal static class Converter
  {

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

    #region Поля и свойства

    /// <summary>
    /// Количество потоков, которое будет использовано для конвертации.
    /// </summary>
    private static readonly int threadsCount = Environment.ProcessorCount * 2;

    /// <summary>
    /// Пул задач конвертации.
    /// </summary>
    private static readonly Task<string>[] taskPool = new Task<string>[threadsCount];

    /// <summary>
    /// Буфер строк лога.
    /// </summary>
    private static readonly string[] logLineBuffer = new string[threadsCount];

    #endregion

    #region Методы

    /// <summary>
    /// Конвертировать один файл.
    /// </summary>
    /// <param name="options">Опции конвертации.</param>
    internal static void Convert(ConvertOptions options)
    {
      if (options.InputPath == null)
      {
        Console.WriteLine("Enter file name");
        options.InputPath = Console.ReadLine();
        if (!File.Exists(options.InputPath))
        {
          Console.WriteLine($"File {options.InputPath} not found");
          Environment.Exit((int)ExitCode.Error);
        }
      }

      if (string.IsNullOrEmpty(options.OutputPath))
        options.OutputPath = GetConvertedFileName(options.InputPath, options.CsvFormat);

      if (!string.IsNullOrEmpty(options.FolderPath))
      {
        options.InputPath = Path.Combine(options.FolderPath, options.InputPath);
        options.OutputPath = Path.Combine(options.FolderPath, options.OutputPath);
      }

      if (File.Exists(options.OutputPath) && !GetUserConfirmation(string.Format(Resources.FileOverwriteConfirmation, options.OutputPath)))
        Environment.Exit((int)ExitCode.Success);

      ConvertJson(options);
    }

    /// <summary>
    /// Конвертировать из папки.
    /// </summary>
    /// <param name="options">Опции конвертации.</param>
    internal static void ConvertFromFolder(ConvertOptions options)
    {
      var files = Directory.GetFiles(options.FolderPath)
        .Where(name => !name.Contains(ConvertedFilenamePostfix))
        .ToArray<string>();

      foreach (var fileNames in GetFileNames(options, files))
      {
        options.InputPath = Path.Combine(options.FolderPath, Path.GetFileName(fileNames.Key));
        options.OutputPath = Path.Combine(options.FolderPath, Path.GetFileName(fileNames.Value));
        ConvertJson(options);
      }
    }

    /// <summary>
    /// Конвертировать JSON файл.
    /// </summary>
    /// <param name="options">Опции конвертации.</param>
    internal static void ConvertJson(ConvertOptions options)
    {
      Console.WriteLine(Resources.ConversionStarted, options.InputPath, options.OutputPath);

      IOutputLineFormatter formatter = options.CsvFormat ? new CsvLineFormatter() : new TsvLineFormatter();
      using var readerStream = new FileStream(options.InputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
      using var reader = new StreamReader(readerStream, Encoding.UTF8);
      using var writer = new StreamWriter(options.OutputPath, false, new UTF8Encoding(options.CsvFormat, true));

      var index = 0;

      while (true)
      {
        var line = reader.ReadLine();
        if (line == null)
          break;

        if (index == logLineBuffer.Length)
        {
          ProcessLogLines(index, formatter, writer);
          index = 0;
        }

        logLineBuffer[index] = line;
        index++;
      }

      if (index > 0)
        ProcessLogLines(index, formatter, writer);
    }

    /// <summary>
    /// Выполнить обработку строк лога.
    /// </summary>
    /// <param name="count">Количество строк лога, которые нужно обработать.</param>
    /// <param name="formatter">Форматтер строки лога.</param>
    /// <param name="writer">Объект для записи результатов конвертации.</param>
    private static void ProcessLogLines(int count, IOutputLineFormatter formatter, TextWriter writer)
    {
      for (int i = 0; i < count; i++)
      {
        taskPool[i] = new Task<string>(line => ConvertLogLine((string)line, formatter), logLineBuffer[i]);
        taskPool[i].Start();
      }

      Task.WaitAll(count < threadsCount ? taskPool.Take(count).ToArray() : taskPool);

      for (int i = 0; i < count; i++)
      {
        var value = taskPool[i].Result;

        if (string.IsNullOrEmpty(value))
          continue;

        writer.Write(value);
        writer.Write('\n');
      }
    }

    /// <summary>
    /// Конвертировать строку лога.
    /// </summary>
    /// <param name="line">Строка лога.</param>
    /// <param name="formatter">Форматтер строки лога.</param>
    /// <returns>Конвертированная строка лога.</returns>
    private static string ConvertLogLine(string line, IOutputLineFormatter formatter)
    {
      try
      {
        var logLineElements = new Dictionary<string, string>() { { "t", "" }, { "pid", "" }, { "l" , "" }, {"tr", "" } };
        var jsonDict = GetJsonValues(line);

        foreach (var jsonPair in jsonDict)
        {
          string value;
          switch (jsonPair.Key)
          {
            case "st":
            case "ex":
              value = ConvertException(jsonPair.Value);
              break;
            case "args":
              value = ConvertArguments(jsonPair.Value);
              break;
            case "cust":
              value = ConvertCustomProperties(jsonPair.Value);
              break;
            case "span":
              value = ConvertSpan(jsonPair.Value);
              break;
            default:
              value = Convert(jsonPair.Value);
              break;
          }

          if (!string.IsNullOrEmpty(value))
            logLineElements[jsonPair.Key] = value;
        }

        return formatter.Format(logLineElements);
      }
      catch (Exception)
      {
        return formatter.Format(new Dictionary<string, string> { { string.Empty, line } });
      }
    }

    /// <summary>
    /// Получить словарь ключей и значений верхнего уровня из json.
    /// </summary>
    /// <param name="json">Строка-json.</param>
    /// <returns>Словарь значений.</returns>
    private static IDictionary<string, IJEnumerable<JToken>> GetJsonValues(string json)
    {
      return JObject.Parse(json).Properties().ToDictionary(kv => kv.Name, kv => kv.Values());
    }

    /// <summary>
    /// Конвертация свойства в строку.
    /// </summary>
    /// <param name="jTokens">Набор токенов.</param>
    /// <param name="prefix">Префикс строки.</param>
    /// <param name="postfix">Постфикс строки.</param>
    /// <returns>Свойство в виде строки.</returns>
    private static string Convert(IEnumerable<JToken> jTokens, string prefix = null, string postfix = null)
    {
      var result = new StringBuilder();
      if (!string.IsNullOrEmpty(prefix))
        result.Append(prefix);
      result.AppendJoin(", ", jTokens.Select(jt => jt.ToString().Replace("\n", string.Empty).Replace("\r", string.Empty)));
      if (!string.IsNullOrEmpty(postfix))
        result.Append(postfix);
      return result.ToString();
    }

    /// <summary>
    /// Конвертировать аргументы.
    /// </summary>
    /// <param name="jTokens">Набор токенов.</param>
    /// <returns>Аргумент в виде строки.</returns>
    private static string ConvertArguments(IEnumerable<JToken> jTokens)
    {
      return Convert(jTokens, "(", ")");
    }

    /// <summary>
    /// Конвертировать свойства.
    /// </summary>
    /// <param name="jTokens">Набор токенов.</param>
    /// <returns>Свойства в виде строки.</returns>
    private static string ConvertCustomProperties(IEnumerable<JToken> jTokens)
    {
      return Convert(jTokens, "[", "]");
    }

    /// <summary>
    /// Конвертировать спан.
    /// </summary>
    /// <param name="jTokens">Набор токенов.</param>
    /// <returns></returns>
    private static string ConvertSpan(IEnumerable<JToken> jTokens)
    {
      return Convert(jTokens, "Span(", ")");
    }

    /// <summary>
    /// Конвертировать исключение.
    /// </summary>
    /// <param name="jTokens">Набор токенов.</param>
    /// <returns>Отформатированное исключение в виде строки.</returns>
    private static string ConvertException(IJEnumerable<JToken> jTokens)
    {
      var result = new StringBuilder("\n");
      var type = jTokens.OfType<JProperty>().FirstOrDefault(property => property.Name == "type")?.Value.ToString();
      var message = jTokens.OfType<JProperty>().FirstOrDefault(property => property.Name == "m")?.Value.ToString();
      var stack = jTokens.OfType<JProperty>().FirstOrDefault(property => property.Name == "stack")?.Value.ToString();

      if (!string.IsNullOrEmpty(type))
        result.Append(type);
      else
        result.AppendJoin('\n', jTokens.Select(jt => jt.ToString()));

      if (!string.IsNullOrEmpty(message))
      {
        result.Append(": ");
        result.Append(message);
      }

      if (!string.IsNullOrEmpty(stack))
      {
        result.Append("\n   ");
        result.Append(stack.Replace("\r\n", "\n"));
      }

      return result.ToString();
    }

    /// <summary>
    /// Сконвертировать имя файла.
    /// </summary>
    /// <param name="name">Имя файла.</param>
    /// <param name="isCsvFormat">Конвертировать в формат CSV?</param>
    /// <returns>Сконвертированное имя.</returns>
    private static string GetConvertedFileName(string name, bool isCsvFormat)
    {
      var extension = Path.GetExtension(name);
      var newExtension = isCsvFormat ? CsvFilenameExtension : extension;
      return name.Substring(0, name.Length - extension?.Length ?? 0) + ConvertedFilenamePostfix + newExtension;
    }

    /// <summary>
    /// Получить подтверждение пользователя на действие.
    /// </summary>
    /// <param name="message">Сообщение о .</param>
    /// <returns>Подтверждение.</returns>
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
    /// Получить пару значений имя/конвертированное имя файлов.
    /// </summary>
    /// <param name="options">Опции конвертации.</param>
    /// <param name="files">Файлы для конвертации.</param>
    /// <returns>Список пар имен файлов.</returns>
    private static List<KeyValuePair<string, string>> GetFileNames(ConvertOptions options, string[] files)
    {
      var fileNamesList = new List<KeyValuePair<string, string>>();
      foreach (var file in files)
        fileNamesList.Add(new KeyValuePair<string, string>(file, GetConvertedFileName(file, options.CsvFormat)));

      var isAnyConvertedFileAlreadyExists = fileNamesList.Any(fileNames => File.Exists(fileNames.Value));
      if (isAnyConvertedFileAlreadyExists && !GetUserConfirmation(Resources.MultipleFilesOverwriteConfirmation))
        Environment.Exit((int)ExitCode.Success);

      return fileNamesList;
    }

    #endregion
  }
}
