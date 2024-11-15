using System;
using BenchmarkDotNet.Running;

namespace AbcArbitrage.Homework
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
