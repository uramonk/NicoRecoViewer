using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            FizzBuzz(3, 5, 100);
            a();
            Console.ReadLine();

        }

        private static void FizzBuzz(int fizzNum, int buzzNum, int end)
        {
            for(int i = 1; i <= end; i++)
            {
                StringBuilder sb = new StringBuilder();
                if (i % fizzNum == 0)
                {
                    sb.Append("Fizz");
                }
                if (i % buzzNum == 0)
                {
                    sb.Append("Buzz");
                }
                if(sb.Length == 0)
                {
                    sb.Append(i.ToString());
                }
                Console.WriteLine(sb.ToString());
            }
            
        }

        private static void a()
        {
            String str = Console.ReadLine();
            StringBuilder sb = new StringBuilder();
            foreach (var c in str)
            {
                sb.Insert(0, c);
            }
            Console.WriteLine(sb.ToString());
        }
    }
}
