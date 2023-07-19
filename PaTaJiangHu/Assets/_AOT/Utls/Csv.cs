using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AOT.Utls
{
    /// <summary>
    /// Csv 读取文件
    /// </summary>
    public static class Csv
    {
        private const string Pattern = @"(?:^|,)(?=[^""]|("")?)""?((?(1)[^""]*|[^,""]*))""?(?=,|$)";
        private static readonly Regex _regex = new(Pattern);
        private const string NullValueIndicator = "NULL"; // You can change this to any other string that represents NULL values in your CSV file.

        public static List<Dictionary<string, object>> Read(TextAsset textAsset,int skipRows = 1)
        {
            var reader = new StringReader(textAsset.text);

            var data = new List<Dictionary<string, object>>();
            string[] columnNames = null;

            var rowIndex = 0;
            string line;

            var sb = new StringBuilder();
            while ((line = reader.ReadLine()) != null)
            {
                if (rowIndex < skipRows)
                {
                    rowIndex++;
                    continue;
                }

                sb.Append(line);
                var quoteCount = line.Count(c => c == '"');

                if (quoteCount % 2 == 1)
                {
                    sb.Append("\n");
                    continue;
                }

                var rowValues = new List<string>();
                foreach (Match match in _regex.Matches(sb.ToString()))
                {
                    var value = match.Groups[2].Value;
                    if (value == NullValueIndicator)
                    {
                        value = null;
                    }
                    rowValues.Add(value);
                }
                sb.Clear();

                if (rowIndex == skipRows)
                {
                    columnNames = rowValues.ToArray();
                }
                else
                {
                    var rowData = new Dictionary<string, object>();
                    for (var i = 0; i < columnNames.Length && i < rowValues.Count; i++)
                    {
                        rowData.Add(columnNames[i], rowValues[i]);
                    }
                    data.Add(rowData);
                }
                rowIndex++;
            }
            return data;
        }

        private static List<Dictionary<string, object>> StreamRead(string filePath, int skipRows = 1)
        {
            var data = new List<Dictionary<string, object>>();
            string[] columnNames = null;

            var rowIndex = 0;
            string line;
            var nullValueIndicator = "NULL";

            using var reader = new StreamReader(filePath);
            var sb = new StringBuilder();
            while ((line = reader.ReadLine()) != null)
            {
                if (rowIndex < skipRows)
                {
                    rowIndex++;
                    continue;
                }

                sb.Append(line);
                var quoteCount = line.Count(c => c == '"');

                if (quoteCount % 2 == 1)
                {
                    sb.Append("\n");
                    continue;
                }

                var rowValues = new List<string>();
                foreach (Match match in _regex.Matches(sb.ToString()))
                {
                    var value = match.Groups[2].Value;
                    if (value == nullValueIndicator)
                    {
                        value = null;
                    }
                    rowValues.Add(value);
                }
                sb.Clear();

                if (rowIndex == skipRows)
                {
                    columnNames = rowValues.ToArray();
                }
                else
                {
                    var rowData = new Dictionary<string, object>();
                    for (var i = 0; i < columnNames.Length && i < rowValues.Count; i++)
                    {
                        rowData.Add(columnNames[i], rowValues[i]);
                    }
                    data.Add(rowData);
                }
                rowIndex++;
            }

            return data;
        }

    }
}