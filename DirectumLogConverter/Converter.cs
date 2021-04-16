using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace DirectumLogConverter
{
  /// <summary>
  /// Класс, который конвертирует из одного формата в другой.
  /// </summary>
  internal static class Converter
  {
    #region Методы

    /// <summary>
    /// Сконвертировать JSON файл.
    /// </summary>
    /// <param name="options">Опции конвертации.</param>
    internal static void ConvertJson(ConvertOptions options)
    {
      IOutputLineFormatter formatter = options.CsvFormat ? new CsvLineFormatter() : new TsvLineFormatter();

      using var reader = new StreamReader(options.InputPath, Encoding.UTF8);
      using var writer = new StreamWriter(options.OutputPath, false, Encoding.UTF8);

      if (options.CsvFormat)
      {
        var bom = Encoding.UTF8.GetPreamble();
        writer.Write(bom);
      }

      while (true)
      {
        var line = reader.ReadLine();
        if (line == null)
          break;

        string formattedValue;
        try
        {
          var logLineElements = new Dictionary<string, string>();
          var jsonDict = GetJsonValues(line);

          foreach (var jsonPair in jsonDict)
          {
            string result;
            switch (jsonPair.Key)
            {
              case "st":
              case "ex":
                result = ConvertException(jsonPair.Value);
                break;
              case "args":
                result = ConvertArguments(jsonPair.Value);
                break;
              case "cust":
                result = ConvertCustomProperties(jsonPair.Value);
                break;
              case "span":
                result = ConvertSpan(jsonPair.Value);
                break;
              default:
                result = Convert(jsonPair.Value);
                break;
            }

            if (!string.IsNullOrEmpty(result))
              logLineElements[jsonPair.Key] = result;
          }

          formattedValue = formatter.Format(logLineElements);
        }
        catch (Exception)
        {
          formattedValue = formatter.Format(new Dictionary<string, string> { { string.Empty, line } });
        }

        if (!string.IsNullOrEmpty(formattedValue))
        {
          writer.Write(formattedValue);
          writer.Write('\n');
        }
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
    /// Сконвертировать аргументы.
    /// </summary>
    /// <param name="jTokens">Набор токенов.</param>
    /// <returns>Аргумент в виде строки.</returns>
    private static string ConvertArguments(IEnumerable<JToken> jTokens)
    {
      return Convert(jTokens, "(", ")");
    }

    /// <summary>
    /// Сконвертировать свойства.
    /// </summary>
    /// <param name="jTokens">Набор токенов.</param>
    /// <returns>Свойства в виде строки.</returns>
    private static string ConvertCustomProperties(IEnumerable<JToken> jTokens)
    {
      return Convert(jTokens, "[", "]");
    }

    /// <summary>
    /// Сконвертировать спан.
    /// </summary>
    /// <param name="jTokens">Набор токенов.</param>
    /// <returns></returns>
    private static string ConvertSpan(IEnumerable<JToken> jTokens)
    {
      return Convert(jTokens, "Span(", ")");
    }

    /// <summary>
    /// Сконвертировать исключение.
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

    #endregion
  }
}
