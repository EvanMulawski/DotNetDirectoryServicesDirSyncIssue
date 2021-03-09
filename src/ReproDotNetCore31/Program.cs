using System;

namespace ReproDotNetCore31
{
    class Program
    {
        static void Main(string[] args)
        {
            using var ds = new DirSync(args[0]/*, args[1], args[2]*/);
            var result = ds.Run();
            result.LogSummary(Console.WriteLine);
            Console.ReadLine();
        }
    }
}
