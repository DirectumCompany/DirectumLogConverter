# Directum Log Converter

[![Publish](https://github.com/DirectumCompany/DirectumLogConverter/actions/workflows/release.yml/badge.svg)](https://github.com/DirectumCompany/DirectumLogConverter/actions/workflows/release.yml)

Утилита для конвертации JSON-логов сервисов Directum в текстовый формат (для удобного чтения) и csv-формат (для удобного анализа, например, в Microsoft Excel).

## Как скачать
Дистрибутив доступен по [ссылке](https://github.com/DirectumCompany/DirectumLogConverter/releases).

## Как собрать
Выполнить скрипт `publish.cmd` (должны быть установлены BuildTools 2019).
Скрипт соберет утилиту в виде single-executable файлов для платформ win-x64 и linux-x64 (они появятся в одноименных подпапках папки publish).

## Как пользоваться
`dlc <имя исходного файла> <имя сконвертированного файла>`

По умолчанию конвертация происходит в текстовый формат, если нужно сконвертировать в формат csv, то нужно добавить ключ `-с`.

Добавленный ключ `-m` развернёт аргументы строки сообщения(args) в его текст.

Второй аргумент можно не указывать, тогда имя сконвертированного файла будет сформировано из имени исходного файла.<br/>Например, `WebServer.log` будет сконвертирован в файл `WebServer_converted.log`, либо в `WebServer_converted.csv` если выбран формат csv.

Если выходной файл уже существует, то будет сделан запрос перед тем, как его перезаписать.

Рекомендуется зарегистрировать путь к утилите в переменной окружения PATH, чтобы вызов dlc был возможен из любой папки.
