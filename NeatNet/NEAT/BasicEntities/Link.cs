using System;
using System.Collections.Generic;

namespace NeatNet.NEAT.BasicEntities
{
    [Serializable]
    public class Link
    {
        public Node Input { get; set; }
        public Node Output { get; set; }
        public int InnNumber;
        public int SplitCount = 0;

        public Link(Node input, Node output, int innNumber)
        {
            Input = input;
            Output = output;
            InnNumber = innNumber;
        }

        protected bool Equals(Link other)
        {
            return Equals(Input, other.Input) && Equals(Output, other.Output);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Link) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Input != null ? Input.GetHashCode() : 0) * 397) ^ (Output != null ? Output.GetHashCode() : 0);
            }
        }

        private sealed class InnNumberRelationalComparer : IComparer<Link>
        {
            public int Compare(Link x, Link y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.InnNumber.CompareTo(y.InnNumber);
            }
        }

        public static IComparer<Link> InnNumberComparer { get; } = new InnNumberRelationalComparer();
    }
}