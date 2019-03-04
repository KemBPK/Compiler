// Brandon Kem
// CPSC 323
// MW 1:00 PM - 2:15 PM

using System;
using System.IO;
using CompilerLib.LexicalAnalyzer;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            //var path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory.ToString(), "input.txt");
            //Debugging path
            string text = System.IO.File.ReadAllText("~\\..\\..\\..\\..\\input.txt");
            /* 
             * dotnet publish Compiler.sln -c Release -r win10-x64 
             * Go to bin\Release
             */
            //Release path
            //string text = System.IO.File.ReadAllText("input.txt");
            var Lexer = new LexicalAnalyzer(text);
            Console.WriteLine("Enter any key to end program.");
            Console.ReadKey();
        }
    }
}
