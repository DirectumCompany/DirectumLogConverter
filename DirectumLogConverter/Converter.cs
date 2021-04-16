using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace LogConverter
{
  /// <summary>
  /// Класс, который конвертирует из одного формата в другой.
  /// </summary>
  internal static class Converter
  {
    #region Константы

    /// <summary>
    /// Ширина поля для ИД процесса и потока.
    /// </summary>
    private const int PidFieldWidth = 10;

    /// <summary>
    /// Ширина поля для уровня логирования.
    /// </summary>
    private const int LevelFieldWidth = 10;

    /// <summary>
    /// Ширина поля названия логгера.
    /// </summary>
    private const int LoggerFieldWidth = 30;

    #endregion

    #region Методы

    /// <summary>
    /// Сконвертировать файл из JSON в TSV.
    /// </summary>
    /// <param name="sourcePath">Путь к файлу, где хранятся json-строки.</param>
    /// <param name="destinationPath">Путь к файлу, куда запишутся TSV-строки.</param>
    internal static void ConvertJsonToTsv(string sourcePath, string destinationPath)
    {
      using (var writer = new StreamWriter(destinationPath, true, Encoding.Default))
      using (var reader = new StreamReader(sourcePath, Encoding.Default))
      {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
          var resultLine = new StringBuilder();
          var jsonDict = GetJsonValues(line);

          foreach (var jsonPair in jsonDict)
          {
            switch (jsonPair.Key)
            {
              case "pid":
                resultLine.Append(ConvertDefault(jsonPair.Value).PadLeft(PidFieldWidth));
                break;
              case "l":
                resultLine.Append(ConvertDefault(jsonPair.Value).PadLeft(LevelFieldWidth));
                break;
              case "lg":
                resultLine.Append(ConvertDefault(jsonPair.Value).PadLeft(LoggerFieldWidth));
                break;
              case "ex":
                resultLine.Append(ConvertException(jsonPair.Value));
                break;
              case "args":
                resultLine.Append(ConvertArguments(jsonPair.Value));
                break;
              case "cust":
                resultLine.Append(ConvertCustomProperties(jsonPair.Value));
                break;
              case "span":
                resultLine.Append(" ").Append(ConvertSpan(jsonPair.Value));
                break;
              default:
                resultLine.Append(" ").Append(ConvertDefault(jsonPair.Value));
                break;
            }
          }

          writer.WriteLine(resultLine.ToString());
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
      return jTokens.Select(jt => jt.ToString().Replace("\n", string.Empty).Replace("\r", string.Empty)).Aggregate((s1, s2) => s1 + ", " + s2);
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
      var result = new StringBuilder();
      var type = jTokens.Select(token => (JProperty)token).Where(property => property.Name == "type").FirstOrDefault().Value.ToString();
      var message = jTokens.Select(token => (JProperty)token).Where(property => property.Name == "m").FirstOrDefault().Value.ToString();
      var stack = jTokens.Select(token => (JProperty)token).Where(property => property.Name == "stack").FirstOrDefault().Value.ToString();

      result.Append($"\n\t{type}: {message}\n");
      var stackLines = stack.Split("\r\n");
      foreach (var l in stackLines)
      {
        result.Append("\t").Append(l).Append("\n");
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
      return "Span: " + ConvertDefault(jTokens);
    }

    #endregion
  }
}
