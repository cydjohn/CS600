using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPRulesRegulater
{

    class IPRange
    {
        public IP StartIP;
        public IP EndIP;
        public IPRange(string IPString)
        {
            this.StartIP = new IP(IPString.Replace("*", "0"));
            this.EndIP = new IP(IPString.Replace("*", "255"));
        }

        public IPRange(int StartIPNum, int EndIPNum)
        {
            this.StartIP = new IP(StartIPNum);
            this.EndIP = new IP(EndIPNum);
        }

        public class IP
        {
            public int F;
            public int S;
            public int T;
            public int L;
            public bool hasslash;
            public int slash;
            public IP(string IPstring)
            {
                try {
                    string[] sep = IPstring.Split('/');
                    this.F = int.Parse(sep[0]);
                    this.S = int.Parse(sep[1]);
                    this.T = int.Parse(sep[2]);
                    if (sep[3].Contains("/"))
                    {
                        this.L = int.Parse(sep[3].Split('/')[0]);
                        this.hasslash = true;
                        this.slash = int.Parse(sep[3].Split('/')[1]);
                    }
                    else
                    {
                        this.L = int.Parse(sep[3]);
                        this.hasslash = false;
                    }
                }
                catch {
                    Console.Write("Wrong IP string. " + IPstring);
                    Console.ReadLine();
                }
            }

            public IP(int num)
            {
                int res = num;
                this.F = res / 16777216;
                res = res - F * 16777216;
                this.S = res / 65536;
                res = res - S * 65536;
                this.T = res / 256;
                res = res - T * 256;
                this.L = res;
//todo这里同样没有考虑slash的情况
            }
            public int toInt()
            {
                int re = ((this.F * 256 + this.S) * 256 + this.T) * 256 + this.L;
                return re;//todo添加转换规则，将一个IP地址换为坐标中的一个数字。还需要把slash编进去
            }

            
        }

    }

    class rule
    {
        public IPRange SourceIPRange;

        public IPRange DestIPRange;

        public bool Allowed;

        public rule(string SourceIPR, string DestIPR, string allowed)
        {
            this.SourceIPRange = new IPRange(SourceIPR);
            this.DestIPRange = new IPRange(DestIPR);

            if (allowed.ToLower() == "allow")
                this.Allowed = true;
            else
                if (allowed.ToLower() == "block")
                this.Allowed = false;
            else
            {
                Console.WriteLine("Wrong rule. " + allowed);
            }
        }

        public rule(int SS, int SE, int DS, int DE, bool allowed)
        {
            this.SourceIPRange = new IPRange(SS, SE);
            this.DestIPRange = new IPRange(DS, DE);
            this.Allowed = allowed;
        }

    }

    class Box
    {
        private List<savedrule> savedrules;

        private static ETREE SourceIPTree;

        private static ETREE ReverseSourceIPTree;

        private static ETREE DestIPTree;//todo 不用public吧

        public void AddRule(rule newrule, int boxindex = 0, bool ischecking = false)
        {

            int SS = newrule.SourceIPRange.StartIP.toInt();
            int SE = newrule.SourceIPRange.EndIP.toInt();
            int DS = newrule.DestIPRange.StartIP.toInt();
            int DE = newrule.DestIPRange.EndIP.toInt();



            //检查有没有重复，先列出要重的
            List<int> Intervaled = FindIntevals(SS, SE, DS, DE);

            //一一调整校对，得出需要添加的裂解的box，以及哪些需要留观后效
            ruledivider Newrules = ChecktheseBoxes(Intervaled, SS, SE, DS, DE);

            //对每一个，分情况加载
            foreach(rec r in Newrules.finishedrecs)
            AddDistinctRules(r, newrule.Allowed);
            
        }

        private void AddRule(int SS, int SE, int DS, int DE, bool allowed)
        {
            savedrule newr = new savedrule(SS, SE, DS, DE, allowed, this.savedrules.Count());
            int index = -1;
            SourceIPTree.addnode(SS, SE, newr.index, out index);
            newr.SourceIPTIndex = index;
            ReverseSourceIPTree.addnode(SE, SS, newr.index, out index);
            newr.RSourceIPTIndex = index;
            DestIPTree.addnode(DS, DE, newr.index, out index);
            newr.DestIPTIndex = index;
            this.savedrules.Add(newr);
        }

        private void AddDistinctRules(rec r, bool allowed)
        {
            //先在两棵树中查找，把紧挨着的找出来，并从上到下排序
            List<int> toright = SourceIPTree.connectedrecs(r.SE);
            toright = toright.Where(i => this.savedrules[i].DS <= r.DE && this.savedrules[i].DE >= r.DS && this.savedrules[i].allowed == newrule.Allowed).ToList();
            toright = toright.OrderBy(t => this.savedrules[t].DS).ToList();
            List<int> toleft = ReverseSourceIPTree.connectedrecs(r.SS);
            toleft = toleft.Where(i => this.savedrules[i].DS <= r.DE && this.savedrules[i].DE >= r.DS && this.savedrules[i].allowed == newrule.Allowed).ToList();
            toleft = toleft.OrderBy(t => this.savedrules[t].DS).ToList();

            if ((toright != null || toright.Count() != 0) && (toleft != null || toleft.Count() != 0))
            {
                //todo 需要填充，左右都有东西的时候该怎么办

            }
            #region ifhaverightneighbor
            if (toright != null || toright.Count() != 0)
            {//仅右侧有可续规则时怎么办
                int upperone = -1;
                int lowerone = -1;
                int uppercheckpoint = r.DE;
                int lowercheckpoint = r.DS;
                foreach (int i in toright)
                {
                    if (this.savedrules[i].DE >= r.DE)
                        upperone = i;
                    if (this.savedrules[i].DS <= r.DS)
                        lowerone = i;
                }

                if (upperone != -1)
                {
                    uppercheckpoint = this.savedrules[upperone].DS - 1;
                    if (this.savedrules[upperone].DS > r.DS)
                    {
                        this.savedrules[upperone].DS = r.DE + 1;
                        this.savedrules[upperone].DestIPTIndex = DestIPTree.changeStartPoint(this.savedrules[upperone].index, this.savedrules[upperone].RSourceIPTIndex, r.DS + 1, this.savedrules[upperone].DE);
                    }
                    else
                        delete(upperone);
                    AddRule(r.SS, this.savedrules[upperone].SE, uppercheckpoint + 1, r.DE, allowed);
                }

                if (lowerone != -1)
                {
                    lowercheckpoint = this.savedrules[lowerone].DE + 1;
                    if (this.savedrules[lowerone].DS < r.DS)
                    {
                        this.savedrules[lowerone].DE = r.DS - 1;
                        DestIPTree.nodes[savedrules[lowerone].DestIPTIndex].Boxes.Remove(lowerone);
                        DestIPTree.nodes[savedrules[lowerone].DestIPTIndex].Boxes.Add(lowerone, r.DS - 1);
                    }
                    else
                        delete(lowerone);
                    AddRule(r.SS, this.savedrules[lowerone].SE, r.DS, lowercheckpoint - 1, allowed);
                }

                List<line> linesegs = new List<line>();
                linesegs.Add(new line(lowercheckpoint, uppercheckpoint));

                foreach (int i in toright)
                {
                    if (i == lowerone || i == upperone) continue;
                    int a = this.savedrules[i].DS;
                    int b = this.savedrules[i].DE;
                    int tempindex = -1;
                    for (int tempi = 1; tempi < linesegs.Count(); tempi++)
                        if (linesegs[tempi].startpoint <= a && linesegs[tempi].endpoint >= b)
                            tempindex = tempi;
                    if (linesegs[tempindex].startpoint < a)
                        linesegs.Add(new line(linesegs[tempindex].startpoint, a - 1));
                    if (linesegs[tempindex].endpoint > b)
                        linesegs.Add(new line(b + 1, linesegs[tempindex].endpoint));
                    linesegs.RemoveAt(tempindex);
                    AddRule(r.SS, this.savedrules[i].SE, a, b, allowed);
                    delete(i);
                }
                foreach (line l in linesegs)
                    AddRule(r.SS, r.SE, l.startpoint, l.endpoint, allowed);
                return;
            }
            #endregion ifhaverightneighbor

            #region ifhaveleftneighbor
            if (toleft != null || toleft.Count() != 0)
            {
                int i = 0;
                int ScanLine = r.DS;
                //检查第一个是否会越界，ds低于r的ds
                if (savedrules[toleft[i]].DS < r.DS)
                {
                    changeDE(toleft[i], r.DS - 1);
                    AddRule(savedrules[toleft[i]].SS, r.SE, r.DS, savedrules[toleft[i]].DE, allowed);
                    i++;
                    ScanLine = savedrules[toleft[i]].DE+1;
                }
                //对中间的，先弄gap里的，再弄共存的
                while (i<toleft.Count() && savedrules[toleft[i]].DE<=r.DE)
                {
                    if(savedrules[toleft[i]].DS> ScanLine)
                    {
                        AddRule(r.SS, r.SE, ScanLine, savedrules[i].DS - 1, allowed);
                        ScanLine = savedrules[toleft[i]].DS;
                    }
                    changeSE(toleft[i], r.SE);
                    i++;
                    ScanLine = savedrules[toleft[i]].DE+1;
                }
                //如果没有超界的就算了
                if (ScanLine > r.DE) return;
                if (i == toleft.Count() - 1)
                {
                    AddRule(r.SS, r.SE, ScanLine, r.DE, allowed);
                }
                else
                {
                    if (savedrules[toleft[i]].DS > ScanLine)
                    {
                        AddRule(r.SS, r.SE, ScanLine, savedrules[i].DS - 1, allowed);
                        ScanLine = savedrules[toleft[i]].DS;
                    }
                    AddRule(savedrules[toleft[i]].SS, r.SE, ScanLine, r.DE, allowed);
                    changeDS(toleft[i], r.DE + 1);
                }
                return;
            }
            #endregion ifhaveleftneighbor

            #region ifnoneighbor
            AddRule(r.SS, r.SE, r.DS, r.DE, allowed);
            #endregion ifnoneighbor
            return;

        }

        /// <summary>
        /// 利用SouceIPT和DestIPT找到所交叠的矩形index
        /// </summary>
        /// <param name="SS"></param>
        /// <param name="SE"></param>
        /// <param name="DS"></param>
        /// <param name="DE"></param>
        /// <returns></returns>
        private List<int> FindIntevals(int SS, int SE, int DS, int DE)
        {
            List<int> SourceIntervals = this.SourceIPTree.intervals(SS, SE);
            List<int> intervals = new List<int>();
            foreach (int ind in SourceIntervals)
                this.savedrules[ind].marked = true;
            foreach (int ind in this.DestIPTree.intervals(DS, DE))
                if (this.savedrules[ind].marked)
                    intervals.Add(ind);
            foreach (int ind in SourceIntervals)
                this.savedrules[ind].marked = false;
            return intervals;
        }

        private ruledivider ChecktheseBoxes(List<int> recindexes, int SS, int SE, int DS, int DE)
        {
            List<Event> events = new List<Event>();
            foreach (int i in recindexes)
            {
                events.Add(new Event(i, Math.Max(this.savedrules[i].SS, SS), true));
                events.Add(new Event(i, Math.Min(this.savedrules[i].SE, SE), false));
            }
            events = events.OrderBy(e => e.time).ToList();
            ruledivider divider = new ruledivider(SS, SE, DS, DE);
            foreach (Event e in events)
            {
                if (e.start)
                    divider.addinterval(Math.Min(DE, this.savedrules[e.index].DE), Math.Max(DS, this.savedrules[e.index].DS), e.time);
                else
                    divider.deleteinterval(Math.Min(DE, this.savedrules[e.index].DE), Math.Max(DS, this.savedrules[e.index].DS), e.time);
            }
            divider.finish();
            return divider;
        }

        private void delete(int index)
        {
            int last = this.savedrules.Count();
            while (this.savedrules[last].empty)
            {
                SourceIPTree.nodes[this.savedrules[last].SourceIPTIndex].Boxes.Remove(last);
                ReverseSourceIPTree.nodes[this.savedrules[last].RSourceIPTIndex].Boxes.Remove(last);
                DestIPTree.nodes[this.savedrules[last].DestIPTIndex].Boxes.Remove(last);
                this.savedrules.RemoveAt(last);
                last--;
            }

            SourceIPTree.nodes[this.savedrules[index].SourceIPTIndex].Boxes.Remove(index);
            ReverseSourceIPTree.nodes[this.savedrules[index].RSourceIPTIndex].Boxes.Remove(index);
            DestIPTree.nodes[this.savedrules[index].DestIPTIndex].Boxes.Remove(index);

            this.savedrules[index].copy(this.savedrules[last]);

            SourceIPTree.nodes[this.savedrules[index].SourceIPTIndex].Boxes.Remove(last);
            SourceIPTree.nodes[this.savedrules[index].SourceIPTIndex].Boxes.Add(index, this.savedrules[index].SE);

            ReverseSourceIPTree.nodes[this.savedrules[index].RSourceIPTIndex].Boxes.Remove(last);
            ReverseSourceIPTree.nodes[this.savedrules[index].RSourceIPTIndex].Boxes.Add(index, this.savedrules[index].SS);

            DestIPTree.nodes[this.savedrules[index].DestIPTIndex].Boxes.Remove(last);
            DestIPTree.nodes[this.savedrules[index].DestIPTIndex].Boxes.Add(index, this.savedrules[index].DE);

            this.savedrules.RemoveAt(last);
        }

        private void changeSS(int ruleindex, int newSS)
        {
            int oldSS = savedrules[ruleindex].SS;
            savedrules[ruleindex].SS = newSS;
            savedrules[ruleindex].SourceIPTIndex = SourceIPTree.changeStartPoint(ruleindex, savedrules[ruleindex].SourceIPTIndex, newSS);
            ReverseSourceIPTree.changeEndPoint(ruleindex, savedrules[ruleindex].RSourceIPTIndex, newSS);
        }
        private void changeSE(int ruleindex, int newSE)
        {
            int oldSE = savedrules[ruleindex].SE;
            savedrules[ruleindex].SE = newSE;
            savedrules[ruleindex].RSourceIPTIndex = ReverseSourceIPTree.changeStartPoint(ruleindex, savedrules[ruleindex].RSourceIPTIndex, newSE);
            SourceIPTree.changeEndPoint(ruleindex, savedrules[ruleindex].SourceIPTIndex, newSE);
        }
        private void changeDS(int ruleindex, int newDS)
        {
            int oldDS = savedrules[ruleindex].DS;
            savedrules[ruleindex].DS = newDS;
            savedrules[ruleindex].DestIPTIndex = DestIPTree.changeStartPoint(ruleindex, savedrules[ruleindex].DestIPTIndex, newDS);
        }
        private void changeDE(int ruleindex, int newDE)
        {
            int oldDE = savedrules[ruleindex].DE;
            savedrules[ruleindex].DE = newDE;
            DestIPTree.changeEndPoint(ruleindex, savedrules[ruleindex].DestIPTIndex, newDE);
        }

        class line
        {
            public int startpoint;
            public int endpoint;
            public line(int a, int b)
            {
                this.startpoint = a;
                this.endpoint = b;
            }
        }

        class Event
        {
            public int time;
            public bool start;
            public int index;
            public Event(int index, int time, bool start)
            {
                this.index = index;
                this.time = time;
                this.start = start;
            }
        }



        class savedrule
        {
            public int SS;
            public int SE;
            public int DS;
            public int DE;
            public int index;
            public bool marked;
            public bool allowed;
            public bool empty;
            public int SourceIPTIndex;
            public int RSourceIPTIndex;
            public int DestIPTIndex;
            public savedrule(int SS, int SE, int DS, int DE, bool allowed, int index)
            {
                this.SS = SS;
                this.SE = SE;
                this.DS = DS;
                this.DE = DE;
                this.allowed = allowed;
                this.index = index;
                this.empty = false;
            }
            public void copy(savedrule saved)
            {
                this.SS = saved.SS;
                this.SE = saved.SE;
                this.DS = saved.DS;
                this.DE = saved.DE;
                this.allowed = saved.allowed;
                this.empty = saved.empty;
                this.index = saved.index;
                this.SourceIPTIndex = saved.SourceIPTIndex;
                this.RSourceIPTIndex = saved.RSourceIPTIndex;
                this.DestIPTIndex = saved.DestIPTIndex;
            }

        }

        class rec
        {
            public int SS;
            public int SE;
            public int DS;
            public int DE;
            public bool end;
            //public bool marked;
            //public bool allowed;
            public rec(int SS, int DS, int DE)
            {
                this.SS = SS;
                this.DS = DS;
                this.DE = DE;
                this.end = false;
            }

        }

        class ruledivider
        {
            int SS;
            int SE;
            int DS;
            int DE;
            List<rec> rectangles;
            public List<rec> finishedrecs;

            /// <summary>
            /// 这个函数表述的是遇到新的已存规则该怎么做
            /// </summary>
            /// <param name="intervalDS"></param>
            /// <param name="intervalDE"></param>
            /// <param name="Scanline"></param>
            public void addinterval(int intervalDS, int intervalDE, int Scanline)
            {
                List<rec> newrecs = new List<rec>();
                foreach (rec r in rectangles)
                {
                    if (r.end || r.DE < intervalDS || r.DS > intervalDE) continue;
                    r.SE = Scanline - 1;
                    r.end = true;
                    if (intervalDE < r.DE)
                        newrecs.Add(new rec(Scanline, intervalDE + 1, r.DE));
                    if (intervalDS > r.DS)
                        newrecs.Add(new rec(Scanline, r.DS, intervalDS - 1));
                }
                finishedrecs.AddRange(rectangles.Where(r => r.end && r.SE >= r.SS));
                rectangles.AddRange(newrecs);
                rectangles = rectangles.Where(r => !r.end).ToList();
            }

            /// <summary>
            /// 当一个已存规则扫描完了该怎么做
            /// </summary>
            /// <param name="intervalDS"></param>
            /// <param name="intervalDE"></param>
            /// <param name="Scanline"></param>
            public void deleteinterval(int intervalDS, int intervalDE, int Scanline)
            {
                rectangles.Add(new rec(Scanline + 1, intervalDS, intervalDE));
            }

            public void finish()
            {
                foreach (rec r in rectangles)
                {
                    r.SE = this.SE;
                    r.end = true;
                }
                finishedrecs.AddRange(rectangles.Where(r => r.end && r.SE >= r.SS));
            }

            public ruledivider(int SS, int SE, int DS, int DE)
            {
                this.SS = SS;
                this.SE = SE;
                this.DS = DS;
                this.DE = DE;
                this.rectangles.Add(new rec(SS, DS, DE));
            }

        }

    }


    class ETREE
    {
        public List<ETreeNode> nodes;

        public int RootNodeIndex;

        public ETREE()
        {
            this.nodes = new List<ETreeNode>();
            this.RootNodeIndex = -1;
        }

        /// <summary>
        /// 输入前后点，返回有重叠的线段所带的savedrule的index
        /// </summary>
        /// <param name="StartPoint"></param>
        /// <param name="EndPoint"></param>
        /// <returns></returns>
        public List<int> intervals(int StartPoint, int EndPoint)
        {
            return intervalboxes(StartPoint, EndPoint, this.RootNodeIndex);
        }

        private List<int> intervalboxes(int StartPoint, int EndPoint, int nodeindex)
        {//todo这里的顺序要调整，尽量做到从左向右
            List<int> result = new List<int>();
            if (nodeindex == -1 || this.nodes[nodeindex].maxEinSubtree < StartPoint)
                return result;
            if (this.nodes[nodeindex].StartPoint > EndPoint)
                return intervalboxes(StartPoint, EndPoint, this.nodes[nodeindex].ToRight);
            foreach (KeyValuePair<int, int> pair in this.nodes[nodeindex].Boxes)
                if (pair.Value > StartPoint)
                    result.Add(pair.Key);
            result.AddRange(intervalboxes(StartPoint, EndPoint, this.nodes[nodeindex].ToLeft));
            result.AddRange(intervalboxes(StartPoint, EndPoint, this.nodes[nodeindex].ToRight));
            return result;
        }

        public void addnode(int StartPoint, int EndPoint, int BoxIndex, out int NodeIndex)
        {
            if(this.RootNodeIndex==-1)
            {
                this.nodes.Add(new ETreeNode(StartPoint, EndPoint, BoxIndex));
                this.RootNodeIndex = 0;
                NodeIndex = 0;
                return;
            }
            int ParentIndex = searchforinsertion(StartPoint);
            if (this.nodes[ParentIndex].StartPoint == StartPoint)
            {
                this.nodes[ParentIndex].Boxes.Add(BoxIndex,EndPoint);
                this.nodes[ParentIndex].maxE = Math.Max(this.nodes[ParentIndex].maxE, EndPoint);//emmm这里写得好纠结
                this.nodes[ParentIndex].maxEinSubtree = Math.Max(this.nodes[ParentIndex].maxE, this.nodes[ParentIndex].maxEinSubtree);
                modifyLvalue(ParentIndex);
                NodeIndex = ParentIndex;
                return;
            }
            else
            {
                ETreeNode newnode = new ETreeNode(StartPoint, EndPoint, BoxIndex, this.nodes.Count(), ParentIndex);
                this.nodes.Add(newnode);
                if (this.nodes[ParentIndex].StartPoint > StartPoint)
                    this.nodes[ParentIndex].ToLeft = newnode.Index;
                else this.nodes[ParentIndex].ToRight = newnode.Index;
                this.nodes[newnode.Index].maxEinSubtree = this.nodes[newnode.Index].maxE;
                modifyLvalue(newnode.Index);
                NodeIndex = newnode.Index;
                return;
            }
        }

        public int searchforinsertion(int StartPoint)
        {
            if (this.nodes == null || this.nodes.Count() == 0)
                return -1;
            ETreeNode currentnode = this.nodes[this.RootNodeIndex];
            while (true)
            {
                if (currentnode.StartPoint == StartPoint)
                    return currentnode.Index;
                if (currentnode.StartPoint > StartPoint)
                    if (currentnode.ToLeft != -1)
                    {
                        currentnode = this.nodes[currentnode.ToLeft];
                        continue;
                    }
                    else
                        return currentnode.Index;
                if (currentnode.StartPoint < StartPoint)
                    if (currentnode.ToRight != -1)
                    {
                        currentnode = this.nodes[currentnode.ToRight];
                        continue;
                    }
                    else return currentnode.Index;
            }
        }

        private void modifyLvalue(int nodeindex)
        {
            int checkpoint = nodeindex;
            
            #region rotation
            if (this.nodes[nodeindex].From != -1 && this.nodes[this.nodes[nodeindex].From].From != -1)
            {
                int d = nodeindex;
                int m = this.nodes[nodeindex].From;
                int u = this.nodes[m].From;
                int uu = this.nodes[u].From;
                //如果是从上到下向右的树
                if (this.nodes[u].ToLeft == -1 && this.nodes[m].ToLeft == -1)
                {
                    //如果顶上不是
                    this.nodes[m].From = uu;
                    if (uu != -1)
                        if (this.nodes[uu].StartPoint > this.nodes[u].StartPoint)
                            this.nodes[uu].ToLeft = m;
                        else this.nodes[uu].ToRight = m;
                    else
                        this.RootNodeIndex = m;
                    this.nodes[u].ToRight = -1;
                    this.nodes[u].From = m;
                    this.nodes[m].ToLeft = u;
                    this.nodes[u].maxEinSubtree = this.nodes[u].maxE;
                    this.nodes[m].maxEinSubtree = Math.Max(Math.Max(this.nodes[d].maxEinSubtree, this.nodes[u].maxEinSubtree), this.nodes[m].maxE);
                    checkpoint = m;
                }
                //如果是从上到下向左的树
                if (this.nodes[u].ToRight == -1 && this.nodes[m].ToRight == -1)
                {
                    this.nodes[m].From = uu;
                    if (uu != -1)
                        if (this.nodes[uu].StartPoint > this.nodes[u].StartPoint)
                            this.nodes[uu].ToLeft = m;
                        else this.nodes[uu].ToRight = m;

                    else
                        this.RootNodeIndex = m;
                    this.nodes[u].ToLeft = -1;
                    this.nodes[u].From = m;
                    this.nodes[m].ToRight = u;
                    this.nodes[u].maxEinSubtree = this.nodes[u].maxE;
                    this.nodes[m].maxEinSubtree = Math.Max(Math.Max(this.nodes[d].maxEinSubtree, this.nodes[u].maxEinSubtree), this.nodes[m].maxE);
                    checkpoint = m;
                }
                //如果是从上到下先左后右的树
                if (this.nodes[u].ToRight == -1 && this.nodes[m].ToLeft == -1)
                {
                    this.nodes[d].From = uu;
                    if (uu != -1)
                        if (this.nodes[uu].StartPoint > this.nodes[u].StartPoint)
                            this.nodes[uu].ToLeft = d;
                        else this.nodes[uu].ToRight = d;

                    else
                        this.RootNodeIndex = d;

                    this.nodes[u].From = d;
                    this.nodes[u].ToLeft = -1;
                    this.nodes[u].maxEinSubtree = this.nodes[u].maxE;
                    this.nodes[m].From = d;
                    this.nodes[m].ToRight = -1;
                    this.nodes[m].maxEinSubtree = this.nodes[m].maxE;
                    this.nodes[d].ToLeft = m;
                    this.nodes[d].ToRight = u;
                    this.nodes[d].maxEinSubtree = Math.Max(Math.Max(this.nodes[m].maxEinSubtree, this.nodes[u].maxEinSubtree), this.nodes[d].maxE);
                    checkpoint = d;
                }
                //如果是从上到下先右后左的树
                if (this.nodes[u].ToLeft == -1 && this.nodes[m].ToRight == -1)
                {
                    this.nodes[d].From = uu;
                    if (uu != -1)
                        if (this.nodes[uu].StartPoint > this.nodes[u].StartPoint)
                            this.nodes[uu].ToLeft = d;
                        else this.nodes[uu].ToRight = d;
                    else
                        this.RootNodeIndex = d;
                    this.nodes[u].From = d;
                    this.nodes[u].ToRight = -1;
                    this.nodes[u].maxEinSubtree = this.nodes[u].maxE;
                    this.nodes[m].From = d;
                    this.nodes[m].ToLeft = -1;
                    this.nodes[m].maxEinSubtree = this.nodes[m].maxE;
                    this.nodes[d].ToRight = m;
                    this.nodes[d].ToLeft = u;
                    this.nodes[d].maxEinSubtree = Math.Max(Math.Max(this.nodes[m].maxEinSubtree, this.nodes[u].maxEinSubtree), this.nodes[d].maxE);
                    checkpoint = d;
                }
                
            }
            #endregion rotation

            int up = this.nodes[checkpoint].From;
            while (up != -1)
            {    
                if (this.nodes[up].maxEinSubtree >= this.nodes[checkpoint].maxEinSubtree)
                    break;
                else
                {
                    this.nodes[up].maxEinSubtree = this.nodes[checkpoint].maxEinSubtree;
                    checkpoint = up;
                    up = this.nodes[checkpoint].From;
                }
            }

        }

        /// <summary>
        /// 用于改变一个线段的起始时。todo还需要把checkvalue给向上复原一下，现在删掉的那个点的最大值还没动呢
        /// </summary>
        /// <param name="boxindex"></param>
        /// <param name="nodeindex"></param>
        /// <param name="changeto"></param>
        /// <returns></returns>
        public int changeStartPoint(int boxindex, int nodeindex, int changeto)
        {
            int newnodeindex = -1;
            int endpoint = this.nodes[nodeindex].Boxes.First(b => b.Key == boxindex).Value;
            this.nodes[nodeindex].Boxes.Remove(boxindex);
            //todo if node 空了要删除它
            addnode(changeto, endpoint, boxindex, out newnodeindex);
            return newnodeindex;
        }

        /// <summary>
        /// 用于改变一个线段的重点。todo这里还需要向上检查一下最大值，跟上面一个函数一起改吧
        /// </summary>
        /// <param name="boxindex"></param>
        /// <param name="nodeindex"></param>
        /// <param name="changeto"></param>
        public void changeEndPoint(int boxindex, int nodeindex, int changeto)
        {
            this.nodes[nodeindex].Boxes.Remove(boxindex);
            this.nodes[nodeindex].Boxes.Add(boxindex, changeto);
        }

        public class ETreeNode
        {
            public int StartPoint;
            public int maxE;
            public int maxEinSubtree;
            public int Index;
            public int ToLeft;
            public int ToRight;
            public int From;
            /// <summary>
            /// 前面是boxindex,后面是这段的endpoint。这样安排的原因是boxindex不会重复
            /// </summary>
            public Dictionary<int, int> Boxes;

            /// <summary>
            /// Generate the rootnode
            /// </summary>
            /// <param name="StartPoint"></param>
            /// <param name="EndPoint"></param>
            /// <param name="BoxIndex"></param>
            public ETreeNode(int StartPoint, int EndPoint, int BoxIndex)
            {
                this.StartPoint = StartPoint;
                this.maxE = EndPoint;
                this.Boxes = new Dictionary<int, int>();
                this.Boxes.Add(BoxIndex, EndPoint);
                this.maxEinSubtree = EndPoint;
                this.Index = 0;
                this.ToLeft = -1;
                this.ToRight = -1;
                this.From = -1;
            }
            public ETreeNode(int StartPoint, int EndPoint, int BoxIndex, int index, int ParentIndex)
            {
                this.StartPoint = StartPoint;
                this.maxE = EndPoint;
                this.Boxes = new Dictionary<int, int>();
                this.Boxes.Add(BoxIndex, EndPoint);
                this.Index = index;
                this.ToLeft = -1;
                this.ToRight = -1;
                this.From = ParentIndex;
            }
        }

        /// <summary>
        /// 找到从该点开始往后续的那些规则
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public List<int> connectedrecs(int endpoint)
        {
            return connectedrecs(this.RootNodeIndex, endpoint+1);
        }

        private List<int> connectedrecs(int index, int endpoint)
        {
            if (this.nodes[index].StartPoint == endpoint)
                return this.nodes[index].Boxes.Keys.ToList();
            if (this.nodes[index].StartPoint < endpoint && this.nodes[index].ToRight != -1)
                return connectedrecs(this.nodes[index].ToRight, endpoint);
            if (this.nodes[index].StartPoint > endpoint && this.nodes[index].ToLeft != -1)
                return connectedrecs(this.nodes[index].ToLeft, endpoint);
            return new List<int>();
        }
    }
}
