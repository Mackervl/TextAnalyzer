using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TextAnalyzer
{
    public class TextAnalyzerHelper
    {
        private static readonly string[] _separators = new string[] { ",", ".", "!", "?", ";", ":", " ", "\r", "\n" };

        private string GetTextFromFile(string filePath)
        {
            string text = string.Empty;
            try
            {
                using (var stream = new StreamReader(filePath))
                {
                    text = stream.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return text;
        }

        private List<string> GetWordsFromText(string text, string[] separators)
        {
            return text.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private List<List<string>> GetWordParts(List<string> words, int maxThreadsUsed)
        {
            var resultList = new List<List<string>>();
            var sourceListLength = words.Count;
            var partLength = (int)Math.Ceiling(sourceListLength / (decimal)maxThreadsUsed);
            for (int i = 0; i < maxThreadsUsed; i++)
            {
                var sourceIndex = i * partLength;
                var targetLength = partLength;
                if (sourceIndex + partLength > sourceListLength)
                {
                    targetLength = sourceListLength - sourceIndex;
                }
                var qwerty = words.ToList().GetRange(sourceIndex, targetLength);

                resultList.Add(qwerty);
            }
            return resultList;
        }

        private void CalculateTrippletCount(ConcurrentDictionary<string, int> trippletDictionary, List<string> wordsArray)
        {
            foreach (string word in wordsArray)
            {
                char trippletSymbol = ' ';
                for (int i = 0; i < word.Length; i++)
                {
                    if (word.Length - i < 3) break;

                    if (trippletSymbol != word[i] && (word[i] == word[i + 1] && word[i] == word[i + 2]))
                    {
                        string tripplet = word.Substring(i, 3);
                        trippletDictionary.AddOrUpdate(tripplet, 1, (key, oldValue) => oldValue + 1);
                        trippletSymbol = word[i];
                    }
                }
            }
        }
        private string GetTrippletReport(ConcurrentDictionary<string, int> trippletDicttionary)
        {
            List<KeyValuePair<string, int>> trippletList = trippletDicttionary.ToList();

            trippletList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));


            var bufList = trippletList.TakeLast(10).Reverse().ToArray();
            var stringBuilder = new StringBuilder("Top 10 tripplets: ");

            for (int i = 0; i < bufList.Length; i++)
            {
                stringBuilder.Append(bufList[i].Key + " - " + bufList[i].Value + ", ");
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);

            return stringBuilder.ToString();
        }

        private List<Task> GetTasksToCalculate(ConcurrentDictionary<string, int> dict, List<List<string>> wordPartsForTasks)
        {
            var tasks = new List<Task>();
            for (int i = 0; i < wordPartsForTasks.Count; i++)
            {
                var targetArray = wordPartsForTasks[i];
                tasks.Add(Task.Run(() => CalculateTrippletCount(dict, targetArray)));
            }
            return tasks;
        }
        public async Task<Tuple<string, TimeSpan>> CalculateTrippletFromFileAsync(string path, string[] separators = null, int maxThreadUsed = 8)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            if (maxThreadUsed > 32) maxThreadUsed = 32;
            if (maxThreadUsed < 1) maxThreadUsed = 8;
            if (separators == null || separators.Length == 0) separators = _separators;
            // Получение текст из файла
            var textFromFile = GetTextFromFile(path);

            // Получение список слов текста
            var listOfWords = GetWordsFromText(textFromFile, separators);

            //Разбиваем список слов на части для тасков
            var wordPartsForTasks = GetWordParts(listOfWords, maxThreadUsed);
            var resultDictionary = new ConcurrentDictionary<string, int>();


            // Формирование набора задач для многопоточного выполнения
            var tasks = GetTasksToCalculate(resultDictionary, wordPartsForTasks);
            await Task.WhenAll(tasks);

            // Формирование результирующей строки
            var resultString = GetTrippletReport(resultDictionary);
            stopWatch.Stop();
            TimeSpan timeSpan = stopWatch.Elapsed;

            return new Tuple<string, TimeSpan>(resultString, timeSpan);
        }
    }
}
