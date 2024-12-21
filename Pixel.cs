using System;

namespace PSI
{
    public class Pixel
    {
        public byte red;
        public byte green;
        public byte blue;

        /// <summary>
        /// Constructeur d'un Pixel
        /// </summary>
        /// <param name="red"> Composante rouge d'un pixel </param>
        /// <param name="green"> Composante verte d'un pixel </param>
        /// <param name="blue"> Composante bleue d'un pixel </param>
        public Pixel(byte red, byte green, byte blue)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
        }


        /// <summary>
        /// Représentation textuelle des composantes d'un Pixel
        /// </summary>
        /// <returns> Texte décrivant le Pixel </returns>
        public string Ecrire()
        {
            Console.WriteLine("red : " + this.red);
            Console.WriteLine("green : " + this.green);
            Console.Write("blue : " + this.blue + "\n");
            return " ";
        }

        public byte R
        {
            get { return this.red; }
            set { this.red = value; }
        }

        public byte G
        {
            get { return this.green; }
            set { this.green = value; }
        }

        public byte B
        {
            get { return this.blue; }
            set { this.blue = value; }
        }
    }
}
