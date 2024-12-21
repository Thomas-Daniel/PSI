using System;
using System.Collections.Generic;
using System.Text;

namespace PSI
{
    class Noeud
    {    
        public Pixel Pixel { get; set; }
        public int Frequence { get; set; }
        public Noeud Gauche { get; set; }
        public Noeud Droit { get; set; }
        /// <summary>
        /// constructeur du noeud prenant seulemnt un pixel et une frequence en entrée
        /// </summary>
        /// <param name="pixel"></param>
        /// <param name="frequence"></param>
        public Noeud(Pixel pixel, int frequence)
        {
            Pixel = pixel;
            Frequence = frequence;
            Gauche = null;
            Droit = null;
        }

        /// <summary>
        /// Fonction pour verifier sur le noeud est une feuille càd s'il n'a pas de noeud adjacent
        /// </summary>
        /// <returns></returns>
        public bool EstFeuille()
        {
            return Gauche == null && Droit == null;
        }

    }
}
