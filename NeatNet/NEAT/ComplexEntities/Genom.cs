using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public List<Node> LocalNodes = new List<Node>();
        public List<Node> Inputs { get; set; } = new List<Node>();
        public List<Node> Hidden { get; set; } = new List<Node>();
        public List<Node> Outputs { get; set; } = new List<Node>();

        public double Fitness = 0;

        public Genom(int inputCount, int outputCount)
        {
            for (int i = 0; i < inputCount; i++)
            {
                Node input = new Node(new NodeId(-2, i));
                Inputs.Add(input);
                LocalNodes.Add(input);
            }

            for (int i = 0; i < outputCount; i++)
            {
                Node output = new Node(new NodeId(-1, i));
                Outputs.Add(output);
                LocalNodes.Add(output);
            }
        }

        public Genom Clone()
        {
            Genom clone = ObjectCopier.Clone(this);
            clone.Fitness = 0;
            return clone;
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
            }
        }

        private void DisableLink(Link link)
        {
            link.Output.Ancestors[link.Input] = 0;
        }

        public void AddNode(Link link)
        {
            Node newNode = new Node(new NodeId(link.InnNumber, link.SplitCount++));
            AddLink(link.Input, newNode, 1);
            AddLink(newNode, link.Output, link.Output.Ancestors[link.Input]);
            DisableLink(link);
            Hidden.Add(newNode);
            LocalNodes.Add(newNode);
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
            if (input.Equals(output))
            {
                return;
            }

            double weight = rnd.NextDouble() * 2 - 1;
            AddLink(input, output, weight);
        }

        public void AddRandomNode(Random rnd)
        {
            AddNode(LocalLinks[rnd.Next(LocalLinks.Count)]);
        }

        public void MutateWeights(Random rnd)
        {
            foreach (Link link in LocalLinks)
            {
                if (link.Output.Ancestors[link.Input] != 0)
                {
                    if (rnd.Next(100) < 90)
                    {
                        link.Output.Ancestors[link.Input] += rnd.NextDouble() - 0.1;
                    }
                    else
                    {
                        link.Output.Ancestors[link.Input] = rnd.NextDouble() * 2 - 1;
                    }
                }
            }
        }

        public Genom Mate(Genom other, Random rnd)
        {
            Genom child = new Genom(Inputs.Count, Outputs.Count);

            bool thisGeneBetter = Fitness > other.Fitness;
            List<Link> bestGenes = thisGeneBetter ? LocalLinks : other.LocalLinks;
            List<Link> worstGenes = !thisGeneBetter ? LocalLinks : other.LocalLinks;
            if (Fitness == other.Fitness)
            {
                worstGenes.ForEach(g =>
                {
                    if (!bestGenes.Contains(g)) bestGenes.Add(g);
                });
            }

            foreach (Link link in bestGenes)
            {
                Node input = new Node(link.Input.Id);
                if (!child.LocalNodes.Contains(input))
                {
                    child.Hidden.Add(input);
                    child.LocalNodes.Add(input);
                }
                else
                {
                    input = child.LocalNodes.Find(n => n.Equals(input));
                }

                Node output = new Node(link.Output.Id);
                if (!child.LocalNodes.Contains(output))
                {
                    child.Hidden.Add(output);
                    child.LocalNodes.Add(output);
                }
                else
                {
                    output = child.LocalNodes.Find(n => n.Equals(output));
                }

                double weight = link.Output.Ancestors[link.Input];
                if (rnd.Next(2) == 0 && worstGenes.Contains(link))
                {
                    Link worseLink = worstGenes.Find(l => l.Equals(link));
                    weight = worseLink.Output.Ancestors[worseLink.Input];
                }

                child.AddLink(input, output, weight);
            }

            return child;
        }

        public double GetDistance(Genom other, double c1, double c2, double c3)
        {
            double N = 1;
            double delW = 0;
            int match = 0;
            int exc = 0;
            int dis = 0;

            List<Link> locLinks = new List<Link>(LocalLinks);
            List<Link> otherLocLinks = new List<Link>(other.LocalLinks);

            locLinks.Sort(Link.InnNumberComparer);
            otherLocLinks.Sort(Link.InnNumberComparer);
            N = locLinks.Count > otherLocLinks.Count ? locLinks.Count : otherLocLinks.Count;
            int biggestLocalInn = locLinks[locLinks.Count - 1].InnNumber;
            int biggestOtherInn = otherLocLinks[otherLocLinks.Count - 1].InnNumber;
            int smallestBiggestInn = Math.Min(biggestLocalInn, biggestOtherInn);

            List<Link> listToTrim = biggestLocalInn > biggestOtherInn ? locLinks : otherLocLinks;
            while (listToTrim.Count > 0 && listToTrim[listToTrim.Count - 1].InnNumber > smallestBiggestInn)
            {
                listToTrim.RemoveAt(listToTrim.Count - 1);
                exc++;
            }

            for (int i = 0; i < locLinks.Count; i++)
            {
                Link otherLink = otherLocLinks.Find(l => l.InnNumber == locLinks[i].InnNumber);
                if (otherLink != null)
                {
                    match++;
                    delW += Math.Abs(locLinks[i].Output.Ancestors[locLinks[i].Input] -
                                     otherLink.Output.Ancestors[otherLink.Input]);
                    locLinks.RemoveAt(i);
                    otherLocLinks.Remove(otherLink);
                    i--;
                }
            }

            dis = locLinks.Count + otherLocLinks.Count;
            if (match == 0)
            {
                match = 1;
            }

            return (c1 * exc) / N + (c2 * dis) / N + c3 * (delW / match);
        }
    }
}