using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPRulesRegulate
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

    class Tree
    {
        List<Box> boxes;

        public int RootBoxIndex;

        public void AddRule(rule newrule, int boxindex = 0, bool ischecking = false)
        {
            #region selectrange
            int SS = 0;
            int SE = 0;
            int DS = 0;
            int DE = 0;
            switch (this.boxes[boxindex].Level)
            {
                case 1:
                    SS = newrule.SourceIP.FS;
                    SE = newrule.SourceIP.FE;
                    DS = newrule.DestIP.FS;
                    DE = newrule.DestIP.FE;
                    break;
                case 2:
                    SS = newrule.SourceIP.SS;
                    SE = newrule.SourceIP.SE;
                    DS = newrule.DestIP.SS;
                    DE = newrule.DestIP.SE;
                    break;
                case 3:
                    SS = newrule.SourceIP.TS;
                    SE = newrule.SourceIP.TE;
                    DS = newrule.DestIP.TS;
                    DE = newrule.DestIP.TE;
                    break;
                case 4:
                    SS = newrule.SourceIP.LS;
                    SE = newrule.SourceIP.LE;
                    DS = newrule.DestIP.LS;
                    DE = newrule.DestIP.LE;
                    break;
                default:
                    SS = newrule.SourceIP.slash;
                    SE = newrule.SourceIP.slash;
                    DS = newrule.DestIP.slash;
                    DE = newrule.DestIP.slash;
                    break;
            }
            #endregion selecrange


            //检查有没有重复，先列出要重的
            List<int> Intervaled = FindIntevals(boxindex, SS, SE, DS, DE);

            //一一调整校对，得出需要添加的裂解的box，以及哪些需要留观后效



        }

        private List<int> FindIntevals(int BoxIndex, int SS, int SE, int DS, int DE)
        {
            List<int> SourceIntervals = this.boxes[BoxIndex].SourceIPTree.intervals(SS, SE);
            List<int> intervals = new List<int>();
            foreach (int ind in SourceIntervals)
                this.boxes[ind].mark = true;
            foreach (int ind in this.boxes[BoxIndex].DestIPTree.intervals(DS, DE))
                if (this.boxes[ind].mark)
                    intervals.Add(ind);
            foreach (int ind in SourceIntervals)
                this.boxes[ind].mark = false;
            return intervals;
        }

        private List<int> ChecktheseBoxes(List<int> boxes, int SS, int SE, int DS, int DE)
        {
            List<Event> events = new List<Event>();
            foreach (int i in boxes)
            {
                if (this.boxes[i].SS <= SS)
                    events.Add(new Event(i, SS, true));
                else events.Add(new Event(i, this.boxes[i].SS, true));
                if (this.boxes[i].SE <= SE)
                    events.Add(new Event(i, this.boxes[i].SE, false));
                else events.Add(new Event(i, SE, false));
            }
            events = events.OrderBy(e => e.time).ToList();
            recs rectangles = new recs(SE, SS, DS, DE);
            foreach (Event e in events)
            {
                if (e.start)
                    rectangles.addinterval(Math.Min(DE, this.boxes[e.index].DE), Math.Max(DS, this.boxes[e.index].DS), e.time);
                else
                    rectangles.deleteinterval(Math.Min(DE, this.boxes[e.index].DE), Math.Max(DS, this.boxes[e.index].DS), e.time);
            }
            rectangles.finish();
            return new List<int>();
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
            public int upper;
            public int lower;
            public int left;
            public int right;
            public bool end;
            public rec(int upper, int lower, int left)
            {
                this.upper = upper;
                this.lower = lower;
                this.left = left;
                this.end = false;
            }
        }

        class recs
        {
            int upper;
            int lower;
            int left;
            int right;
            List<rec> rectangles;
            List<rec> finishedrecs;

            public void addinterval(int u, int l, int time)
            {
                List<rec> newrecs = new List<rec>();
                foreach (rec r in rectangles)
                {
                    if (r.end || r.upper < l || r.left > u) continue;
                    r.right = time - 1;
                    r.end = true;
                    if (u < r.upper)
                        newrecs.Add(new rec(r.upper, u + 1, time));
                    if (l > r.lower)
                        newrecs.Add(new rec(l - 1, r.lower, time));
                }
                finishedrecs.AddRange(rectangles.Where(r => r.end && r.right >= r.left));
                rectangles.AddRange(newrecs);
                rectangles = rectangles.Where(r => !r.end).ToList();
            }

            public void deleteinterval(int u, int l, int time)
            {
                rectangles.Add(new rec(u, l, time+1));
            }

            public void finish()
            {
                foreach (rec r in rectangles)
                {
                    r.right = right;
                    r.end = true;
                }
                finishedrecs.AddRange(rectangles.Where(r => r.end && r.right >= r.left));
            }

            public recs(int upper, int lower, int left, int right)
            {
                this.upper = upper;
                this.lower = lower;
                this.left = left;
                this.right = right;
            }

        }

    }




    class ETREE
    {
        private List<ETreeNode> nodes;

        private int RootNodeIndex;

        public ETREE()
        {
            this.nodes = new List<ETreeNode>();
            this.RootNodeIndex = -1;
        }

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

        public void addnode(int StartPoint, int EndPoint, int BoxIndex)
        {
            if(this.RootNodeIndex==-1)
            {
                this.nodes.Add(new ETreeNode(StartPoint, EndPoint, BoxIndex));
                this.RootNodeIndex = 0;
                return;
            }
            int ParentIndex = searchforinsertion(StartPoint);
            if (this.nodes[ParentIndex].StartPoint == StartPoint)
            {
                this.nodes[ParentIndex].Boxes.Add(BoxIndex,EndPoint);
                this.nodes[ParentIndex].maxE = Math.Max(this.nodes[ParentIndex].maxE, EndPoint);//emmm这里写得好纠结
                this.nodes[ParentIndex].maxEinSubtree = Math.Max(this.nodes[ParentIndex].maxE, this.nodes[ParentIndex].maxEinSubtree);
                modifyLvalue(ParentIndex);
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
            }
        }

        private int searchforinsertion(int StartPoint)
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

        private class ETreeNode
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

    }



}
