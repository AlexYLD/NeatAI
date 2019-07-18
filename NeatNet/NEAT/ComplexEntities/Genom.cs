using System;
using System.Collections.Generic;
using System.Linq;
using NeatNet.NEAT.BasicEntities;
using NeatNet.NEAT.Utils;

namespace NeatNet.NEAT.ComplexEntities
{
    [Serializable]
    public class Genom
    {
        public static int InnCount = 0; //Innovation count
        public static HashSet<Link> AllLinks = new HashSet<Link>();

        public List<Link> LocalLinks = new List<Link>();
        public List<Node> Inputs { get; set; } = new List<Node>();
        public List<Node> Hidden { get; set; } = new List<Node>();
        public List<Node> Outputs { get; set; } = new List<Node>();

        public Genom Clone(Genom parent)
        {
            return ObjectCopier.Clone(parent);
        }


        public void AddLink(Node input, Node output, double weight)
        {
            Link link = new Link(input, output, -1);
            Link oldLink = AllLinks.FirstOrDefault(l => l.Equals(link));
            if (oldLink == null)
            {
                link.InnNumber = InnCount++;
                output.Ancestors.Add(input, weight);
                AllLinks.Add(link);
                LocalLinks.Add(link);
                return;
            }

            link.InnNumber = oldLink.InnNumber;
            Link localLink = LocalLinks.Find(l => l.Equals(link));
            if (localLink == null)
            {
                output.Ancestors.Add(input, weight);
                LocalLinks.Add(link);
            }
            else
            {
                output.Ancestors[input] = weight;
                localLink.IsActive = true;
            }
        }

        private void DisableLink(Link link)
        {
            link.Output.Ancestors[link.Input] = 0;
            link.IsActive = false;
        }

        public void AddNode(Link link)
        {
            Node newNode = new Node(new NodeId(link.InnNumber, link.SplitCount++));
            AddLink(link.Input, newNode, 1);
            AddLink(newNode, link.Output, link.Output.Ancestors[link.Input]);
            DisableLink(link);
            Hidden.Add(newNode);
        }

        public void AddRandomLink(Random rnd)
        {
            List<Node> allNodes = new List<Node>();
            allNodes.AddRange(Inputs);
            allNodes.AddRange(Hidden);
            allNodes.AddRange(Outputs);
            Node input = allNodes[rnd.Next(allNodes.Count)];
            allNodes.RemoveAll(n => Inputs.Contains(n));
            Node output = allNodes[rnd.Next(allNodes.Count)];
            double weight = rnd.NextDouble() * 10 - 5;
            AddLink(input, output, weight);
        }

        public void AddRandomNode(Random rnd)
        {
            AddNode(LocalLinks[rnd.Next(LocalLinks.Count)]);
        }

        public Genom Mate(Genom other)
        {
            return null;
        }

        public double GetDifference(Genom other)
        {
            return 0;
        }
    }
}