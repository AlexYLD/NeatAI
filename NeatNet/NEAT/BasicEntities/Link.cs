namespace NeatNet.NEAT.BasicEntities
{
    public class Link
    {
        public Node Input { get; set; }
        public Node Output { get; set; }
        public int InnNumber;
        public int SplitCount = 0;
        public bool IsActive = true;

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
        
    }
}