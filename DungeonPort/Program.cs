using System;

namespace DungeonPort
{
    class Program
    {
        static void Main(string[] args)
        {
            int width, height, items = 0; // Vars
            Console.Write("Enter a width: ");
            width = int.Parse(Console.ReadLine());
            //-------------------------------------------
            Console.Write("Enter a height: ");
            height = int.Parse(Console.ReadLine());
            //-------------------------------------------
            Console.Write("Enter the number of items: ");
            items = int.Parse(Console.ReadLine());
            //-------------------------------------------
            MainGeneration d = new MainGeneration(width, height);
            d.generate(items);
            d.print();
        }
    }
}
