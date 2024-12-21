using System.Collections.Generic;

namespace PSI
{
    class Huffman
    {
        //on crée le noeud racine
        //et le dictonnaire dess fréquences 
        public Noeud racine;
        public Dictionary<Pixel, int> frequencePixels;


        /// <summary>
        /// Constructeur de Huffman
        /// Le principe du constructeur est d'alimenter le dictionnaire des fréquences avec chaque pixel de l'image avec sa frequence d'apparition
        /// </summary>
        /// <param name="image"></param>
        public Huffman(MyImage image)
        {
            frequencePixels = new Dictionary<Pixel, int>(new PixelComparer());// PixelComparer() sert à comparer deux instances de pixel sinon le .ContainsKey ne fait pas la différence
            for (int x = 0; x < image.Height; x++)
            {
                for (int y = 0; y < image.Width; y++)
                {
                    Pixel pixel = image.pixels[x,y];
                    if (frequencePixels.ContainsKey(pixel))//On vérifie si le pixel est déja dans le dictionnaire
                    {

                        frequencePixels[pixel]++;//S'il est deja dedans alors on incremente sa fréquence 
                    }
                    else
                    {
                        frequencePixels.Add(pixel, 1);//sinon on le crée et on lui associe une fréquence de 1
                    }
                }
            }
            ConstruireArbreHuffman();//on lance la constrution de l'arbre
        }


        /// <summary>
        /// La fonction va construire l'arbre en fonction des noeuds et des fréquences jusqu'a qu'il reste qu'un seul noeud (la racine)
        /// </summary>
        private void ConstruireArbreHuffman()
        {
            List<Noeud> noeuds = new List<Noeud>();
            foreach (KeyValuePair<Pixel, int> kvp in frequencePixels)
            {
                Noeud n = new Noeud(kvp.Key, kvp.Value);//on crée un nouveau noeud pour chaque pixel avec sa fréquence associé
                noeuds.Add(n);//on l'ajoute à la liste              
            }

            while (noeuds.Count > 1)//tant qu'il est reste plus de un noeud on continue
            {
                noeuds.Sort((n1, n2) => n1.Frequence.CompareTo(n2.Frequence));//on trie les noeuds par frequence
                Noeud nGauche = noeuds[0];// on recupere les deux avec la frequence la plus faible
                Noeud nDroit = noeuds[1];// on recupere les deux avec la frequence la plus faible
                Noeud nParent = new Noeud(null, nGauche.Frequence + nDroit.Frequence);//on créé le noeud parent à partir des deux plus petits noeuds 
                nParent.Gauche = nGauche;
                nParent.Droit = nDroit;
                noeuds.RemoveRange(0, 2);//on supprime les deux noeuds les plus petits
                noeuds.Add(nParent);//et on rajoute le noeud parent à la liste
                if (noeuds.Count == 1) 
                {
                    racine = noeuds[0];//si c'est le dernier noeud alors il devient la racine de l'arbre
                    break;
                }
            }
            racine = noeuds[0]; //si c'est le dernier noeud alors il devient la racine de l'arbre
        }


        /// <summary>
        /// Fonction qui genere toute la table de codage pour pouvoir encoder chaque chemin 
        /// </summary>
        /// <returns></returns>
        public Dictionary<Pixel, string> GenererTableCodage()
        {
            Dictionary<Pixel, string> table = new Dictionary<Pixel, string>(new PixelComparer());//on utilise aussi le PixelComparer pour regrouper les pixels identiques
            GenererTableCodageRecursive(racine, "", table);//on appele la fonction recursive qui va "explorer" l'arbre
            return table;
        }


        private void GenererTableCodageRecursive(Noeud n, string code, Dictionary<Pixel, string> table)
        {
            if (n.EstFeuille())
            {
                table.Add(n.Pixel, code);//si l'arbre est feuille on ajoute le pixel dans la liste (on ne peut plus remonter)
            }
            else
            {
                GenererTableCodageRecursive(n.Gauche, code + "0", table);//sinon on explore à gauche et à droite
                GenererTableCodageRecursive(n.Droit, code + "1", table);//sinon on explore à gauche et à droite
            }
        }


        /// <summary>
        /// Fonction qui encode chaque chemin de l'arbre 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public string Encoder(MyImage image)
        {
            Dictionary<Pixel, string> table = GenererTableCodage();//on reécupère la table de codage 
            string resultat = "";
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)//on parcourt toute l'image
                {
                    Pixel pixel = image.pixels[i,j];
                    resultat += table[pixel];//on rajoute les pixels dans la string resulat
                }
            }
            return resultat;
        }


        /// <summary>
        /// la fonction inverse de l'encoder
        /// </summary>
        /// <param name="code"></param>
        /// <param name="largeur"></param>
        /// <param name="hauteur"></param>
        /// <returns></returns>
        public Pixel[,] Decoder(string code, int h, int w)
        {
            Pixel[,] resultat = new Pixel[h, w];//on recrée la matrice de pixel
            Noeud n = racine;//on part de la racine 
            int index = 0;
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    while (!n.EstFeuille())// tant que le noeud n'est pas une feuille on reconstruit le code 
                    {
                        char c = code[index];
                        if (c == '0')
                        {
                            n = n.Gauche;
                        }
                        else
                        {
                            n = n.Droit;
                        }
                        index++;
                    }
                    resultat[i,j] = new Pixel(n.Pixel.red,n.Pixel.green,n.Pixel.blue);//on recrée le tableau de pixel 
                    n = racine;
                }
            }
            return resultat; //on retourne notre tableau fini
        }
    }
}
