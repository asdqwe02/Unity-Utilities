using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Newtonsoft.Json;
namespace Mint.Gdk.Utilities.Runtime
{
    public static class TSVParser
    {
        public static IEnumerable<T> ParseTSVData<T>(string tsvData) where T : new()
        {
            if (string.IsNullOrEmpty(tsvData))
            {
                Debug.LogError("TSV data is null or empty");
                return null;
            }

            try
            {
                var lines = tsvData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].TrimEnd('\r'); // Remove trailing \r if exists
                }
                if (lines.Length < 2)
                {
                    Debug.LogError($"[{nameof(TSVParser)}] TSV data must have at least header and one data row");
                    return null;
                }

                var properties = lines[0].Split('\t').ToArray();
                var listOfData = new List<Dictionary<string, string>>();

                for (var i = 1; i < lines.Length; i++)
                {
                    var lineData = lines[i].Split('\t').ToArray();
                    if (lineData.Length != properties.Length)
                    {
                        Debug.LogError($"[{nameof(TSVParser)}] Row {i} has {lineData.Length} columns but header has {properties.Length} columns\n"
                                       + $"Row {i} data: {string.Join(", ", lineData)}");

                        continue;
                    }
                    if (lineData.All(string.IsNullOrEmpty))
                    {
                        Debug.LogWarning($"[{nameof(TSVParser)}] Row {i} is empty");
                        continue;
                    }

                    var objResult = new Dictionary<string, string>();
                    for (var j = 0; j < properties.Length; j++)
                    {
                        if (string.IsNullOrEmpty(properties[j])) continue;
                        objResult[properties[j]] = lineData[j] ?? string.Empty;
                    }
                    listOfData.Add(objResult);
                }

                var jsonData = JsonConvert.SerializeObject(listOfData);
                return JsonConvert.DeserializeObject<IEnumerable<T>>(jsonData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(TSVParser)}] Error processing TSV data: {ex.Message} {ex.StackTrace}");
                return null;
            }
        }

    }
}
