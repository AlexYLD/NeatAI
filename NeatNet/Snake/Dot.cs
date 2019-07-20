using System.Collections.Generic;
using System.Windows.Controls;

namespace NeatNet.Snake
{
    public class Dot
    {
        public int X;
        public int Y;
        public Label Label = null;
        public DotStatus status;

        public Dot(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Dot(int x, int y, Label label)
        {
            X = x;
            Y = y;
            Label = label;
        }

        protected bool Equals(Dot other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Dot) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        
    }

    public enum DotStatus
    {
        Field,
        Wall,
        Snake,
        Apple
    }
}