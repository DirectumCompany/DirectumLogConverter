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
            var result = string.Empty;
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
                result = ConvertDefault(jsonPair.Value);
                break;
            }

            if (!string.IsNullOrEmpty(result))
              logLineElements[jsonPair.Key] = result;
          }

          formattedValue = formatter.Format(logLineElements);
        }
        catch (Exception)
        {
          formattedValue = formatter.Format(new Dictionary<string, string> {{string.Empty, line}});
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
    internal static IDictionary<string, IJEnumerable<JToken>> GetJsonValues(string json)
    {
      return JObject.Parse(json).Properties().ToDictionary(kv => kv.Name, kv => kv.Values());
    }

    /// <summary>
    /// Конвертация свойства по умолчанию.
    /// </summary>
    /// <param name="jTokens">Набор токенов.</param>
    /// <returns>Свойство в виде строки.</returns>
    private static string ConvertDefault(IJEnumerable<JToken> jTokens)
    {
      return string.Join(", ", jTokens.Select(jt => jt.ToString().Replace("\n", string.Empty).Replace("\r", string.Empty)));
    }

    /// <summary>
    /// Сконвертировать аргументы.
    /// </summary>
    /// <param name="jTokens">Набор токенов.</param>
    /// <returns>Аргумент в виде строки.</returns>
    private static string ConvertArguments(IJEnumerable<JToken> jTokens)
    {
      return $"({ConvertDefault(jTokens)})";
    }

    /// <summary>
    /// Сконвертировать свойства.
    /// </summary>
    /// <param name="jTokens">Набор токенов.</param>
    /// <returns>Свойства в виде строки.</returns>
    private static string ConvertCustomProperties(IJEnumerable<JToken> jTokens)
    {
      return $"[{ConvertDefault(jTokens)}]";
    }

    /// <summary>
    /// Сконвертировать исключение.
    /// </summary>
    /// <param name="jTokens">Набор токенов.</param>
    /// <returns>Отформатированное исключение в виде строки.</returns>
    private static string ConvertException(IJEnumerable<JToken> jTokens)
    {
      var result = new StringBuilder("\n");
      var type = jTokens.OfType<JProperty>().Where(property => property.Name == "type").FirstOrDefault()?.Value.ToString();
      var message = jTokens.OfType<JProperty>().Where(property => property.Name == "m").FirstOrDefault()?.Value.ToString();
      var stack = jTokens.OfType<JProperty>().Where(property => property.Name == "stack").FirstOrDefault()?.Value.ToString();

      if (!string.IsNullOrEmpty(type))
        result.Append(type);
      else
        result.Append(string.Join('\n', jTokens.Select(jt => jt.ToString())));

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
    /// Сконвертировать спан.
    /// </summary>
    /// <param name="jTokens">Набор токенов.</param>
    /// <returns></returns>
    private static string ConvertSpan(IJEnumerable<JToken> jTokens)
    {
      return "Span(" + ConvertDefault(jTokens) + ")";
    }

    #endregion
  }
}
