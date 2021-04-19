using System.Collections.Generic;
using System.Text;

namespace DirectumLogConverter
{
  /// <summary>
  /// Форматтер строки лога формата tsv.
  /// </summary>
  internal sealed class TsvLineFormatter : IOutputLineFormatter
  {
    #region Поля и свойства

    /// <summary>
    /// Словарь постфиксов для элементов строки лога.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> logLineElementPostfix = new Dictionary<string, string>
    {
      {"lg", " |"}
    };

    /// <summary>
    /// Значения ширины по умолчанию для элементов строки лога.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, int> defaultLogLineElementWidth = new Dictionary<string, int>
    {
      {"pid", 10},
      {"l", 5},
      {"lg", 30}
    };

    #endregion

    #region IOutputLineFormatter

    public string Format(IReadOnlyDictionary<string, string> logLineElements)
    {
      var sb = new StringBuilder();
      var firstElement = true;
      var onNewLine = string.Empty;
      foreach (var element in logLineElements)
      {
        var value = element.Value;

        if (value.Length > 0 && value[0] == '\n')
        {
          onNewLine += value;
          continue;
        }

        if (defaultLogLineElementWidth.TryGetValue(element.Key, out var width))
        {
          value = value.Length > width
            ? value.Substring(value.Length - width, width)
            : value.PadLeft(width);
        }

        if (firstElement)
          firstElement = false;
        else
          sb.Append(' ');
        sb.Append(value);

        if (logLineElementPostfix.TryGetValue(element.Key, out var postfix))
          sb.Append(postfix);
      }

      if (!string.IsNullOrEmpty(onNewLine))
        sb.Append(onNewLine);

      return sb.ToString();
    }

    #endregion
  }
}
