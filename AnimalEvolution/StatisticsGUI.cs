using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalEvolution
{
    class StatisticsGUI
    {
        private SpriteFont spriteFont;


        public readonly int MAXFRAMETIMESSAVED = 10;
        public readonly int MAXANIMALSTATICSSAVED = 1000;
        public readonly int MAXTICKSTATISTICSSAVED = 1000;
        public readonly int MAXBESTANIMALSTATISTICSSAVED = 100;

        
        private LinkedList<Animal> deadAnimals = new LinkedList<Animal>();


        private LinkedList<int> ticksPerFrame = new LinkedList<int>();
        private LinkedList<double> frameTimes = new LinkedList<double>();

        private LinkedList<double> foodAvailableCounts = new LinkedList<double>();
        private LinkedList<int> animalsAliveCounts = new LinkedList<int>();

        private LinkedList<Animal> oldestAnimals = new LinkedList<Animal>();
        private LinkedList<Animal> energyRichestAnimals = new LinkedList<Animal>();
        private LinkedList<Animal> mostConsumingAnimals = new LinkedList<Animal>();
        private LinkedList<Animal> mostOffspringAnimals = new LinkedList<Animal>();

        public StatisticsGUI(Simulation simulation)
        {
            spriteFont = simulation.Content.Load<SpriteFont>("Arial");
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            currentLogPos = Vector2.Zero;
            Log(spriteBatch, "FPS: " + frameTimes.Count * 1000 / frameTimes.Sum());
            Log(spriteBatch, "Ticks: " + ticksPerFrame.Last());
            Log(spriteBatch, "          Avg: " + ticksPerFrame.Average());
            Log(spriteBatch, "Animals alive: " + animalsAliveCounts.Last());
            Log(spriteBatch, "          Avg: " + animalsAliveCounts.Average());
            Log(spriteBatch, "Food Available: " + foodAvailableCounts.Last());
            Log(spriteBatch, "          Avg: " + foodAvailableCounts.Average());
            if(oldestAnimals.Count > 0)
            {
                Log(spriteBatch, "Oldest Animals: " + oldestAnimals.Count);
                Log(spriteBatch, "          Num offspring: " + oldestAnimals.Average(delegate (Animal animal) { return animal.NumDirectOffspring; }));
                Log(spriteBatch, "          Age: " + oldestAnimals.Average(delegate (Animal animal) { return animal.Age; }));
                Log(spriteBatch, "          Genetic Stability: " + oldestAnimals.Average(delegate (Animal animal) { return animal.GeneticStability; }));
                Log(spriteBatch, "          Energy consumed: " + oldestAnimals.Average(delegate (Animal animal) { return animal.EnergyConsumed; }));
                Log(spriteBatch, "          Energy available: " + oldestAnimals.Average(delegate (Animal animal) { return animal.Energy; }));
            }

            if (energyRichestAnimals.Count > 0)
            {
                Log(spriteBatch, "Energy richest Animals: " + energyRichestAnimals.Count);
                Log(spriteBatch, "          Num offspring: " + energyRichestAnimals.Average(delegate (Animal animal) { return animal.NumDirectOffspring; }));
                Log(spriteBatch, "          Age: " + energyRichestAnimals.Average(delegate (Animal animal) { return animal.Age; }));
                Log(spriteBatch, "          Genetic Stability: " + energyRichestAnimals.Average(delegate (Animal animal) { return animal.GeneticStability; }));
                Log(spriteBatch, "          Energy consumed: " + energyRichestAnimals.Average(delegate (Animal animal) { return animal.EnergyConsumed; }));
                Log(spriteBatch, "          Energy available: " + energyRichestAnimals.Average(delegate (Animal animal) { return animal.Energy; }));
            }

            if (mostConsumingAnimals.Count > 0)
            {
                Log(spriteBatch, "Most consuming Animals: " + mostConsumingAnimals.Count);
                Log(spriteBatch, "          Num offspring: " + mostConsumingAnimals.Average(delegate (Animal animal) { return animal.NumDirectOffspring; }));
                Log(spriteBatch, "          Age: " + mostConsumingAnimals.Average(delegate (Animal animal) { return animal.Age; }));
                Log(spriteBatch, "          Genetic Stability: " + mostConsumingAnimals.Average(delegate (Animal animal) { return animal.GeneticStability; }));
                Log(spriteBatch, "          Energy consumed: " + mostConsumingAnimals.Average(delegate (Animal animal) { return animal.EnergyConsumed; }));
                Log(spriteBatch, "          Energy available: " + mostConsumingAnimals.Average(delegate (Animal animal) { return animal.Energy; }));
            }

            if (mostOffspringAnimals.Count > 0)
            {
                Log(spriteBatch, "Most offspring Animals: " + mostOffspringAnimals.Count);
                Log(spriteBatch, "          Num offspring: " + mostOffspringAnimals.Average(delegate (Animal animal) { return animal.NumDirectOffspring; }));
                Log(spriteBatch, "          Age: " + mostOffspringAnimals.Average(delegate (Animal animal) { return animal.Age; }));
                Log(spriteBatch, "          Genetic Stability: " + mostOffspringAnimals.Average(delegate (Animal animal) { return animal.GeneticStability; }));
                Log(spriteBatch, "          Energy consumed: " + mostOffspringAnimals.Average(delegate (Animal animal) { return animal.EnergyConsumed; }));
                Log(spriteBatch, "          Energy available: " + mostOffspringAnimals.Average(delegate (Animal animal) { return animal.Energy; }));
            }

            if (deadAnimals.Count > 0)
            {
                Log(spriteBatch, "Dead Animals: " + deadAnimals.Count);
                Log(spriteBatch, "          Num offspring: " + deadAnimals.Average(delegate (Animal animal) { return animal.NumDirectOffspring; }));
                Log(spriteBatch, "          Age: " + deadAnimals.Average(delegate (Animal animal) { return animal.Age; }));
                Log(spriteBatch, "          Genetic Stability: " + deadAnimals.Average(delegate (Animal animal) { return animal.GeneticStability; }));
                Log(spriteBatch, "          Energy consumed: " + deadAnimals.Average(delegate (Animal animal) { return animal.EnergyConsumed; }));
            }


        }

        public void SaveFrameStatistics(double newTime, int numTicksPerFrame)
        {
            frameTimes.AddLast(newTime);
            while (frameTimes.Count > MAXFRAMETIMESSAVED)
                frameTimes.RemoveFirst();
            ticksPerFrame.AddLast(numTicksPerFrame);
            while (ticksPerFrame.Count > MAXFRAMETIMESSAVED)
                ticksPerFrame.RemoveFirst();
        }

        public void SaveAnimal(Animal animal)
        {
            deadAnimals.AddLast(animal);
            while (deadAnimals.Count > MAXANIMALSTATICSSAVED)
                deadAnimals.RemoveFirst();
        }

        public void SaveTick(AnimalManager manager)
        {
            foodAvailableCounts.AddLast(manager.Map.CountAvailableFood());
            while (foodAvailableCounts.Count > MAXTICKSTATISTICSSAVED)
                foodAvailableCounts.RemoveFirst();
            animalsAliveCounts.AddLast(manager.AnimalCount);
            while (animalsAliveCounts.Count > MAXTICKSTATISTICSSAVED)
                animalsAliveCounts.RemoveFirst();
            if(manager.AnimalCount > 0)
            {
                Animal oldestAnimal = manager.Animals.MaxObject(delegate (Animal a, Animal b) { return a.Age > b.Age ? a : b; });
                if (!oldestAnimals.Contains(oldestAnimal))
                    oldestAnimals.AddLast(oldestAnimal);
                Animal energyRichestAnimal = manager.Animals.MaxObject(delegate (Animal a, Animal b) { return a.Energy > b.Energy ? a : b; });
                if (!energyRichestAnimals.Contains(energyRichestAnimal))
                    energyRichestAnimals.AddLast(energyRichestAnimal);
                Animal mostConsumingAnimal = manager.Animals.MaxObject(delegate (Animal a, Animal b) { return a.EnergyConsumed > b.EnergyConsumed ? a : b; });
                if (!mostConsumingAnimals.Contains(mostConsumingAnimal))
                    mostConsumingAnimals.AddLast(mostConsumingAnimal);
                Animal mostOffspringAnimal = manager.Animals.MaxObject(delegate (Animal a, Animal b) { return a.NumDirectOffspring > b.NumDirectOffspring ? a : b; });
                if (!mostOffspringAnimals.Contains(mostOffspringAnimal))
                    mostOffspringAnimals.AddLast(mostOffspringAnimal);
            }
            while (oldestAnimals.Count > MAXBESTANIMALSTATISTICSSAVED)
                oldestAnimals.RemoveFirst();
            while (energyRichestAnimals.Count > MAXBESTANIMALSTATISTICSSAVED)
                energyRichestAnimals.RemoveFirst();
            while (mostConsumingAnimals.Count > MAXBESTANIMALSTATISTICSSAVED)
                mostConsumingAnimals.RemoveFirst();
            while (mostOffspringAnimals.Count > MAXBESTANIMALSTATISTICSSAVED)
                mostOffspringAnimals.RemoveFirst();
        }

        private Vector2 currentLogPos;
        private void Log(SpriteBatch spriteBatch, String str)
        {
            spriteBatch.DrawString(spriteFont, str, currentLogPos, Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            currentLogPos += Vector2.UnitY * 14;
        }
    }
}
