using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPRulesRegulater
{
    class Program
    {
        static void Main(string[] args)
        {
            AddRule();
            test();
        }

        public static void AddRule()//rule newrule)
        {

            //string[] testlinesA = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(),"RuleSetA.txt"), Encoding.Default);
            //List<rule> testrulesA = new List<rule>();
            //foreach (string t in testlinesA)
            //{
            //    string[] f = t.Split(',');
            //    testrulesA.Add(new rule(f[0], f[1], f[2]));
            //        }
            string[] testlinesB = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "RuleSetC.txt"), Encoding.Default);
            
            List<rule> testrulesB = new List<rule>();
            foreach (string t in testlinesB)
            {
                string[] f = t.Split(',');
                testrulesB.Add(new rule(f[0], f[1], f[2]));
            }

            Box testBoxB = new Box();
            foreach (rule r in testrulesB)
                testBoxB.AddRule(r);

        }

        public static void test()
        {
            //ETREE testtree = new ETREE();
            //int boxindex = 1;
            //while (true)
            //{
            //    string readline = Console.ReadLine();
            //    if (readline.ToLower() == "end") break;
            //    else
            //    {
            //        string[] numinstr = readline.Split(',');
            //        int a = int.Parse(numinstr[0]);
            //        int b = int.Parse(numinstr[1]);
            //        testtree.addnode(a, b, boxindex++);
            //    }
            //}


        }




    }
}
