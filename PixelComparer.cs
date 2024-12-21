using System.Collections.Generic;

namespace PSI
{
    class PixelComparer : IEqualityComparer<Pixel>
    {
        /// <summary>
        /// classe qui compare deux instances de pixel pour crée le dictionnaire correctement (voir site Microsoft pour l'implementatino de la classe)
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool Equals(Pixel p1, Pixel p2)
        {
            return p1.red == p2.red && p1.green == p2.green && p1.blue == p2.blue;//compare juste les valeurs RGB 
        }

        public int GetHashCode(Pixel p)
        {
            return p.red.GetHashCode() ^ p.green.GetHashCode() ^ p.blue.GetHashCode();
        }
    }
}