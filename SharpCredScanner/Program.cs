using System;
using System.Threading.Tasks;
using System.CommandLine;
using System.IO;

namespace SharpCredScanner
{
    class Program
    {
        /// <summary>
        /// <param name="path">The path to look for</param>
        /// </summary>
        /// <returns></returns>
        static async Task<int> Main(string path)
        {
            if(String.IsNullOrWhiteSpace(path))
            {
                path = ".";
            }
            Console.WriteLine("The path is: " + path);

            foreach(var file in Directory.EnumerateFiles(path, "", SearchOption.AllDirectories))
            {
                Console.WriteLine("-> " + file);
            }
            return 0;
        }
    }
}
