using System;
using System.Diagnostics;

namespace AnimalEvolution
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            String path = null;
            if (args.Length > 0)
                path = args[0];
            using (var game = new Simulation(path))
                game.Run();
            
        }
    }
#endif
}
