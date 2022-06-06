using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DirectumLogConverter
{
  /// <summary>
  /// Класс, который конвертирует из одного формата в другой.
  /// </summary>
  internal static class Converter
  {
    #region Поля и свойства

    /// <summary>
    /// Опции конвертации.
    /// </summary>
    private static ConvertOptions CurrentOptions;

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
    /// Конвертировать JSON файл.
    /// </summary>
    /// <param name="options">Опции конвертации.</param>
    internal static void ConvertJson(ConvertOptions options)
    {
      CurrentOptions = options;
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
            case "mt":
              value = Convert(jsonPair.Value);
              if (CurrentOptions.NeedMergingArgumentsIntoMessageText && jsonDict.TryGetValue("args", out var argsJson))
                value = Converter.MergeArgumentsIntoMessage(value, argsJson);
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
    /// Раскрыть параметры сообщения в его текст.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    /// <param name="args">Параметры.</param>
    /// <returns>Строку с подставленными значениями аргументов в тексте сообщения.</returns>
    private static string MergeArgumentsIntoMessage(string message, IJEnumerable<JToken> args)
    {
      try
      {
        var jsonArgs = JObject.Parse(Convert(args, "{", "}"));
        foreach (var arg in jsonArgs)
        {
          var keyString = arg.Key.ToString();
          var valueString = arg.Value.ToString();
          var replacingString = string.IsNullOrEmpty(valueString) ? $"({keyString}:Empty)" : valueString;
          message = message.Replace(keyString, replacingString, StringComparison.Ordinal);
        }
        
        message = message.Replace("{", string.Empty).Replace("}", string.Empty);
      }
      catch (Exception)
      {
        // В случае ошибки вернётся сообщение без изменений.
      }

      return message;
    }

    #endregion
  }
}
