using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatNet.NEAT.ComplexEntities
{
    public class Population
    {
        private const double DistThreshold = 3.0;
        private const double C1 = 1;
        private const double C2 = 1;
        private const double C3 = 0.4;
        private const int Amount = 150;
        public List<Genom> AllNets = new List<Genom>();
        Dictionary<Genom, List<Genom>> _species = new Dictionary<Genom, List<Genom>>();
        Random rnd = new Random();

        public Population(int inCount, int outCount)
        {
            for (int i = 0; i < Amount; i++)
            {
                Genom genom = new Genom(inCount, outCount);
                genom.AddRandomLink(rnd);
                AllNets.Add(genom);
            }
        }

        private void Speciate()
        {
            Dictionary<Genom, List<Genom>> newSpecies = new Dictionary<Genom, List<Genom>>();
            foreach (List<Genom> oldSpicie in _species.Values)
            {
                Genom representative = oldSpicie[rnd.Next(oldSpicie.Count)];
                newSpecies.Add(representative, new List<Genom>());
                newSpecies[representative].Add(representative);
            }

            bool isUniq = true;
            foreach (Genom genom in AllNets)
            {
                foreach (Genom specieRepresentative in newSpecies.Keys)
                {
                    if (genom.GetDistance(specieRepresentative, C1, C2, C3) < DistThreshold)
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

            _species = newSpecies;
        }

        public void NextGen()
        {
            Dictionary<List<Genom>, double> speciesFitness = new Dictionary<List<Genom>, double>();
            double totalSum = 0;
            foreach (List<Genom> spicie in _species.Values)
            {
                double speFitSum = 0;
                foreach (Genom genom in spicie)
                {
                    double adjFitness = CalcAdjFitness(genom);
                    genom.Fitness = adjFitness;
                    speFitSum += adjFitness;
                }

                totalSum += speFitSum;
                speciesFitness.Add(spicie, speFitSum);
            }

            List<Genom> newPopulation = new List<Genom>();

            foreach (List<Genom> spicie in speciesFitness.Keys)
            {
                foreach (Genom genom in spicie)
                {
                    int childrenCount = (int) Math.Round(speciesFitness[spicie] / totalSum * Amount * genom.Fitness /
                                                         speciesFitness[spicie]);
                    if (childrenCount == 0)
                    {
                        continue;
                    }

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
                            child = genom.Mate(spicie[rnd.Next(spicie.Count)], rnd);
                        }
                    }

                    newPopulation.Add(child);
                }
            }

            AllNets = newPopulation;
            Speciate();
        }

        private double CalcAdjFitness(Genom genom)
        {
            int count = AllNets.Count(g => g.GetDistance(genom, C1, C2, C3) < DistThreshold);
            return genom.Fitness / count;
        }
    }
}