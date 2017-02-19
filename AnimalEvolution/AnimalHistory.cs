using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalEvolution
{
    class AnimalHistory
    {
        LinkedList<Animal> cachedAnimals;

        public AnimalHistory()
        {
            cachedAnimals = new LinkedList<Animal>();
        }

        public void Save(Animal animal)
        {
            cachedAnimals.AddLast(animal);
        }
        
    }
}
