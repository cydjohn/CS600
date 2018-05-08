using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPRulesRegulater
{
    class Program
    {
        static void Main(string[] args)
        {
            test();
        }

        public static void AddRule(rule newrule)
        {





        }

        public static void test()
        {
            ETREE testtree = new ETREE();
            int boxindex = 1;
            while (true)
            {
                string readline = Console.ReadLine();
                if (readline.ToLower() == "end") break;
                else
                {
                    string[] numinstr = readline.Split(',');
                    int a = int.Parse(numinstr[0]);
                    int b = int.Parse(numinstr[1]);
                    testtree.addnode(a, b, boxindex++);
                }
            }


        }




    }
}
