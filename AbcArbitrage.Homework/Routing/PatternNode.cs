using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbcArbitrage.Homework.Routing
{
    // Tree node 
    public class PatternNode
    {
        public Dictionary<string, PatternNode> Children = new Dictionary<string, PatternNode>();
        public bool IsEndOfPattern = false;
        public int ShortestDepth = int.MaxValue; // Initialize with a large value
    }
}
