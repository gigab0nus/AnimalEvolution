using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AnimalEvolution
{
    public class Simulation : Game
    {
        GraphicsDeviceManager graphics;

        private Map map;
        private AnimalManager animalManager;
        private Camera camera;
        private StatisticsGUI statisticsGUI;

        private SpriteBatch spriteBatch;

        private double targetFps = 5;
        private int currentTicksPerFrame = 1;

        private bool fixedSimulationSpeed = true;

        private String savePath;

        public Simulation(String path = null)
        {
            graphics = new GraphicsDeviceManager(this);
            
            Content.RootDirectory = "Content";
            if (path == null)
                this.savePath = Environment.CurrentDirectory + "/SavedData";
            else
                this.savePath = Path.GetDirectoryName(path);
        }

        protected override void Initialize()
        {
            Window.Position = new Point(0, 0);
            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.ApplyChanges();

            // TODO: Add your initialization logic here
            IsFixedTimeStep = false;
            IsMouseVisible = true;
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Animal.LoadTexture(Content);

            statisticsGUI = new StatisticsGUI(this);
            camera = new Camera();

            try{
                map = Map.Deserialize(savePath, this, camera, statisticsGUI);
            }catch(Exception e){
                map = new Map(this, camera, statisticsGUI);
            }
            try{
                animalManager = AnimalManager.Read(savePath, map, delegate(Animal animal) { });
            } catch(Exception e){
                animalManager = new AnimalManager(map, delegate (Animal animal) { });
            }

            // TODO: use this.Content to load your game content here
        }
        
        protected override void UnloadContent()
        {
            String path = savePath;
            System.IO.Directory.CreateDirectory(path);
            map.Serialize(path);
            animalManager.Write(path);
            animalManager.TerminateThreads();
            CreateStartingPoint(path);
            // TODO: Unload any non ContentManager content here
        }

        private void CreateStartingPoint(String path)
        {
            FileStream file = File.Open(path + "/start.aesimulation", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);
            writer.Write(path);
            file.Close();
        }

        private bool wasSpacePressed = false;
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();
            if(keyboardState.IsKeyDown(Keys.Space) && !wasSpacePressed)
            {
                fixedSimulationSpeed = !fixedSimulationSpeed;
            }
            wasSpacePressed = keyboardState.IsKeyDown(Keys.Space);
            if (keyboardState.IsKeyDown(Keys.PageUp))
                targetFps *= Math.Pow(2, gameTime.ElapsedGameTime.Milliseconds / 1000f);
            if (keyboardState.IsKeyDown(Keys.PageDown))
                targetFps /= Math.Pow(2, gameTime.ElapsedGameTime.Milliseconds / 1000f);
            camera.Update();
            if (fixedSimulationSpeed)
                currentTicksPerFrame = 1;
            else
                currentTicksPerFrame = (int)(currentTicksPerFrame * 1000f / targetFps / gameTime.ElapsedGameTime.Milliseconds);
            if (currentTicksPerFrame < 1) currentTicksPerFrame = 1;
            if (currentTicksPerFrame > 100) currentTicksPerFrame = 100;
            for (int i = 0; i< currentTicksPerFrame; i++)
            {
                map.GrowFood();
                animalManager.Update();

                statisticsGUI.SaveTick(animalManager);
            }
            statisticsGUI.SaveFrameStatistics(gameTime.ElapsedGameTime.Milliseconds, currentTicksPerFrame);
            // TODO: Add your update logic here

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);


            spriteBatch.Begin(transformMatrix: camera.Transform, samplerState: SamplerState.PointClamp);

            map.Draw(spriteBatch);
            if (fixedSimulationSpeed)
            {
                animalManager.DrawAnimals(spriteBatch);
            }
            spriteBatch.End();

            spriteBatch.Begin();
            animalManager.DrawBrain(spriteBatch);
            statisticsGUI.Draw(spriteBatch);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        


        private static Random globalRandom = null;

        private static Random GlobalRandom
        {
            get
            {
                if (globalRandom == null)
                    globalRandom = new Random();
                return globalRandom;
            }
        }

        public static double NextRandomDouble()
        {
            Random random = GlobalRandom;
            double val;
            lock (random)
            {
                val = random.NextDouble();
            }
            return val;
        }

        public static double NextRandomGaussian()
        {
            Random random = GlobalRandom;
            double val;
            lock (random)
            {
                val = random.NextGaussian();
            }
            return val;
        }

        public static int NextRandom(int num)
        {
            Random random = GlobalRandom;
            int val;
            lock (random)
            {
                val = random.Next(num);
            }
            return val;
        }

    }
}
