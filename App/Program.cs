using System;
using System.Collections.Generic;
using App.Attributes;
using Animals;

namespace App
{
    [ExcludeFromCoverage]
    class Program
    {
        static void Main()
        {
            var animals = new List<IAnimal>
            {
                new Elephant(),
                new Fish()
            };

            foreach (var animal in animals)
            {
                Console.WriteLine(animal.MakeSound());
            }
        }
    }
}
