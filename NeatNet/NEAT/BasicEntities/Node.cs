using System;
using System.Collections.Generic;

namespace NeatNet.NEAT.BasicEntities
{
    [Serializable]
    public class Node
    {
        public double OldValue { get; set; } = 0;
        public Dictionary<Node, double> Ancestors { get; }
        private bool _isCalculated;

        public NodeId Id { get; set; }

        public Node(NodeId id)
        {
            Id = id;
            Ancestors = new Dictionary<Node, double>();
        }

        public double GetValue()
        {
            if (_isCalculated || Ancestors.Count == 0)
            {
                return OldValue;
            }

            double res = 0;
            _isCalculated = true;

            foreach (KeyValuePair<Node, double> ancestor in Ancestors)
            {
                if (ancestor.Value == 0)
                {
                    continue;
                }

                res += ancestor.Key.GetValue() * ancestor.Value;
            }

            res = Math.Max(0, res);
            OldValue = res;
            return res;
        }

        public void reset()
        {
            if (!_isCalculated || Ancestors.Count == 0)
            {
                return;
            }
            _isCalculated = false;

            foreach (Node node in Ancestors.Keys)
            {
                node.reset();
            }

           
        }

        protected bool Equals(Node other)
        {
            return Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Node) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}";
        }
    }
}