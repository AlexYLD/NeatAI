using System;
using System.Collections.Generic;

namespace NeatNet.NEAT.BasicEntities
{
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

        public double getValue()
        {
            if (_isCalculated || Ancestors.Count == 0)
            {
                return OldValue;
            }

            double res = 0;

            foreach (KeyValuePair<Node, double> ancestor in Ancestors)
            {
                res += ancestor.Key.getValue() * ancestor.Value;
            }

            res = Math.Max(0, res);
            OldValue = res;
            _isCalculated = true;
            return res;
        }

        public void reset()
        {
            if (!_isCalculated || Ancestors.Count == 0)
            {
                return;
            }

            foreach (Node node in Ancestors.Keys)
            {
                node.reset();
            }

            _isCalculated = false;
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
    }
}