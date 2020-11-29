using System;
using System.Threading.Tasks;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;

namespace SharpCredScanner
{
    public class Program
    {
        /// <summary>
        /// <param name="path">The path to look for</param>
        /// </summary>
        /// <returns></returns>
        static async Task<int> Main(string path)
        {
            var ui = new ConsoleHelper();

            if (String.IsNullOrWhiteSpace(path))
            {
                path = ".";
            }
            Logger.Log("The path is: " + path);

            var config = await Config.Load("config.json");
            var results = new List<RuleMatch>();

            var allFiles = Directory.EnumerateFiles(path, "", SearchOption.AllDirectories).ToList();
            var fileFilter = new FileFilter();
            var files = fileFilter.Filter(allFiles, config);

            Logger.Log("Results:");
            Logger.Log("-------------------------------");
            foreach (var file in files)
            {
                foreach(var rule in config.Rules.Where(r => r.Scope == Scope.Filename))
                {
                    if(Regex.IsMatch(file, rule.Regex))
                    {
                        ui.WriteResult(new RuleMatch(rule, file));
                    }
                }

                var content = await File.ReadAllLinesAsync(file);
                foreach (var rule in config.Rules.Where(r => r.Scope == Scope.Content))
                {
                    if(rule.OnlyForExtensions.Any() && !rule.OnlyForExtensions.Contains(Path.GetExtension(file).ToLower()))
                    {
                        continue;
                    }
                    for (var i = 0; i < content.Length; i++)
                    {
                        if (Regex.IsMatch(content[i], rule.Regex))
                        {
                            ui.WriteResult(new RuleMatch(rule, file, content[i], i + 1));
                        }
                    }
                }
                //Entropy
                var entropyRule = new EntropyRule();
                for (var i = 0; i < content.Length; i++)
                {
                    var stringValues = Regex.Matches(content[i], "\"[^\"]+\"").Select(x => x.Value.Replace("\"", ""));
                    foreach(var value in stringValues.Where(v => v.GetEntropy() > 5))
                    {
                        ui.WriteResult(new RuleMatch(entropyRule, file, value, i));
                    }
                }
            }
            return results.Count > 0 ? 1 : 0;
        }
    }

    public class ConsoleHelper
    {
        public void WriteResult(RuleMatch result)
        {
            Logger.Log($"Rule : {result.Rule.Caption}");
            Logger.Log($"File : {result.File}");
            Logger.Log($"Match: {result.Line}");
            Logger.Log($"Line : {result.LineNumber}");
            Logger.Log("-------------------------------");
        }
    }
}
