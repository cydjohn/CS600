using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPRulesRegulater
{
    class Structure
    { }

    class IPRange
    {
        public int FS;
        public int SS;
        public int TS;
        public int LS;
        public int FE;
        public int SE;
        public int TE;
        public int LE;
        public bool hasslash;
        public int slash;
        public int convertStart()
        {
            return 0;//todo添加转换规则
        }
        public int convertEnd()
        {
            return 0;//todo添加转换规则
        }

    }
    class rule
    {
        public IPRange SourceIP;

        public IPRange DestIP;

        public bool Allowed;

        //public int[] toArries()
        //{
        //    int[] res = new int[20];
        //    res[0] = this.SourceIP.FS;
        //    res[1] = this.SourceIP.SE;
        //    res[2] = 

        //}

    }

    class Box
    {
        List<savedrule> savedrules;

        public ETREE SourceIPTree;

        public ETREE ReverseSourceIPTree;

        public ETREE DestIPTree;//todo 不用public吧

        public void AddRule(rule newrule, int boxindex = 0, bool ischecking = false)
        {

            int SS = newrule.SourceIP.convertStart();
            int SE = newrule.SourceIP.convertEnd();
            int DS = newrule.DestIP.convertStart();
            int DE = newrule.DestIP.convertEnd();



            //检查有没有重复，先列出要重的
            List<int> Intervaled = FindIntevals(boxindex, SS, SE, DS, DE);

            //一一调整校对，得出需要添加的裂解的box，以及哪些需要留观后效
            ruledivider Newrules = ChecktheseBoxes(Intervaled, SS, SE, DS, DE);

            //先在两个sourceIP树里检索一遍
            foreach (rec r in Newrules.finishedrecs)
            {
                List<int> toright = this.SourceIPTree.connectedrecs(r.SE);
                toright = toright.Where(i => this.savedrules[i].DS <= r.DE && this.savedrules[i].DE >= r.DS && this.savedrules[i].allowed == newrule.Allowed).ToList();
                List<int> toleft = this.ReverseSourceIPTree.connectedrecs(r.SS);
                toleft = toleft.Where(i => this.savedrules[i].DS <= r.DE && this.savedrules[i].DE >= r.DS && this.savedrules[i].allowed == newrule.Allowed).ToList();
                if ((toright != null || toright.Count() != 0) && (toleft != null || toleft.Count() != 0))
                {
                    //todo
                    continue;
                }
                #region ifhaverightneighbor
                if (toright != null || toright.Count() != 0)
                {
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
                            this.savedrules[upperone].DestIPTIndex = this.DestIPTree.changeStartPoint(this.savedrules[upperone].index, this.savedrules[upperone].RSourceIPTIndex, r.DS + 1, this.savedrules[upperone].DE);
                        }
                        else
                            delete(upperone);
                        AddRule(r.SS, this.savedrules[upperone].SE, uppercheckpoint + 1, r.DE, newrule.Allowed);
                    }

                    if (lowerone != -1)
                    {
                        lowercheckpoint = this.savedrules[lowerone].DE + 1;
                        if (this.savedrules[lowerone].DS < r.DS)
                        {
                            this.savedrules[lowerone].DE = r.DS - 1;
                            this.DestIPTree.nodes[savedrules[lowerone].DestIPTIndex].Boxes.Remove(lowerone);
                            this.DestIPTree.nodes[savedrules[lowerone].DestIPTIndex].Boxes.Add(lowerone, r.DS - 1);
                        }
                        else
                            delete(lowerone);
                        AddRule(r.SS, this.savedrules[lowerone].SE, r.DS, lowercheckpoint - 1, newrule.Allowed);
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
                        AddRule(r.SS, this.savedrules[i].SE, a, b, newrule.Allowed);
                        delete(i);
                    }
                    foreach (line l in linesegs)
                        AddRule(r.SS, r.SE, l.startpoint, l.endpoint, newrule.Allowed);
                    continue;
                }
                #endregion ifhaverightneighbor

                if (toleft != null || toleft.Count() != 0)
                {
                    continue;
                }
                AddRule(r.SS, r.SE, r.DS, r.DE, newrule.Allowed);

            }
        }

        private void AddRule(int SS, int SE, int DS, int DE, bool allowed)
        {
            savedrule newr = new savedrule(SS, SE, DS, DE, allowed, this.savedrules.Count());
            int index = -1;
            this.SourceIPTree.addnode(SS, SE, newr.index, out index);
            newr.SourceIPTIndex = index;
            this.ReverseSourceIPTree.addnode(SE, SS, newr.index, out index);
            newr.RSourceIPTIndex = index;
            this.DestIPTree.addnode(DS, DE, newr.index, out index);
            newr.DestIPTIndex = index;
            this.savedrules.Add(newr);
        }

        private List<int> FindIntevals(int BoxIndex, int SS, int SE, int DS, int DE)
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
                if (this.savedrules[i].SS <= SS)
                    events.Add(new Event(i, SS, true));
                else events.Add(new Event(i, this.savedrules[i].SS, true));
                if (this.savedrules[i].SE <= SE)
                    events.Add(new Event(i, this.savedrules[i].SE, false));
                else events.Add(new Event(i, SE, false));
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
                this.SourceIPTree.nodes[this.savedrules[last].SourceIPTIndex].Boxes.Remove(last);
                this.ReverseSourceIPTree.nodes[this.savedrules[last].RSourceIPTIndex].Boxes.Remove(last);
                this.DestIPTree.nodes[this.savedrules[last].DestIPTIndex].Boxes.Remove(last);
                this.savedrules.RemoveAt(last);
                last--;
            }

            this.SourceIPTree.nodes[this.savedrules[index].SourceIPTIndex].Boxes.Remove(index);
            this.ReverseSourceIPTree.nodes[this.savedrules[index].RSourceIPTIndex].Boxes.Remove(index);
            this.DestIPTree.nodes[this.savedrules[index].DestIPTIndex].Boxes.Remove(index);

            this.savedrules[index].copy(this.savedrules[last]);

            this.SourceIPTree.nodes[this.savedrules[index].SourceIPTIndex].Boxes.Remove(last);
            this.SourceIPTree.nodes[this.savedrules[index].SourceIPTIndex].Boxes.Add(index, this.savedrules[index].SE);

            this.ReverseSourceIPTree.nodes[this.savedrules[index].RSourceIPTIndex].Boxes.Remove(last);
            this.ReverseSourceIPTree.nodes[this.savedrules[index].RSourceIPTIndex].Boxes.Add(index, this.savedrules[index].SS);

            this.DestIPTree.nodes[this.savedrules[index].DestIPTIndex].Boxes.Remove(last);
            this.DestIPTree.nodes[this.savedrules[index].DestIPTIndex].Boxes.Add(index, this.savedrules[index].DE);

            this.savedrules.RemoveAt(last);
        }

        class Box
        {
            public List<int> subBoxes;
            public ETREE SourceIPTree;
            public ETREE DestIPTree;
            public int Level;
            public int SS;
            public int SE;
            public int DS;
            public int DE;
            public List<int> From;
            public bool mark;

            public void add(rule newrule)
            {
                if (this.Level == 4) return;

            }



            public Box(int SS, int SE, int DS, int DE, int Level)
            {
                this.SS = SS;
                this.SE = SE;
                this.DS = DS;
                this.DE = DE;
                this.Level = Level;
                this.subBoxes = new List<int>();
                this.SourceIPTree = new ETREE();
                this.DestIPTree = new ETREE();
                this.mark = false;
            }

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

        class rec
        {
            public int SS;
            public int SE;
            public int DS;
            public int DE;
            public bool end;
            //public bool marked;
            public bool allowed;
            public rec(int SS, int DS, int DE)
            {
                this.SS = SS;
                this.DS = DS;
                this.DE = DE;
                this.end = false;
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


        class ruledivider
        {
            int SS;
            int SE;
            int DS;
            int DE;
            List<rec> rectangles;
            public List<rec> finishedrecs;

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

        public List<int> intervals(int StartPoint, int EndPoint)
        {
            return intervalboxes(StartPoint, EndPoint, this.RootNodeIndex);
        }

        public List<int> intervalboxes(int StartPoint, int EndPoint, int nodeindex)
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

        public void modifyLvalue(int nodeindex)
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

        public int changeStartPoint(int boxindex, int nodeindex, int changeto, int endpoint)
        {
            int newnodeindex = -1;
            this.nodes[nodeindex].Boxes.Remove(boxindex);
            //todo if node 空了要删除它
            addnode(changeto, endpoint, boxindex, out newnodeindex);
            return newnodeindex;
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

        public List<int> connectedrecs(int endpoint)
        {
            return connectedrecs(0, endpoint);
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
