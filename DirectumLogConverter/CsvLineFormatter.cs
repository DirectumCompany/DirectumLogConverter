using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DirectumLogConverter
{
  /// <summary>
  /// Форматтер строки лога формата сsv.
  /// </summary>
  internal sealed class CsvLineFormatter : IOutputLineFormatter
  {
    #region Поля и свойства

    /// <summary>
    /// Имена элементов строки лога, которые должны быть в начале.
    /// </summary>
    private static readonly string[] logElementsAtBeginning = { "t", "pid", "v", "un", "tn", "l", "tr" };

    #endregion

    #region Методы

    /// <summary>
    /// Выполнить экранирование строки для экспорта с csv.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string EscapeCsv(string str)
    {
      if (!str.Any(c => c is ';' or '"' or '\r' or '\n'))
        return str;

      var sb = new StringBuilder();
      sb.Append('\"');
      foreach (var nextChar in str)
      {
        sb.Append(nextChar);
        if (nextChar == '"')
          sb.Append('\"');
      }
      sb.Append('\"');
      return sb.ToString();
    }

    #endregion

    #region IOutputLineFormatter

    public string Format(IReadOnlyDictionary<string, string> logLineElements)
    {
      var sb = new StringBuilder();
      var firstElement = true;
      var onNewLine = string.Empty;

      var keys = logElementsAtBeginning.Union(logLineElements.Keys.Except(logElementsAtBeginning));

      foreach (var key in keys)
      {
        var value = string.Empty;
        if (logLineElements.ContainsKey(key))
          value = logLineElements[key];

        if (value.Length > 0 && value[0] == '\n')
        {
          onNewLine += value;
          continue;
        }

        if (firstElement)
          firstElement = false;
        else
          sb.Append(';');

        sb.Append(EscapeCsv(value));
      }

      onNewLine = onNewLine.TrimStart('\n');
      if (!string.IsNullOrEmpty(onNewLine))
      {
        if (!firstElement)
          sb.Append(';');
        sb.Append(EscapeCsv(onNewLine));
      }

      return sb.ToString();
    }

    #endregion
  }
}
