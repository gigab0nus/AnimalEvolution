using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using b0nus;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace AnimalEvolution
{
    class Map
    {
        public static readonly int MAPSIZE = 100;
        public static readonly double FOODPERTICK = 0.005f;
        public static readonly double WATERLANDBORDER = 0.4f;
        public static readonly double MINIMUMFOODTOBEFERTILE = 0.4;
        
        private Texture2D dirtTexture;
        private Texture2D waterTexture;
        private Field[,] fields;

        private Camera camera;
        private GraphicsDevice graphicsDevice;

        private StatisticsGUI statisticsGui;
        public StatisticsGUI StatisticsGui
        {
            get
            {
                return statisticsGui;
            }
        }

        private object foodLock = new object();

        private Map(Simulation game, Field[,] fields, Camera camera, StatisticsGUI statisticsGui)
        {
            dirtTexture = game.Content.Load<Texture2D>("dirt");
            waterTexture = game.Content.Load<Texture2D>("water");
            //waterTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            //waterTexture.SetData(new Color[] { Color.Blue });
            this.fields = fields;
            this.camera = camera;
            graphicsDevice = game.GraphicsDevice;
            this.statisticsGui = statisticsGui;
        }

        public Map(Simulation game, Camera camera, StatisticsGUI statisticsGui)
        {
            NoiseGenerator noiseGenerator = new NoiseGenerator();
            dirtTexture = game.Content.Load<Texture2D>("dirt");
            waterTexture = game.Content.Load<Texture2D>("water");
            //waterTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            //waterTexture.SetData(new Color[] { Color.Blue });
            double [,] heightMap = noiseGenerator.Generate(MAPSIZE, 0, 1, 10,2, 0.9);
            fields = new Field[MAPSIZE, MAPSIZE];
            for (int i = 0; i < MAPSIZE; i++)
                for (int j = 0; j < MAPSIZE; j++)
                    fields[i, j] = new Field(0, heightMap[i, j]);
            InitializeFood();
            this.camera = camera;
            graphicsDevice = game.GraphicsDevice;
            this.statisticsGui = statisticsGui;
        }

        private void InitializeFood()
        {
            for (int i = 0; i < fields.GetLength(0); i++)
            {
                for (int j = 0; j < fields.GetLength(1); j++)
                {
                    if (IsDirt(i, j))
                    {
                        fields[i, j].food = 1 - fields[i, j].height;
                    }
                }
            }
        }

        public void GrowFood()
        {
            for (int i = 0; i < MAPSIZE; i++)
            {
                for (int j = 0; j < MAPSIZE; j++)
                {
                    if(IsDirt(i,j) && fields[i,j].food != 1)
                    {
                        if (IsNextToFertileField(i, j))
                        {
                            //fields[i, j].food += (1-fields[i, j].height) * (1 - fields[i, j].food) * 2 * FOODPERTICK;
                            fields[i, j].food += (1 - fields[i, j].height) * FOODPERTICK;
                            if (fields[i, j].food > 1)
                                fields[i, j].food = 1;
                        }
                    }
                }
            }
        }

        public Field FieldAt(double x, double y)
        {
            return fields[(int)Math.Floor(x), (int)Math.Floor(y)];
            //return fields[(int)x, (int)y];
        }

        private bool IsNextToFertileField(double x, double y)
        {
            if (IsFertile(x, y))
                return true;
            if (IsFertile(x - 1, y))
                return true;
            if (IsFertile(x, y - 1))
                return true;
            if (IsFertile(x + 1, y))
                return true;
            if (IsFertile(x, y + 1))
                return true;
            return false;
        }

        private bool IsFertile(double x, double y)
        {
            if (IsWater(x, y))
                return true;
            if (FieldAt(x,y).food > MINIMUMFOODTOBEFERTILE)
                return true;
            return false; 
        }

        public bool IsWater(double x, double y)
        {
            if (!IsOnMap(x, y))
                return true;
            return FieldAt(x, y).height <= WATERLANDBORDER;
        }

        public bool IsDirt(double x, double y)
        {
            if (!IsOnMap(x, y))
                return false;
            return FieldAt(x, y).height > WATERLANDBORDER; 
        }

        public double GetFoodValueAt(double x, double y)
        {
            if (!IsOnMap(x, y))
                return 0;
            return FieldAt(x, y).food;
        }

        public double GetHeightValueAt(double x, double y)
        {
            if (!IsOnMap(x, y))
                return 0;
            return FieldAt(x, y).height;
        }

        
        public double SubtractFoodUpTo(double x, double y, double foodValue)
        {
            lock(foodLock)
            {
                if (GetFoodValueAt(x, y) <= 0 || foodValue < 0)
                    return 0;
                if (GetFoodValueAt(x, y) < foodValue)
                {
                    double foodTaken = FieldAt(x, y).food;
                    FieldAt(x, y).food = 0;
                    return foodTaken;
                }
                FieldAt(x, y).food -= foodValue;
            }
            
            return foodValue;
        }

        public void SubtractFoodValue(double x, double y, double foodValue)
        {
            lock(foodLock)
            {
                if (foodValue < 0)
                    return;
                if (FieldAt(x, y).food < foodValue)
                    FieldAt(x, y).food = 0;
                else
                    FieldAt(x, y).food -= foodValue;
            }
        }

        public double CountAvailableFood()
        {
            double sum = 0;
            for(int i = 0; i< MAPSIZE; i++)
            {
                for(int j = 0; j< MAPSIZE; j++)
                {
                    if (IsDirt(i, j))
                        sum += GetFoodValueAt(i, j);
                }
            }
            return sum;
        }


        public bool IsOnMap(double x, double y)
        {
            return x >= 0 && y >= 0 && y < MAPSIZE && x < MAPSIZE;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            
            Matrix inverseTransform = Matrix.Invert(camera.Transform);
            int minX = (int) Math.Max(0, Vector2.Transform(Vector2.Zero, inverseTransform).X);
            int maxX = (int) Math.Min(MAPSIZE-1, Vector2.Transform(new Vector2(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight), inverseTransform).X);
            int minY = (int)Math.Max(0, Vector2.Transform(Vector2.Zero, inverseTransform).Y);
            int maxY = (int)Math.Min(MAPSIZE-1, Vector2.Transform(new Vector2(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight), inverseTransform).Y);
            for (int i = minX; i< maxX+1; i++)
            {
                for(int j = minY; j< maxY+1; j++)
                {
                    if(IsDirt(i,j))
                    {
                        spriteBatch.Draw(dirtTexture, new Rectangle(i, j, 1, 1), new Color((float)fields[i, j].height, (float)(fields[i, j].height + fields[i, j].food * (1 - fields[i, j].height)), (float)fields[i, j].height));
                    }
                    else
                    {
                        spriteBatch.Draw(waterTexture, new Rectangle(i, j, 1, 1), new Color((float)(0.5+fields[i, j].height / WATERLANDBORDER/2), (float)(0.5+fields[i, j].height / WATERLANDBORDER/2), (float)(0.5+fields[i, j].height / WATERLANDBORDER/2)));
                    }
                }
            }
        }

        public void Serialize(String path)
        {
            FileStream file = File.Open(path + "/map.aemap", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);

            writer.WriteArray2(fields, delegate (Field field) { field.Serialize(writer);});

            file.Close();
        }

        public static Map Deserialize(String path, Simulation simulation, Camera camera, StatisticsGUI statisticsGui)
        {
            FileStream file = File.Open(path + "/map.aemap", FileMode.Open);
            BinaryReader reader = new BinaryReader(file);

            Field[,] fields = reader.ReadArray2<Field>(delegate () {return Field.Deserialize(reader); });

            file.Close();
            return new Map(simulation,fields, camera, statisticsGui);
        }

    }

    class Field
    {
        public double food;
        public double height;

        public Field(double food, double height)
        {
            this.food = food;
            this.height = height;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(food);
            writer.Write(height);

        }

        public static Field Deserialize(BinaryReader reader)
        {
            double food = reader.ReadDouble();
            double height = reader.ReadDouble();

            return new Field(food, height);
        }
    }
}
