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
            //AddRule();
            //test();
            //nievtest();
            List<rect> A = burttestA();
            List<rect> B = burttestB();
            compareAB(A, B);
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

            //Box testBoxB = new Box();
            //foreach (rule r in testrulesB)
            //    testBoxB.AddRule(r);

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

        public static void nievtest()
        {
            List<rule> rules = new List<rule>();
            string[] testlinesB = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "RuleSetB.csv"), Encoding.Default);
            foreach (string s in testlinesB)
            {
                string[] sp = s.Split(',');
                rules.Add(new rule(sp[0], sp[1], sp[2]));
            }
            Box testbox = new Box();
            int n = 0;
            foreach (rule nr in rules)
            {
                bool redundant = false;
                if (n == 59)
                {
                }
                testbox.AddRule(nr, out redundant);
                if (redundant)
                {
                    Console.WriteLine("Rule Redundant: " + n.ToString()+"      " +nr.tostring());
                    //Console.ReadLine();

                }
                n++;
            }
        }

        public static List<rect> burttestB()
        {
            List<rule> rules = new List<rule>();
            string[] testlinesB = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "RuleSetB.csv"), Encoding.Default);
            foreach (string s in testlinesB)
            {
                string[] sp = s.Split(',');
                rules.Add(new rule(sp[0], sp[1], sp[2]));
            }
            List<rect> finrules = new List<rect>();
            int i = 1;
            List<int> redundant = new List<int>();
            foreach(rule nr in rules)
            {
                List<rect> nrs = new List<rect>();
                nrs.Add(new rect(nr.SourceIPRange.StartIP.toInt(), nr.SourceIPRange.EndIP.toInt(), nr.DestIPRange.StartIP.toInt(), nr.DestIPRange.EndIP.toInt(), nr.Allowed));
                foreach (rect r in finrules)
                {
                    //这是一个已存在的r与新加入的组nrs的兼并方案
                    nrs = Combine(r, nrs);
                }
                finrules.AddRange(nrs);
                if (nrs.Count() == 0)
                    redundant.Add(i);
                i++;
            }
            List<string> red = new List<string>();
            foreach (int j in redundant)
                red.Add(j.ToString());
            File.WriteAllLines(Path.Combine(Directory.GetCurrentDirectory(), "RedB.txt"), red.ToArray(), Encoding.Default);
            List<string> cleanedB = new List<string>();
            foreach (rect r in finrules)
                cleanedB.Add(r.torulestring());
            File.WriteAllLines(Path.Combine(Directory.GetCurrentDirectory(), "CleanedB.txt"), cleanedB.ToArray(), Encoding.Default);

            return finrules;
        }
        public static List<rect> burttestA()
        {
            List<rule> rules = new List<rule>();
            string[] testlinesB = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "RuleSetA.csv"), Encoding.Default);
            foreach (string s in testlinesB)
            {
                string[] sp = s.Split(',');
                rules.Add(new rule(sp[0], sp[1], sp[2]));
            }
            List<rect> finrules = new List<rect>();
            int i = 1;
            List<int> redundant = new List<int>();
            foreach (rule nr in rules)
            {
                List<rect> nrs = new List<rect>();
                nrs.Add(new rect(nr.SourceIPRange.StartIP.toInt(), nr.SourceIPRange.EndIP.toInt(), nr.DestIPRange.StartIP.toInt(), nr.DestIPRange.EndIP.toInt(), nr.Allowed));
                foreach (rect r in finrules)
                {
                    //这是一个已存在的r与新加入的组nrs的兼并方案
                    nrs = Combine(r, nrs);
                }
                finrules.AddRange(nrs);
                if (nrs.Count() == 0)
                    redundant.Add(i);
                i++;
            }
            List<string> red = new List<string>();
            foreach (int j in redundant)
                red.Add(j.ToString() + "," + rules[j - 1].tostring());
            File.WriteAllLines(Path.Combine(Directory.GetCurrentDirectory(), "RedA.txt"), red.ToArray(), Encoding.Default);
            List<string> cleanedB = new List<string>();
            foreach (rect r in finrules)
                cleanedB.Add(r.torulestring());
            File.WriteAllLines(Path.Combine(Directory.GetCurrentDirectory(), "CleanedA.txt"), cleanedB.ToArray(), Encoding.Default);

            return finrules;
        }

        public static void compareAB(List<rect> A, List<rect> B)
        {
            List<rect> cA = new List<rect>();
            List<rect> cB = new List<rect>();
            foreach (rect r in A)
                if (!AhasB(B,r))
                    cA.Add(r);
            foreach (rect r in B)
                if (!AhasB(A,r))
                    cB.Add(r);
            List<rect> AA = cA.Where(a => a.allowed).ToList();
            List<rect> AD = cA.Where(a => !a.allowed).ToList();
            List<rect> BA = cB.Where(a => a.allowed).ToList();
            List<rect> BD = cB.Where(a => !a.allowed).ToList();

            List<string> result = new List<string>();
            cA = cA.OrderBy(c => c.SS).ThenBy(c => c.DS).ToList();
            foreach (rect a in cA)
                result.Add(a.torulestring());
            File.WriteAllLines(Path.Combine(Directory.GetCurrentDirectory(), "Rule_inAagainstB.txt"), result.ToArray(), Encoding.Default);
            result = new List<string>();
            foreach (rect a in cB)
                result.Add(a.torulestring());
            File.WriteAllLines(Path.Combine(Directory.GetCurrentDirectory(), "Rule_inBagainstA.txt"), result.ToArray(), Encoding.Default);


            List<rect> merged = Merge(cA);
            merged = Merge(merged);
            merged = Merge(merged);
            merged = Merge(merged);
            merged = Merge(merged);
            merged = Merge(merged);
            merged = Merge(merged);
            merged = Merge(merged);
            merged = Merge(merged);
            merged = Merge(merged);
            merged = Merge(merged);
            merged = Merge(merged);
            merged = Merge(merged);
            merged = Merge(merged);
            merged = Merge(merged);
            merged = Merge(merged);
            result = new List<string>();
            foreach (rect a in merged)
                result.Add(a.torulestring());
            File.WriteAllLines(Path.Combine(Directory.GetCurrentDirectory(), "merged.txt"), result.ToArray(), Encoding.Default);


        }

        public static bool AhasB(List<rect>A, rect b)
        {
            List<rect> nbs = new List<rect>();
            nbs.Add(b);
            foreach(rect r in A)
            {
                nbs = ahasb(r, nbs, b.allowed);
            }

            bool result = (nbs.Count() == 0);

            return result;
        }

        public static List<rect> ahasb(rect r, List<rect> nrs, bool allowed)
        {
            List<rect> result = new List<rect>();
            if (r.allowed != allowed) return nrs;
            foreach (rect nr in nrs)
            {
                if (nr.SS > r.SE || nr.SE < r.SS || nr.DS > r.DE || nr.DE < r.DS)
                {
                    result.Add(nr);
                    continue;
                }
                if (nr.SS < r.SS)
                {
                    #region 第一层
                    if (nr.SE <= r.SE)
                    {
                        if (nr.DS < r.DS)
                        {
                            if (nr.DE <= r.DE)
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                                result.Add(new rect(nr.SS, r.SS - 1, r.DS, nr.DE, nr.allowed));
                            }
                            else
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                                result.Add(new rect(nr.SS, r.SS - 1, r.DS, r.DE, nr.allowed));
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                        else
                        {
                            if (nr.DE <= r.DE)
                            {
                                result.Add(new rect(nr.SS, r.SS - 1, nr.DS, nr.DE, nr.allowed));
                            }
                            else
                            {
                                result.Add(new rect(nr.SS, r.SS - 1, nr.DS, r.DE, nr.allowed));
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                    }
                    else
                    {
                        if (nr.DS < r.DS)
                        {
                            if (nr.DE <= r.DE)
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                                result.Add(new rect(nr.SS, r.SS - 1, r.DS, nr.DE, nr.allowed));
                                result.Add(new rect(r.SE + 1, nr.SE, r.DS, nr.DE, nr.allowed));
                            }
                            else
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                                result.Add(new rect(nr.SS, r.SS - 1, r.DS, r.DE, nr.allowed));
                                result.Add(new rect(r.SE + 1, nr.SE, r.DS, r.DE, nr.allowed));
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                        else
                        {
                            if (nr.DE <= r.DE)
                            {
                                result.Add(new rect(nr.SS, r.SS - 1, nr.DS, nr.DE, nr.allowed));
                                result.Add(new rect(r.SE + 1, nr.SE, nr.DS, nr.DE, nr.allowed));
                            }
                            else
                            {
                                result.Add(new rect(nr.SS, r.SS - 1, nr.DS, r.DE, nr.allowed));
                                result.Add(new rect(r.SE + 1, nr.SE, nr.DS, r.DE, nr.allowed));
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                    }
                    #endregion 第一层
                }
                else
                {
                    #region 第二层
                    if (nr.SE <= r.SE)
                    {
                        if (nr.DS < r.DS)
                        {
                            if (nr.DE <= r.DE)
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                            }
                            else
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                        else
                        {
                            if (nr.DE <= r.DE)
                            {
                                ;
                            }
                            else
                            {
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                    }
                    else
                    {
                        if (nr.DS < r.DS)
                        {
                            if (nr.DE <= r.DE)
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                                result.Add(new rect(r.SE + 1, nr.SE, r.DS, nr.DE, nr.allowed));
                            }
                            else
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                                result.Add(new rect(r.SE + 1, nr.SE, r.DS, r.DE, nr.allowed));
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                        else
                        {
                            if (nr.DE <= r.DE)
                            {
                                result.Add(new rect(r.SE + 1, nr.SE, nr.DS, nr.DE, nr.allowed));
                            }
                            else
                            {
                                result.Add(new rect(r.SE + 1, nr.SE, nr.DS, r.DE, nr.allowed));
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                    }
                    #endregion 第二层
                }

            }

            return result;
        }


        public static List<rect> Combine(rect r, List<rect> nrs)
        {
            List<rect> result = new List<rect>();
            foreach (rect nr in nrs)
            {
                if (nr.SS > r.SE || nr.SE < r.SS || nr.DS > r.DE || nr.DE < r.DS)
                {
                    result.Add(nr);
                    continue;
                }
                if (nr.SS < r.SS)
                {
                    #region 第一层
                    if (nr.SE <= r.SE)
                    {
                        if (nr.DS < r.DS)
                        {
                            if (nr.DE <= r.DE)
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                                result.Add(new rect(nr.SS, r.SS - 1, r.DS, nr.DE, nr.allowed));
                            }
                            else
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                                result.Add(new rect(nr.SS, r.SS - 1, r.DS, r.DE, nr.allowed));
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                        else
                        {
                            if (nr.DE <= r.DE)
                            {
                                result.Add(new rect(nr.SS, r.SS - 1, nr.DS, nr.DE, nr.allowed));
                            }
                            else
                            {
                                result.Add(new rect(nr.SS, r.SS - 1, nr.DS, r.DE, nr.allowed));
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                    }
                    else
                    {
                        if (nr.DS < r.DS)
                        {
                            if (nr.DE <= r.DE)
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                                result.Add(new rect(nr.SS, r.SS - 1, r.DS, nr.DE, nr.allowed));
                                result.Add(new rect(r.SE + 1, nr.SE, r.DS, nr.DE, nr.allowed));
                            }
                            else
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                                result.Add(new rect(nr.SS, r.SS - 1, r.DS, r.DE, nr.allowed));
                                result.Add(new rect(r.SE + 1, nr.SE, r.DS, r.DE, nr.allowed));
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                        else
                        {
                            if (nr.DE <= r.DE)
                            {
                                result.Add(new rect(nr.SS, r.SS - 1, nr.DS, nr.DE, nr.allowed));
                                result.Add(new rect(r.SE+1, nr.SE, nr.DS, nr.DE, nr.allowed));
                            }
                            else
                            {
                                result.Add(new rect(nr.SS, r.SS - 1, nr.DS, r.DE, nr.allowed));
                                result.Add(new rect(r.SE + 1, nr.SE, nr.DS, r.DE, nr.allowed));
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                    }
                    #endregion 第一层
                }
                else
                {
                    #region 第二层
                    if (nr.SE <= r.SE)
                    {
                        if (nr.DS < r.DS)
                        {
                            if (nr.DE <= r.DE)
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                            }
                            else
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                        else
                        {
                            if (nr.DE <= r.DE)
                            {
                                ;
                            }
                            else
                            {
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                    }
                    else
                    {
                        if (nr.DS < r.DS)
                        {
                            if (nr.DE <= r.DE)
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                                result.Add(new rect(r.SE + 1, nr.SE, r.DS, nr.DE, nr.allowed));
                            }
                            else
                            {
                                result.Add(new rect(nr.SS, nr.SE, nr.DS, r.DS - 1, nr.allowed));
                                result.Add(new rect(r.SE + 1, nr.SE, r.DS, r.DE, nr.allowed));
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                        else
                        {
                            if (nr.DE <= r.DE)
                            {
                                result.Add(new rect(r.SE + 1, nr.SE, nr.DS, nr.DE, nr.allowed));
                            }
                            else
                            {
                                result.Add(new rect(r.SE + 1, nr.SE, nr.DS, r.DE, nr.allowed));
                                result.Add(new rect(nr.SS, nr.SE, r.DE + 1, nr.DE, nr.allowed));
                            }
                        }
                    }
                    #endregion 第二层
                }

            }
            return result;
        }

        public static List<rect> Merge(List<rect> oldrects)
        {
            List<rect> nrects=new List<rect>();
            nrects.AddRange(oldrects);
            List<int> jump = new List<int>();
            for (int i = 0; i < nrects.Count(); i++)
            {
                for(int j = i+1;j<nrects.Count();j++)
                {
                    if (nrects[j].SS == nrects[i].SS && nrects[j].SE == nrects[i].SE)
                        if (nrects[j].DS > nrects[i].DE && nrects[j].DS == nrects[i].DE + 1)
                        {
                            nrects[i].DE = nrects[j].DE;
                            nrects.RemoveAt(j);
                        }
                    else if(nrects[j].DE<nrects[i].DS && nrects[j].DE == nrects[i].DS-1)
                        {
                            nrects[i].DS = nrects[j].DS;
                            nrects.RemoveAt(j);
                        }
                    if (nrects.Count() == j) break;
                    if(nrects[j].DS == nrects[i].DS && nrects[j].DE == nrects[i].DE)
                        if(nrects[j].SS>nrects[i].SE && nrects[j].SS == nrects[i].SE+1)
                        {
                            nrects[i].SE = nrects[j].SE;
                            nrects.RemoveAt(j);
                        }
                    else if(nrects[j].SE<nrects[i].SS && nrects[j].SE == nrects[i].SS-1)
                        {
                            nrects[i].SS = nrects[j].SS;
                            nrects.RemoveAt(j);
                        }

                }
            }
            return nrects;
        }

        public class rect
        {
            public int SS;
            public int SE;
            public int DS;
            public int DE;
            public bool allowed;
            public rect(int SS, int SE, int DS, int DE, bool allowed)
            {
                this.SS = SS;
                this.SE = SE;
                this.DS = DS;
                this.DE = DE;
                this.allowed = allowed;
            }
            public string torulestring()
            {
                string result = "";
                result += numstring(this.SS) + "-" + numstring(this.SE) + "," + numstring(this.DS) + "-" + numstring(this.DE) + ",";
                if (this.allowed)
                    result = result + "Allow";
                else
                    result = result + "Deny";
                return result;
            }
            public string numstring(int SS)
            {
                bool neg = false;
                if (SS < 0) neg = true;

                if (neg) SS = SS - (-2147483648);
                int F = SS / 16777216;
                SS = SS - F * 16777216;
                int S = SS / 65536;
                SS = SS - S * 65536;
                int T = SS / 256;
                SS = SS - T * 256;
                int L = SS;

                return F.ToString()+"."+S.ToString()+"."+T.ToString()+"."+L.ToString();
            }
        }



    }
}
