using Business.Base;
using Business.Extensions;
using Business.Extensions.ValueTypes;
using Contracts.Entities.EvolutionaryAlgorithms.SimpleGA;
using Contracts.Entities.Instances;
using Contracts.Entities.Results;
using Contracts.Interfaces.Business;
using Contracts.Interfaces.Repository.Instances;
using CrossCutting.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Business.EvolutionaryAlgorithms
{
    public class SimpleGeneticAlgotithm : BaseBusiness
    {
        public void Execute(int instanceNumber, DateTime begin, DateTime end)
        {
            StringBuilder log = new StringBuilder();
            List<DeliveryOrderTrip> deliveryOrdersTrips = 
                GetDeliveriesOrdersWithDeliveryOrderTrips(instanceNumber, begin, end);
            Dictionary<int, Location> loadPlaces = 
                GetLoadPlacesWithVehicles(instanceNumber);
            Dictionary<string, double> distances = 
                ComputeDistances(log, instanceNumber, begin, end, deliveryOrdersTrips, loadPlaces);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Individual individual = GeneticAlgorithm(deliveryOrdersTrips, loadPlaces, distances);

            stopwatch.Stop();

            Console.WriteLine("MAXIMIZE PROFIT HEURISTIC");
            Console.WriteLine("{0,15} {1,15} {2,15}",
                "Total V.",
                "Tempo",
                "Lucro");
            Console.WriteLine("{0,15} {1,15} {2,15}",
                deliveryOrdersTrips.Count,
                stopwatch.Elapsed.TotalSeconds,
                individual.Fitness);

            File.WriteAllText(Directory.GetCurrentDirectory() + $"/Logs-BestLoadPlaceFit-{instanceNumber}.txt", "");
            File.AppendAllText(Directory.GetCurrentDirectory() + $"/Logs-BestLoadPlaceFit-{instanceNumber}.txt", log.ToString());
            log.Clear();

            stopwatch.Stop();
        }

        public Individual GeneticAlgorithm(
            List<DeliveryOrderTrip> deliveryOrdersTrips,
            Dictionary<int, Location> loadPlaces, 
            Dictionary<string, double> distances)
        {
            int populationSize = 10;
            List<Individual> population = InitializePopulation(deliveryOrdersTrips, loadPlaces, distances);
            int maximuNumberOfGenerations = 100;

            foreach (Individual individual in population)
            {
                individual.Fitness = ComputeFitness(individual, distances);
            }

            int currentGeneration = 0;
            while(currentGeneration < maximuNumberOfGenerations)
            {
                List<Individual> parents = ParentsSelection(population);
                population = Mating(populationSize, parents, loadPlaces, distances);
                foreach (Individual individual in population)
                {
                    individual.Fitness = ComputeFitness(individual, distances);
                }
                currentGeneration++;
            }

            return population.OrderBy(i => i.Fitness).FirstOrDefault();
        }

        public List<Individual> Mating(int populationSize,
            List<Individual> parents, 
            Dictionary<int, Location> loadPlaces,
            Dictionary<string, double> distances)
        {
            Random random = new Random();
            while(parents.Count < populationSize)
            {
                Individual newIndividual = Clone.DeepClone<Individual>(parents[random.Next(0, 4)]);
                newIndividual = Mutation(newIndividual, loadPlaces, distances);
                parents.Add(newIndividual);
            }

            return parents;
        }

        public Individual Mutation(Individual individual, 
            Dictionary<int, Location> loadPlaces,
            Dictionary<string, double> distances)
        {
            Random random = new Random();
            foreach (Gene gene in individual.Chromosome.Genes)
            {
                double probability = random.NextDouble();
                if (probability < 0.2)
                {
                    int newLoadPlaceId = loadPlaces.Values.ToList()[random.Next(0, loadPlaces.Count)].LocationId;

                    double distance;
                    distances.TryGetValue(newLoadPlaceId.Format(gene.LocationId), out distance);

                    double loadDuration = 1 * gene.Volume;
                    DateTime initialLoadTime = gene.RequestedTime.
                        Subtract(TimeSpan.FromMinutes(distance + 10)).
                        Subtract(TimeSpan.FromMinutes(5)).
                        Subtract(TimeSpan.FromMinutes(loadDuration));
                    DateTime finalLoadTime = initialLoadTime.
                        Add(TimeSpan.FromMinutes(gene.Volume * 1));
                    DateTime departureTimeFromLoadPlace = finalLoadTime.
                        Add(TimeSpan.FromMinutes(5));
                    DateTime arrivalTimeAtConstruction = departureTimeFromLoadPlace.
                        Add(TimeSpan.FromMinutes(distance)).
                        Add(TimeSpan.FromMinutes(5));
                    DateTime initialUnloadTimeAtConstruction =
                        arrivalTimeAtConstruction.Add(TimeSpan.FromMinutes(5));
                    DateTime finalUnloadTimeAtConstruction =
                        initialUnloadTimeAtConstruction.Add(TimeSpan.FromMinutes(20));
                    DateTime fepartureTimeFromConstruction =
                        finalUnloadTimeAtConstruction.Add(TimeSpan.FromMinutes(5));
                    DateTime arrivalTimeAtLoadPlace =
                        finalUnloadTimeAtConstruction.Add(TimeSpan.FromMinutes(distance));

                    gene.LoadPlaceId = newLoadPlaceId;
                    gene.Begin = initialLoadTime;
                    gene.End = arrivalTimeAtLoadPlace; 
                }
            }
            return individual;
        }

        public List<Individual> ParentsSelection(List<Individual> population)
        {
            return population.OrderBy(p => p.Fitness).Take(4).ToList();
        }

        public List<Individual> InitializePopulation(
            List<DeliveryOrderTrip> deliveryOrdersTrips,
            Dictionary<int, Location> loadPlaces,
            Dictionary<string, double> distances)
        {
            int populationSize = 10;
            List<Individual> pupulation = new List<Individual>();
            for (int i = 0; i < populationSize; i++)
            {
                pupulation.Add(new Individual());
            }

            foreach (Individual individual in pupulation)
            {
                foreach (DeliveryOrderTrip deliveryOrderTrip in deliveryOrdersTrips)
                {
                    double minDistance = double.MaxValue;
                    Location loadPlaceMinDistance = null;
                    double currentDistance;
                    foreach (KeyValuePair<int, Location> loadPlace in loadPlaces)
                    {
                        if (distances.TryGetValue(
                            deliveryOrderTrip.Construction.LocationId.Format(
                                loadPlace.Value.LocationId),
                                out currentDistance))
                        {
                            if (currentDistance < minDistance)
                            {
                                minDistance = currentDistance;
                                loadPlaceMinDistance = loadPlace.Value;
                            }
                        }
                    }

                    DateTime initialLoadTime = deliveryOrderTrip.GetBestInitialLoadTime(loadPlaceMinDistance, 
                        TimeSpan.FromMinutes(minDistance + 10));
                    DateTime finalLoadTime = initialLoadTime.
                        Add(TimeSpan.FromMinutes(deliveryOrderTrip.Volume * loadPlaceMinDistance.RateRMCProduction));
                    DateTime departureTimeFromLoadPlace = finalLoadTime.
                        Add(TimeSpan.FromMinutes(5));
                    DateTime arrivalTimeAtConstruction = departureTimeFromLoadPlace.
                        Add(TimeSpan.FromMinutes(minDistance)).
                        Add(TimeSpan.FromMinutes(5));
                    DateTime initialUnloadTimeAtConstruction =
                        arrivalTimeAtConstruction.Add(TimeSpan.FromMinutes(5));
                    DateTime finalUnloadTimeAtConstruction =
                        initialUnloadTimeAtConstruction.Add(TimeSpan.FromMinutes(20));
                    DateTime fepartureTimeFromConstruction =
                        finalUnloadTimeAtConstruction.Add(TimeSpan.FromMinutes(5));
                    DateTime arrivalTimeAtLoadPlace =
                        finalUnloadTimeAtConstruction.Add(TimeSpan.FromMinutes(minDistance));

                    Gene gene = new Gene();
                    gene.DeliveryOrderTripId = deliveryOrderTrip.DeliveryOrderTripId;
                    gene.Begin = initialLoadTime;
                    gene.End = arrivalTimeAtLoadPlace;
                    gene.VehicleId = 0;
                    gene.LoadPlaceId = loadPlaceMinDistance.LocationId;
                    gene.Volume = deliveryOrderTrip.Volume;
                    gene.RequestedTime = deliveryOrderTrip.RequestedTime;
                    gene.Income = deliveryOrderTrip.Income;
                    gene.RMCCost = deliveryOrderTrip.RMCCost;
                    gene.LocationId = deliveryOrderTrip.Construction.LocationId;

                    individual.Chromosome.Genes.Add(gene);
                }
            }

            return pupulation;
        }

        public decimal ComputeFitness(
            Individual individual,
            Dictionary<string, double> distances)
        {
            decimal totalIncome = 0;

            foreach(Gene gene in individual.Chromosome.Genes)
            {
                double distance;
                distances.TryGetValue(
                    gene.LocationId.Format(gene.LoadPlaceId),
                        out distance);
                decimal cost =
                        (decimal)(distance * 0.1d) +
                        (decimal)(2 * (distance / 4d) * 4d);

                totalIncome += gene.Income - cost - gene.RMCCost;
            }

            return totalIncome;
        }

        public SimpleGeneticAlgotithm(
            IDeliveryOrderRepository _deliveryOrderRepository,
            ILoadPlacesRepository _loadPlacesRepository) : 
            base(_deliveryOrderRepository, _loadPlacesRepository)
        {

        }
    }
}
