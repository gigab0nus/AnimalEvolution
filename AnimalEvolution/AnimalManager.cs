using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace AnimalEvolution
{
    class AnimalManager
    {
        public static readonly int MINIMUMLIVINGANIMALS = 200;

        public static readonly long IDNONE = -1;

        private Action<Animal> animalSaver;

        private LinkedList<Animal> animals;
        public LinkedList<Animal> Animals { get { return animals; } }
        private Map map;
        public Map Map { get { return map; } }
        private long highestId;

        public int AnimalCount { get { return animals.Count; } }

        public AnimalManager (Map map, Action<Animal> animalSaver)
        {
            this.map = map;
            animals = new LinkedList<Animal>();
            highestId = IDNONE;
        }

        public long GetNextId()
        {
            highestId += 1;
            return highestId;
        }

        private void UpdateUnthreaded()
        {
            int animalCount = animals.Count;
            //Update Animals
            for (int i = 0; i < animalCount; i++)
            {
                animals.ElementAt(i).Update();
            }
        }
        
        AnimalUpdateThread[] threads;
        private void UpdateThreaded(int threadCount)
        {
            int animalCount = animals.Count;
            if (threads != null)
            {
                if (threads.Count() != threadCount)
                    TerminateThreads();
            }
            if (threads == null)
            {
                threads = new AnimalUpdateThread[threadCount];
                for(int i = 0; i< threadCount; i++)
                {
                    threads[i] = new AnimalUpdateThread(animals);
                }
            }
            
            for(int i = 0; i< threadCount; i++)
            {
                int from = animalCount * i / (threadCount-1);
                int to = animalCount * (i + 1) / (threadCount-1);
                if (to > animalCount)
                    to = animalCount;
                if (from > animalCount)
                    from = animalCount;

                threads[i].SetTask(from, to);
            }
            for (int i = 0; i < threadCount; i++)
            {
                lock(threads[i])
                {
                    if(!threads[i].Finished)
                    {
                        Monitor.Wait(threads[i]);
                    }
                }
            }
        }

        public void TerminateThreads()
        {
            if(threads != null)
            {
                for(int i = 0; i< threads.Length; i++)
                {
                    threads[i].Abort();
                    threads[i] = null;
                }
                threads = null;
                Debug.WriteLine("Threads terminated");
            }
        }

        public void Update()
        {
            int animalCount = animals.Count;
            //Update Animals
            UpdateThreaded(8);
            //UpdateUnthreaded();

            //Remove dead Animals
            
            if(animals.Count > 0)
            {
                
                for (LinkedListNode<Animal> i = animals.First; i != null;)
                {
                    Animal current = i.Value;
                    i = i.Next;
                    if (!current.Alive)
                    {
                        map.StatisticsGui.SaveAnimal(current);
                        animals.Remove(current);
                    }
                }
            }
            //Add new random Animals
            while (animals.Count < MINIMUMLIVINGANIMALS)
            {
                SpawnRandomAnimal();
            }
            
        }

        private void LinkChildren()
        {
            foreach(Animal animal in animals)
            {
                animal.LinkChildren(animals);
            }
        }

        public void SpawnRandomAnimal()
        {
            SpawnAnimal(new Animal(new Vector2((float)(Simulation.NextRandomDouble() * Map.MAPSIZE), (float)(Simulation.NextRandomDouble() * Map.MAPSIZE)), this));
        }
        
        public void SpawnAnimal(Animal animal)
        {
            lock(animals)
            {
                animals.AddLast(animal);
            }
        }

        public void DrawAnimals(SpriteBatch spriteBatch)
        {
            foreach(Animal animal in animals)
            {
                animal.Draw(spriteBatch);
            }
        }

        public void DrawBrain(SpriteBatch spriteBatch)
        {

            if (animals.Count > 0)
                animals.First().DrawNetwork(spriteBatch);
        }

        public void Write(String path)
        {
            FileStream file = File.Open(path + "/manager.aemanager", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);
            //Write Start
            writer.Write(highestId);
            writer.Write(AnimalCount);
            for(int i = 0; i< AnimalCount; i++)
            {
                animals.ElementAt(i).Write(writer, path);
            }
            //Write End
            file.Close();
        }

        public static AnimalManager Read(String path, Map map, Action<Animal> animalSaver)
        {

            LinkedList<Animal> animals = new LinkedList<Animal>();
            AnimalManager manager = new AnimalManager(map, animalSaver);

            FileStream file = File.Open(path + "/manager.aemanager", FileMode.Open);
            BinaryReader reader = new BinaryReader(file);
            //Read Start
            manager.highestId = reader.ReadInt64();
            int animalCount = reader.ReadInt32();
            for(int i = 0; i< animalCount; i++)
            {
                manager.SpawnAnimal(Animal.ReadWithBrain(reader, manager, path));
            }
            //Read End
            file.Close();

            manager.LinkChildren();
            return manager;
        }








        private class AnimalUpdateThread
        {
            private LinkedList<Animal> animals;
            private int from = 0;
            private int to = 0;
            private bool shouldAbort = false;
            private bool finished = true;
            public bool Finished
            {
                get
                {
                    return finished;
                }
            }

            private Thread thread;
            public Thread Thread
            {
                get
                {
                    return thread;
                }
            }

            public void SetTask(int from, int to)
            {
                
                lock(this)
                {
                    if(!finished)
                    {
                        Monitor.Wait(this);
                    }
                    this.from = from;
                    this.to = to;
                    finished = false;
                    Monitor.Pulse(this);
                }
            }

            public AnimalUpdateThread(LinkedList<Animal>animals)
            {
                this.animals = animals;
                thread = null;
                thread = new Thread(new ThreadStart(run));
                thread.Start();
            }

            public void Abort()
            {
                lock (this)
                {
                    shouldAbort = true;
                    Monitor.Pulse(this);
                }
            }

            public void run()
            {
                while(!shouldAbort)
                {
                    lock(this)
                    {
                        if(finished)
                        {
                            Monitor.Wait(this);
                        }
                        if (shouldAbort)
                            return;
                        for (int i = from; i < to; i++)
                        {
                            Animal animal = null;
                            lock (animals)
                            {
                               animal = animals.ElementAt(i);
                            }
                            animal.Update();
                        }
                        finished = true;
                        Monitor.Pulse(this);
                    }
                }
            }
        }
    }
}
