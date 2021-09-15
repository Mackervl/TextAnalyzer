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
    class Program
    {
        static async Task Main(string[] args)
        {
            TextAnalyzerHelper textAnalizer = new TextAnalyzerHelper();
            Console.WriteLine("Enter file path:");
            string path = Console.ReadLine();
            string[] separators = { ",", ".", "!", "?", ";", ":", "-", " ", "\r", "\n" };
            var resultTuple = await textAnalizer.CalculateTrippletFromFileAsync(path);
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", resultTuple.Item2.Hours, resultTuple.Item2.Minutes, resultTuple.Item2.Seconds, resultTuple.Item2.Milliseconds / 10);
            Console.WriteLine(resultTuple.Item1);
            Console.WriteLine("Elapsed time: " + elapsedTime);
            Console.ReadKey();
        }
    }
}
