using System;
using System.Collections.Generic;
using System.Linq;
using NeatNet.NEAT.BasicEntities;
using NeatNet.NEAT.Utils;

namespace NeatNet.NEAT.ComplexEntities
{
    public class Population
    {
        private const double DistThreshold = 3;
        private const double C1 = 1;
        private const double C2 = 1;
        private const double C3 = 3;
        private const int Amount = 150;
        public List<Genom> AllNets = new List<Genom>();
        public Dictionary<Genom, List<Genom>> _species = new Dictionary<Genom, List<Genom>>();
        Random rnd = new Random();

        public Population(int inCount, int outCount)
        {
            for (int i = 0; i < Amount; i++)
            {
                Genom genom = new Genom(inCount, outCount);
                foreach (Node input in genom.Inputs)
                {
                    foreach (Node output in genom.Outputs)
                    {
                        genom.AddLink(input, output, rnd.NextDouble() * 2 - 1);
                    }
                }

                AllNets.Add(genom);
            }

            Speciate();
        }

        private void Speciate()
        {
            Dictionary<Genom, List<Genom>> newSpecies = new Dictionary<Genom, List<Genom>>();
            foreach (List<Genom> oldSpicie in _species.Values)
            {
                if (oldSpicie.Count == 0)
                {
                    continue;
                }

                Genom representative = oldSpicie[rnd.Next(oldSpicie.Count)];
                newSpecies.Add(representative, new List<Genom>());
                //newSpecies[representative].Add(representative);
            }

            bool isUniq;
            foreach (Genom genom in AllNets)
            {
                isUniq = true;
                foreach (Genom specieRepresentative in newSpecies.Keys)
                {
                    if (genom.GetDistance(specieRepresentative, C1, C2, C3) <= DistThreshold)
                    {
                        newSpecies[specieRepresentative].Add(genom);
                        isUniq = false;
                        break;
                    }
                }

                if (isUniq)
                {
                    newSpecies.Add(genom, new List<Genom>());
                    newSpecies[genom].Add(genom);
                }
            }

            _species = new Dictionary<Genom, List<Genom>>(newSpecies);
            foreach (Genom key in newSpecies.Keys)
            {
                if (newSpecies[key].Count == 0)
                {
                    _species.Remove(key);
                }
            }
        }

        public Genom GetBest(List<Genom> group)
        {
            Genom max = new Genom(0, 0);
            max.Fitness = 0;
            foreach (Genom genom in group)
            {
                if (genom.Fitness > max.Fitness)
                {
                    max = genom;
                }
            }

            return max;
        }

        public void NextGen()
        {
            double totalSum = 0;
            List<Genom> newPopulation = new List<Genom>();
            List<Genom> bests = new List<Genom>();
            foreach (List<Genom> specie in _species.Values)
            {
                specie.Sort((g, g1) =>
                {
                    if (g.Fitness > g1.Fitness) return -1;
                    if (g.Fitness < g1.Fitness) return 1;
                    return 0;
                });
                if (specie.Count >= 5)
                {
                    bests.Add(specie[0]);
                }


                for (int i = 0; i <= specie.Count / 2; i++)
                {
                    double adjFitness = specie[i].Fitness / specie.Count;
                    specie[i].Fitness = adjFitness;
                    totalSum += adjFitness;
                }

                int specieCount = specie.Count;
                for (int i = specieCount - 1; i > specieCount / 2; i--)
                {
                    AllNets.Remove(specie[i]);
                    specie.RemoveAt(i);
                }
            }

            foreach (List<Genom> specie in _species.Values)
            {
                foreach (Genom genom in specie)
                {
                    int childrenCount =
                        (int) Math.Round(Amount * (genom.Fitness / totalSum));

                    if (bests.Contains(genom))
                    {
                        childrenCount--;
                        genom.Fitness = 0;
                        newPopulation.Add(genom);
                    }

                    if (childrenCount <= 0)
                    {
                        continue;
                    }


                    for (int i = 0; i < childrenCount; i++)
                    {
                        Genom child;
                        if (rnd.NextDouble() <= 0.25)
                        {
                            child = genom.Clone();
                            if (rnd.NextDouble() <= 0.8)
                            {
                                child.MutateWeights(rnd);
                            }

                            if (rnd.NextDouble() <= 0.03)
                            {
                                child.AddRandomNode(rnd);
                            }

                            if (rnd.NextDouble() <= 0.05)
                            {
                                child.AddRandomLink(rnd);
                            }
                        }
                        else
                        {
                            if (rnd.NextDouble() < 0.001)
                            {
                                child = genom.Mate(AllNets[rnd.Next(AllNets.Count)], rnd);
                            }
                            else
                            {
                                child = genom.Mate(specie[rnd.Next(specie.Count)], rnd);
                            }
                        }

                        newPopulation.Add(child);
                    }
                }
            }

            AllNets = newPopulation;
            Speciate();
        }

        private double CalcAdjFitness(Genom genom)
        {
            int count = AllNets.Count(g => g.GetDistance(genom, C1, C2, C3) <= DistThreshold);


            return genom.Fitness / count;
        }
    }
}