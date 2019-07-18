namespace NeatNet.NEAT.BasicEntities
{
    public class NodeId
    {
        public int InnNumber { get; }
        public int SplitNumber { get; }

        public NodeId(int innNumber, int splitNumber)
        {
            InnNumber = innNumber;
            SplitNumber = splitNumber;
        }

        protected bool Equals(NodeId other)
        {
            return InnNumber == other.InnNumber && SplitNumber == other.SplitNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NodeId) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (InnNumber * 397) ^ SplitNumber;
            }
        }
    }
}