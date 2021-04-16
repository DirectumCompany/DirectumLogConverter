using System.Collections.Generic;

namespace DirectumLogConverter
{
  /// <summary>
  /// Интерфейс форматтера строки лога.
  /// </summary>
  internal interface IOutputLineFormatter
  {
    /// <summary>
    /// Отформатировать стоку лога.
    /// </summary>
    /// <param name="logLineElements">Элементы строки лога.</param>
    /// <returns>Форматированная стока лога.</returns>
    string Format(IReadOnlyDictionary<string, string> logLineElements);
  }
}
