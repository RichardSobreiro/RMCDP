using Business.Base;
using Business.Extensions;
using Business.Extensions.ValueTypes;
using Contracts.Entities.EvolutionaryAlgorithms.MaghrebiWallerSammut;
using Contracts.Entities.Instances;
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
    public class MaghrebiWallerSammut : BaseBusiness
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

            Individual individual = Fase1(deliveryOrdersTrips, loadPlaces, distances);

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

        public Individual Fase2(Individual individual,
            List<DeliveryOrderTrip> deliveryOrdersTrips,
            Dictionary<int, Location> loadPlaces,
            Dictionary<string, double> distances)
        {
            int populationSize = 10;
            int maximuNumberOfGenerations = 500;
            List<Individual> population = InitializePopulationFase2(individual, populationSize);

            foreach (Individual ind in population)
            {
                ind.Fitness = ComputeFitness(ind, distances);
            }

            int currentGeneration = 0;
            while (currentGeneration < maximuNumberOfGenerations)
            {
                List<Individual> parents = ParentsSelection(population);



                currentGeneration++;
            }
        }

        public List<Individual> MatingFase2(int populationSize,
            List<Individual> parents,
            Dictionary<int, Location> loadPlaces,
            Dictionary<string, double> distances)
        {
            Random random = new Random();
            while (parents.Count < populationSize)
            {
                Individual newIndividual = Clone.DeepClone<Individual>(parents[random.Next(0, 4)]);
                newIndividual = MutationFase2(newIndividual, loadPlaces, distances);
                parents.Add(newIndividual);
            }

            return parents;
        }

        public Individual MutationFase2(Individual individual,
            Dictionary<int, Location> loadPlaces,
            Dictionary<string, double> distances)
        {
            Random random = new Random();
            foreach (LoadPlaceGene loadPlaceGene in individual.Chromosome.LoadPlaceGenes)
            {
                double probability = random.NextDouble();
                if (probability < 0.2)
                {
                    int newLoadPlaceId = loadPlaces.Values.ToList()[random.Next(0, loadPlaces.Count)].LocationId;

                    double distance;
                    distances.TryGetValue(newLoadPlaceId.Format(loadPlaceGene.LocationId), out distance);

                    double loadDuration = 1 * loadPlaceGene.Volume;
                    DateTime initialLoadTime = loadPlaceGene.RequestedTime.
                        Subtract(TimeSpan.FromMinutes(distance + 10)).
                        Subtract(TimeSpan.FromMinutes(5)).
                        Subtract(TimeSpan.FromMinutes(loadDuration));
                    DateTime finalLoadTime = initialLoadTime.
                        Add(TimeSpan.FromMinutes(loadPlaceGene.Volume * 1));
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

                    loadPlaceGene.LoadPlaceId = newLoadPlaceId;
                    loadPlaceGene.Begin = initialLoadTime;
                    loadPlaceGene.End = arrivalTimeAtLoadPlace;

                    VehicleGene vehicleGene = null;
                    foreach (VehicleGene vg in individual.Chromosome.VehicleGenes)
                    {
                        int indexBeginTrip = 0;
                        int indexEndTrip = 0;
                        bool windowFinded = false;
                        foreach(DateTime begin in vg.BeginLastTrip)
                        {
                            if(begin < initialLoadTime)
                            {
                                foreach (DateTime end in vg.EndLastTrip)
                                {
                                    if(end > arrivalTimeAtLoadPlace && indexEndTrip == (indexBeginTrip + 1))
                                    {
                                        windowFinded = true;
                                        break;
                                    }
                                    indexEndTrip++;
                                }
                            }
                            if (windowFinded)
                            {
                                break;
                            }
                            indexBeginTrip++;
                        }
                    }

                    //VehicleGene vehicleGene = individual.Chromosome.VehicleGenes.FirstOrDefault(g =>
                    //        g.VehicleId != 0 &&
                    //        g.LoadPlaceId == loadPlaceGene.LoadPlaceId &&
                    //        g.EndLastTrip.Last() < loadPlaceGene.Begin
                    //    );
                    if (vehicleGene != null)
                    {
                        loadPlaceGene.VehicleId = vehicleGene.;

                    }
                    else
                    {
                        vehicleGene = new VehicleGene();
                        vehicleGene.BeginLastTrip.Add(loadPlaceGene.Begin);
                        vehicleGene.EndLastTrip.Add(loadPlaceGene.End);
                        vehicleGene.VehicleId = individual.FleetSize++;
                        vehicleGene.LoadPlaceId = loadPlaceGene.LoadPlaceId;
                        individual.Chromosome.VehicleGenes.Add(vehicleGene);
                    }
                }
            }
            return individual;
        }

        public List<Individual> InitializePopulationFase2(Individual individual, int populationSize)
        {
            List<Individual> population = new List<Individual>();
            for (int i = 0; i < populationSize; i++)
            {
                population.Add(Clone.DeepClone(individual));
            }
            int vehicleReference = 1;
            foreach(Individual individual1 in population)
            {
                foreach(LoadPlaceGene loadPlaceGene in individual1.Chromosome.LoadPlaceGenes)
                {
                    int? vehicleId = individual1.Chromosome.VehicleGenes.FirstOrDefault(g =>
                            g.VehicleId != 0 &&
                            g.LoadPlaceId == loadPlaceGene.LoadPlaceId &&
                            g.EndLastTrip.Last() < loadPlaceGene.Begin
                        )?.VehicleId;
                    if (vehicleId.HasValue)
                    {
                        loadPlaceGene.VehicleId = vehicleId.Value;
                    }
                    else
                    {
                        VehicleGene vehicleGene = new VehicleGene();
                        vehicleGene.BeginLastTrip.Add(loadPlaceGene.Begin);
                        vehicleGene.EndLastTrip.Add(loadPlaceGene.End);
                        vehicleGene.VehicleId = vehicleReference;
                        vehicleGene.LoadPlaceId = loadPlaceGene.LoadPlaceId;
                        individual1.Chromosome.VehicleGenes.Add(vehicleGene);
                        vehicleReference++;
                    }
                }
                individual1.FleetSize = vehicleReference;
            }
            return population;
        }

        public Individual Fase1(
            List<DeliveryOrderTrip> deliveryOrdersTrips,
            Dictionary<int, Location> loadPlaces,
            Dictionary<string, double> distances)
        {
            int populationSize = 10;
            List<Individual> population = InitializePopulationFase1(deliveryOrdersTrips, loadPlaces, distances);
            int maximuNumberOfGenerations = 500;

            foreach (Individual individual in population)
            {
                individual.Fitness = ComputeFitness(individual, distances);
            }

            int currentGeneration = 0;
            while (currentGeneration < maximuNumberOfGenerations)
            {
                List<Individual> parents = ParentsSelection(population);
                population = MatingFase1(populationSize, parents, loadPlaces, distances);
                foreach (Individual individual in population)
                {
                    individual.Fitness = ComputeFitness(individual, distances);
                }
                currentGeneration++;
            }

            return population.OrderBy(i => i.Fitness).FirstOrDefault();
        }

        public List<Individual> MatingFase1(int populationSize,
            List<Individual> parents,
            Dictionary<int, Location> loadPlaces,
            Dictionary<string, double> distances)
        {
            Random random = new Random();
            while (parents.Count < populationSize)
            {
                Individual newIndividual = Clone.DeepClone<Individual>(parents[random.Next(0, 4)]);
                newIndividual = MutationFase1(newIndividual, loadPlaces, distances);
                parents.Add(newIndividual);
            }

            return parents;
        }

        public Individual MutationFase1(Individual individual,
            Dictionary<int, Location> loadPlaces,
            Dictionary<string, double> distances)
        {
            Random random = new Random();
            foreach (LoadPlaceGene gene in individual.Chromosome.LoadPlaceGenes)
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

        public List<Individual> InitializePopulationFase1(
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

                    LoadPlaceGene gene = new LoadPlaceGene();
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

                    individual.Chromosome.LoadPlaceGenes.Add(gene);
                }
            }

            return pupulation;
        }

        public decimal ComputeFitness(
            Individual individual,
            Dictionary<string, double> distances)
        {
            decimal totalIncome = 0;

            foreach (LoadPlaceGene gene in individual.Chromosome.LoadPlaceGenes)
            {
                double distance;
                distances.TryGetValue(gene.LocationId.Format(gene.LoadPlaceId), out distance);
                decimal cost = (decimal)(distance * 0.1d) + (decimal)(2 * (distance / 4d) * 4d);
                totalIncome += gene.Income - cost - gene.RMCCost;
            }

            return totalIncome;
        }

        public MaghrebiWallerSammut(
            IDeliveryOrderRepository _deliveryOrderRepository,
            ILoadPlacesRepository _loadPlacesRepository) :
            base(_deliveryOrderRepository, _loadPlacesRepository)
        {

        }
    }
}
