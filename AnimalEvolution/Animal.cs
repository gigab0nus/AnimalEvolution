using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.IO;

namespace AnimalEvolution
{

    class Animal
    {

        public static readonly int NUMNEURONIN = 2;
        public static readonly int NEURONINenergy = 0;
        public static readonly int NEURONINage = 1;

        public static readonly int NUMNEURONINPERFEELER = 3;
        public static readonly int NEURONINFEELERfoodOnField = 0;
        public static readonly int NEURONINFEELERisOnWater = 1;
        public static readonly int NEURONINFEELERheight = 2;


        public static readonly int NUMNEURONOUT = 8;
        public static readonly int NEURONOUTmovement = 0;
        public static readonly int NEURONOUTrotation = 1;
        public static readonly int NEURONOUTeat = 2;
        public static readonly int NEURONOUTreproduce = 3;
        public static readonly int NEURONOUTenergyToOffspring = 4;
        public static readonly int NEURONOUTmutationStrength = 5;
        public static readonly int NEURONOUTmutationStrengthAngle = 6;
        public static readonly int NEURONOUTmutationCount = 7;
        public static readonly int NUMNEURONMEMORY = 8;

        public static readonly double INITIALENERGY = 2;
        public static readonly int MAXMUTATIONCOUNT = 80;
        public static readonly double MAXMUTATIONSTRENGTH = Math.Log(1.4);
        public static readonly double MAXMUTATIONANGLESTRENGTH = Math.PI / 5;

        public static readonly float MAXSPEED = 5 / 100f;
        public static readonly float MAXROTATIONSPEED = 10 / 100f;
        public static readonly double MAXEATSPEED = 2f / 100f;

        public static readonly double ENERGYCOSTMOVEMENT = 0.1f;
        public static readonly double ENERGYCOSTROTATION = 0.1f;
        public static readonly double ENERGYCOSTEAT = 0.01f;
        public static readonly double ENERGYCOSTFACTORPERAGE = 0.2f / 100.0f;
        public static readonly double ENERGYCOSTREPRODUCTION = 0.5f;

        public static readonly double ENERGYCOSTSWIM = 0.1f / 100f;

        public static readonly double FOODNUTRITIONFACTOR = 1.25f;

        private static readonly float animalDiameter = 0.5f;

        private static Texture2D animalTexture = null;
        private static Vector2 animalTextureScaleFactor;
        private static Vector2 animalTextureOrigin;


        private LinkedList<Animal> children = new LinkedList<Animal>();
        private LinkedList<long> childrenIdOnly = new LinkedList<long>();
        
        public readonly long Id;

        public readonly long ParentId;

        private Vector2 pos;
        private float rotation;
        private double energy;
        public double Energy { get { return energy; } }
        private double energyConsumed;
        public double EnergyConsumed { get { return energyConsumed; } }
        private long age = 0;
        public long Age { get { return age; } }
        public readonly int Generation;

        public double GeneticStability { get { return brain.GeneticStability; } }
        public int NumDirectOffspring { get { return children.Count; } }
        public bool IsBrainLoaded { get { return brain != null; } }



        private static Vector2[] feelerPos = new Vector2[]
        {
            Vector2.Zero,
            Vector2.UnitX,
            Vector2.Transform(Vector2.UnitX*0.9f, Matrix.CreateRotationZ((float)Math.PI/4)),
            Vector2.Transform(Vector2.UnitX*0.9f, Matrix.CreateRotationZ((float)-Math.PI/4)),
            Vector2.Transform(Vector2.UnitX*0.9f*2, Matrix.CreateRotationZ((float)Math.PI/4)),
            Vector2.Transform(Vector2.UnitX*0.9f*2, Matrix.CreateRotationZ((float)-Math.PI/4)),
            Vector2.Transform(Vector2.UnitX*0.9f*2, Matrix.CreateRotationZ((float)Math.PI/12)),
            Vector2.Transform(Vector2.UnitX*0.9f*2, Matrix.CreateRotationZ((float)-Math.PI/12)),
        };

        private Color color;

        private NeuralNetwork brain;

        private bool isAlive = true;
        public bool Alive
        {
            get
            {
                return isAlive;
            }
        }

        private AnimalManager animalManager;

        private Animal(Vector2 pos, AnimalManager animalManager, Color color, float rotation, double energy, int generation, long age, double energyConsumed, long id, LinkedList<long> children, long parentId, NeuralNetwork brain = null)
        {
            this.brain = brain;
            this.pos = pos;
            this.color = color;
            this.rotation = rotation;
            this.energy = energy;
            this.animalManager = animalManager;
            this.Generation = generation;
            this.age = age;
            this.energyConsumed = energyConsumed;
            this.Id = id;
            this.childrenIdOnly = children;
            this.ParentId = parentId;
        }


        public Animal(Vector2 pos, AnimalManager animalManager)
        {
            brain = new NeuralNetwork(new int[] { NUMNEURONIN + feelerPos.Length * NUMNEURONINPERFEELER + NUMNEURONMEMORY, 20, NUMNEURONOUT + NUMNEURONMEMORY });
            this.pos = pos;
            this.color = new Color((float)Simulation.NextRandomDouble(), (float)Simulation.NextRandomDouble(), (float)Simulation.NextRandomDouble());
            this.rotation = (float)(Simulation.NextRandomDouble() * Math.PI * 2);
            energy = INITIALENERGY;
            this.animalManager = animalManager;
            this.Id = animalManager.GetNextId();
            this.Generation = 0;
            this.ParentId = AnimalManager.IDNONE;
        }

        public Animal(Animal mother, double startEnergy, AnimalManager animalManager)
        {
            brain = new NeuralNetwork(mother.brain);
            this.pos = mother.pos;
            this.color = MutateColor(mother.color);
            this.rotation = mother.rotation + (float)Simulation.NextRandomDouble() * 4 - 2f;
            energy = startEnergy;
            this.animalManager = animalManager;
            this.Id = animalManager.GetNextId();
            this.Generation = mother.Generation + 1;
            this.ParentId = mother.Id;
        }

        public void Update()
        {
            //Brain Input
            double[] input = new double[NUMNEURONIN + feelerPos.Length * NUMNEURONINPERFEELER + NUMNEURONMEMORY];
            input[NEURONINage] = age / 1000f;
            input[NEURONINenergy] = energy / 10f;

            for (int i = 0; i < feelerPos.Length; i++)
            {
                Vector2 absoluteFeelerPos = Vector2.Transform(feelerPos[i], Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(pos.X, pos.Y, 0));
                input[NUMNEURONIN + i * NUMNEURONINPERFEELER + NEURONINFEELERfoodOnField] = animalManager.Map.GetFoodValueAt(absoluteFeelerPos.X, absoluteFeelerPos.Y);
                input[NUMNEURONIN + i * NUMNEURONINPERFEELER + NEURONINFEELERheight] = animalManager.Map.GetHeightValueAt(absoluteFeelerPos.X, absoluteFeelerPos.Y);
                input[NUMNEURONIN + i * NUMNEURONINPERFEELER + NEURONINFEELERisOnWater] = animalManager.Map.IsWater(absoluteFeelerPos.X, absoluteFeelerPos.Y) ? 1 : 0;
            }




            for (int i = 0; i < NUMNEURONMEMORY; i++)
            {
                input[i + NUMNEURONIN + feelerPos.Length * NUMNEURONINPERFEELER] = brain.GetOutput(i + NUMNEURONOUT);
            }

            brain.Feed(input);

            //Acting
            Move();
            Eat();
            Reproduce();

            //Dying
            if (energy < 0)
            {
                isAlive = false;
            }

            //Age
            age += 1;
        }

        private void UseRelativeEnergy(double energy)
        {
            this.energy -= energy * (1 + age * ENERGYCOSTFACTORPERAGE);
        }

        private double UseAbsoluteEnergy(double energy)
        {
            double relativeEnergy = energy / (1 + age * ENERGYCOSTFACTORPERAGE);
            this.energy -= energy;
            return relativeEnergy;
        }

        private void Move()
        {
            //Rotation
            float rotationTick = (float)brain.GetOutput(NEURONOUTrotation) * MAXROTATIONSPEED;
            rotation += rotationTick;
            UseRelativeEnergy(Math.Abs(rotationTick) * ENERGYCOSTROTATION);
            if (rotation > Math.PI * 2)
                rotation %= (float)Math.PI * 2;
            if (rotation < 0)
                rotation += (float)Math.PI * 2;

            //Movement
            double speed = brain.GetOutput(NEURONOUTmovement) * MAXSPEED;
            pos += Vector2.Transform(Vector2.UnitX * (float)speed, Matrix.CreateRotationZ(rotation));
            UseRelativeEnergy(Math.Abs(speed) * ENERGYCOSTMOVEMENT);

            if (animalManager.Map.IsWater(pos.X, pos.Y))
            {
                UseRelativeEnergy(ENERGYCOSTSWIM);
            }
        }

        private void Eat()
        {
            if (age < 1)
                return;
            double eatPower = brain.GetOutput(NEURONOUTeat) * MAXEATSPEED;
            if (eatPower > 0)
            {
                UseRelativeEnergy(ENERGYCOSTEAT * eatPower);
                double newEnergy = animalManager.Map.SubtractFoodUpTo(pos.X, pos.Y, eatPower) * FOODNUTRITIONFACTOR;
                energy += newEnergy;
                energyConsumed += newEnergy;
            }
        }

        private void Reproduce()
        {
            if (age < 2)
                return;
            if (brain.GetOutput(NEURONOUTreproduce) > 0)
            {
                UseRelativeEnergy(ENERGYCOSTREPRODUCTION);
                if (energy > 0f)//MINIMUMREPRODUCEENERGY
                {

                    double energyToUseForOffspring = brain.GetPositiveOutput(NEURONOUTenergyToOffspring) * energy;
                    double mutationStrength = brain.GetPositiveOutput(NEURONOUTmutationStrength) * MAXMUTATIONSTRENGTH;
                    double mutationStrengthAngle = brain.GetPositiveOutput(NEURONOUTmutationStrengthAngle) * MAXMUTATIONANGLESTRENGTH;
                    int numMutations = (int)Math.Ceiling(brain.GetPositiveOutput(NEURONOUTmutationCount) * MAXMUTATIONCOUNT);
                    Animal offspring = new Animal(this, UseAbsoluteEnergy(energyToUseForOffspring) / 2, animalManager);
                    offspring.brain.Mutate(MAXMUTATIONCOUNT, Math.Exp(mutationStrength), mutationStrengthAngle);
                    animalManager.SpawnAnimal(offspring);
                    children.AddLast(offspring);
                }
            }
        }

        public static void LoadTexture(ContentManager content)
        {
            if (animalTexture != null)
                return;
            animalTexture = content.Load<Texture2D>("animalTexture");
            animalTextureScaleFactor = new Vector2(1f / animalTexture.Width, 1f / animalTexture.Height);
            animalTextureOrigin = new Vector2(animalTexture.Width / 2f, animalTexture.Height / 2f);
        }



        public void DrawNetwork(SpriteBatch spriteBatch)
        {
            brain.Draw(spriteBatch);
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            
            for (int i = 0; i < feelerPos.Length; i++)
            {
                Vector2 absoluteFeelerPos = Vector2.Transform(feelerPos[i], Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(pos.X, pos.Y, 0));
                spriteBatch.Draw(animalTexture, rotation: rotation, color: color, position: absoluteFeelerPos, scale: animalTextureScaleFactor / 20, origin: animalTextureOrigin);
            }
            
            spriteBatch.Draw(animalTexture, rotation: rotation, color: color, position: pos, scale: animalTextureScaleFactor*animalDiameter, origin: animalTextureOrigin);
        }

        private static int COLORMUTATIONSTRENGTH = 5;
        private Color MutateColor(Color original)
        {
            int red = original.R;
            int green = original.G;
            int blue = original.B;
            switch (Simulation.NextRandom(6))
            {
                case 0:
                    if (red < 255 - COLORMUTATIONSTRENGTH)
                        red += COLORMUTATIONSTRENGTH;
                    break;
                case 1:
                    if (red > COLORMUTATIONSTRENGTH)
                        red -= COLORMUTATIONSTRENGTH;
                    break;
                case 2:
                    if (green < 255 - COLORMUTATIONSTRENGTH)
                        green += COLORMUTATIONSTRENGTH;
                    break;
                case 3:
                    if (green > COLORMUTATIONSTRENGTH)
                        green -= COLORMUTATIONSTRENGTH;
                    break;
                case 4:
                    if (blue < 255 - COLORMUTATIONSTRENGTH)
                        blue += COLORMUTATIONSTRENGTH;
                    break;
                case 5:
                    if (blue > COLORMUTATIONSTRENGTH)
                        blue -= COLORMUTATIONSTRENGTH;
                    break;
            }
            return new Color(red, green, blue);
        }

        public void LinkChildren(LinkedList<Animal> animals)
        {
            for(int i = 0; i< childrenIdOnly.Count; i++)
            {
                Animal child = animals.FirstWhere(delegate (Animal animal) { return animal.Id == childrenIdOnly.ElementAt(i); });
                if(child != null)
                {
                    children.AddLast(child);
                    childrenIdOnly.Remove(childrenIdOnly.ElementAt(i));
                    i--;
                }
            }
        }

        public void WriteBrain(String path)
        {
            System.IO.Directory.CreateDirectory(path + "/brains");
            FileStream file = File.Open(path + "/brains/b" + Id + ".aebrain", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);
            brain.Write(writer);
            brain = null;
            file.Close();
        }

        public void Write(BinaryWriter writer, String path)
        {
            writer.Write(Id);
            writer.Write(pos);
            writer.Write(color);
            writer.Write(rotation);
            writer.Write(energy);
            writer.Write(Generation);
            writer.Write(age);
            writer.Write(energyConsumed);
            writer.Write(ParentId);
            IEnumerable<long> childrenIds = childrenIdOnly.Concat(children.Select(delegate (Animal animal) { return animal.Id; }));
            writer.WriteEnumerable(children, delegate (Animal animal) { writer.Write(animal.Id); });
            if(IsBrainLoaded)
                WriteBrain(path);
        }

        public bool ReadBrain(String path)
        {
            FileStream file = File.Open(path + "/brains/b" + Id + ".aebrain", FileMode.OpenOrCreate);
            BinaryReader reader = new BinaryReader(file);
            try
            {
                brain = NeuralNetwork.Read(reader);
            }
            catch(Exception e)
            {
                brain = null;
            }
            file.Close();
            return IsBrainLoaded;
        }

        public static Animal ReadWithoutBrain(BinaryReader reader, AnimalManager manager)
        {

            long id = reader.ReadInt64();
            Vector2 pos = reader.ReadVector2();
            Color color = reader.ReadColor();
            float rotation = reader.ReadSingle();
            double energy = reader.ReadDouble();
            int generation = reader.ReadInt32();
            long age = reader.ReadInt64();
            double energyConsumed = reader.ReadDouble();
            long parentId = reader.ReadInt64();
            LinkedList<long> children = reader.ReadLinkedList(delegate () { return reader.ReadInt64(); });
            return new Animal(pos, manager, color, rotation, energy, generation, age, energyConsumed, id, children, parentId);
        }

        public static Animal ReadWithBrain(BinaryReader reader, AnimalManager manager, String path)
        {
            Animal animal = ReadWithoutBrain(reader, manager);
            animal.ReadBrain(path);
            return animal;
        }
    }

}
