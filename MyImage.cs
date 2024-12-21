using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Numerics;
using System.Linq;
using Newtonsoft.Json;
using Ipfs.Http;


namespace PSI
{



    /* Constructeur et méthodes d'initialisation */



    public class MyImage
    {
        // Informations générales sur l'image
        private string imageType;
        private int fileSize;
        private int sizeOffset;
        private int width;
        private int height;
        private int bitsPerPixel;
        public Pixel[,] pixels;

        /// <summary>
        /// Constructeur pour créer une instance de MyImage à partir d'un fichier .bmp ou à partir d'une image IPFS
        /// </summary>
        /// <param name="filename"></param>
        public MyImage(string type, string filename = "coco", string cid = "bafkreieuke64c3krj2phfgodm5ud6g6prbco7vitpbhl3tjaxxp5gzjj24", int height = 0, int width = 0)
        {
            // Cas image non initialisée --> renvoie une erreur
            if (type == "file")
            {
                // Récupération du chemin jusqu'au dossier du projet
                string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
                string fullname = path + "\\" + filename + ".bmp";

                // Ouvrir du fichier en mode lecture binaire
                FileStream fileStream = new FileStream(fullname, FileMode.Open, FileAccess.Read);
                ReadImageFromStream(fileStream);
            }
            
            else if (type == "ipfs")
            {
                byte[] bytes = RetrieveFromIpfs(cid).GetAwaiter().GetResult();

                using (var tempStream = new FileStream("temp_ipfs", FileMode.Create, FileAccess.Write))
                {
                    tempStream.Write(bytes, 0, bytes.Length);
                }
                var fileStream = new FileStream("temp_ipfs", FileMode.Open, FileAccess.Read);
                ReadImageFromStream(fileStream);
                File.Delete("temp_ipfs");
            }
            
            else if (type == "new")
            {
                this.height = height;
                this.width = width;
                this.imageType = "BM";
                this.sizeOffset = 54;
                this.fileSize = sizeOffset + height * width * 3;
                // Chaque pixel contient 3 couleurs sous forme de 3 bytes. Chaque byte correspond à 8 octets (bits) donc 3 * 8
                this.bitsPerPixel = 3 * 8;
                this.pixels = new Pixel[height, width];
                this.Whiten();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        private void ReadImageFromStream(FileStream fileStream)
        {
            // Récupérer l'en-tête du fichier BMP
            ReadHeader(fileStream);

            // Récupérer le padding
            int paddingBytes = (4 - ((width * 3) % 4)) % 4;

            // Récupérer l'image (tableau de pixels)
            pixels = ReadImage(fileStream, paddingBytes);
            fileStream.Close();
        }

        /// <summary>
        /// Permet de lire l'en-tête de l'image
        /// </summary>
        /// <param name="fileStream"></param>
        public void ReadHeader(FileStream fileStream)
        {
            // Lire l'en-tête du fichier BMP
            byte[] header = new byte[54];
            fileStream.Read(header, 0, 54);

            // Extraire les informations de l'en-tête
            this.imageType = Encoding.ASCII.GetString(header, 0, 2);
            this.fileSize = ConvertEndianToInt(header, 2, 4);
            this.sizeOffset = ConvertEndianToInt(header, 10, 4);
       
            this.width = ConvertEndianToInt(header, 18, 4);
            this.height = ConvertEndianToInt(header, 22, 4);
            this.bitsPerPixel = ConvertEndianToInt(header, 28, 2);
        }


        /// <summary>
        /// Permet de lire l'image sous la forme d'un tableau de pixels
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="paddingBytes"></param>
        /// <returns></returns>
        private Pixel[,] ReadImage(FileStream fileStream, int paddingBytes)
        {
            // Allouer le tableau de pixels
            pixels = new Pixel[height, width];

            // Lire les données de l'image
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    byte blue = (byte)fileStream.ReadByte();
                    byte green = (byte)fileStream.ReadByte();
                    byte red = (byte)fileStream.ReadByte();
                    pixels[x, y] = new Pixel(red, green, blue);
                }
                // Ignorer les octets de remplissage en fin de ligne
                fileStream.Seek(paddingBytes, SeekOrigin.Current);
            }
            return pixels;
        }


        /// <summary>
        /// Convertir une séquence d'octets au format little endian en entier
        /// </summary>
        /// <param name="tab">prends en entrée le tableau de byte à convertir </param>
        /// <param name="startIndex">le premiere position à convertir</param>
        /// <param name="length">la longueur à convertir </param>
        /// <returns></returns>
        ///
        
        // Cette fonction prend en entrée un tableau de bytes, un index de départ et une longueur.
        public static int ConvertEndianToInt(byte[] tab, int startIndex, int length)
        {
            // Initialise une variable "result" qui stockera le résultat de la conversion.
            int result = 0;
            // Parcourt chaque élément du tableau "tab" entre "startIndex" et "startIndex + length - 1".
            for (int i = 0; i < length; i++)
            {
                // Pour chaque élément, ajoute sa valeur au résultat en décalant les bits de 8 * i positions.
                result += tab[startIndex + i] << (8 * i);
            }
            // Retourne le résultat de la conversion.
            return result;
        }


        /// <summary>
        /// Convertir un entier en séquence d'octets au format little endian
        /// </summary>
        /// <param name="val">valeur à traduire</param>
        /// <param name="length">longueur à traduire</param>
        /// <returns></returns>

        // Cette fonction prend en entrée un entier "val" et une longueur "length".
        public static byte[] ConvertIntToEndian(int val, int length)
        {
            // Initialise un tableau de bytes "result" qui stockera le résultat de la conversion.
            byte[] result = new byte[length];
            // Parcourt chaque élément du tableau "result".
            for (int i = 0; i < length; i++)
            {
                // Pour chaque élément, extrait le byte correspondant à la position i en décalant les bits de 8 * i positions.
                // Utilise un masque 0xFF pour obtenir les 8 bits les moins significatifs et les stocke dans le tableau "result".
                result[i] = (byte)((val >> (8 * i)) & 0xFF);
            }
            // Retourne le tableau de bytes représentant l'entier.
            return result;
        }


        /// <summary>
        /// Enregistrer l'image dans un fichier en binaire en respectant la structure du fichier BMP
        /// </summary>
        /// <param name="filename"></param>
        public string FromImageToFile(string filename)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            // string modifiedFilename = filename.Insert(filename.Length - 4, "_modified");
            string modifiedFilename = filename;
            string modifiedFullname = path + "\\" + modifiedFilename + ".bmp";

            // Créer stream sortie
            using (BinaryWriter writer = new BinaryWriter(File.Open(modifiedFullname, FileMode.Create)))
            {
                // Ecriture du header
                writer.Write(Encoding.ASCII.GetBytes(imageType)); // Type de l'image (BM pour BitMap)
                writer.Write(sizeOffset + height * width * 3); // Taille du fichier
                writer.Write((ushort)0); // Réservé
                writer.Write((ushort)0); // Réservé
                writer.Write(sizeOffset); // Offset des pixels
                writer.Write(40); // Taille de l'en-tête BITMAPINFOHEADER
                writer.Write(width); // Largeur en pixels
                writer.Write(height); // Hauteur en pixels
                writer.Write((ushort)1); // Planes
                writer.Write((ushort)bitsPerPixel); // Bits par pixel
                writer.Write(0); // Compression
                writer.Write(pixels.Length * 3); // Taille de l'image en octets
                writer.Write(0); // XResolution
                writer.Write(0); // YResolution
                writer.Write(0); // Couleurs utilisées
                writer.Write(0); // Couleurs importantes

                // Ecriture des pixels (image)
                foreach (Pixel pixel in pixels)
                {
                    writer.Write(pixel.B);
                    writer.Write(pixel.G);
                    writer.Write(pixel.R);
                }
            }
            return modifiedFullname;
        }


        
        /* Propriétés */



        public string ImageType
        {
            get { return this.imageType; }
        }

        public int FileSize
        {
            get { return this.fileSize; }
        }

        public int SizeOffset
        {
            get { return this.sizeOffset; }
        }

        public int Width
        {
            get { return this.width; }
        }

        public int Height
        {
            get { return this.height; }
        }

        public int BitsPerPixel
        {
            get { return this.bitsPerPixel; }
        }

        public Pixel[,] Pixels
        {
            get { return this.pixels; }
            set { this.pixels = value; }
        }



        /// <summary>
        /// Passage de l'image en blanc
        /// </summary>
        public void Whiten()
        {
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    pixels[x, y] = new Pixel(255, 255, 255);
                }                
            }
        }



        /// <summary>
        /// Passage de l'image en noir
        /// </summary>
        public void Blacken()
        {
            foreach (Pixel pixel in pixels)
            {
                pixel.R = 0;
                pixel.G = 0;
                pixel.B = 0;
            }
        }



        /// <summary>
        /// Coloration aléatoire de la matrice de pixel
        /// </summary>
        public void RandomColor()
        {
            Random r = new Random();
            int nbPixels = r.Next(height * width);
            int index = 0;
            int pos;
            int posX;
            int posY;

            while (index < nbPixels)
            {
                pos = r.Next(height * width);
                posX = pos / width;
                posY = pos % width;
                pixels[posX, posY].R = (byte)r.Next(255);
                pixels[posX, posY].G = (byte)r.Next(255);
                pixels[posX, posY].B = (byte)r.Next(255);
                index++;
            }     
        }



        /// <summary>
        /// Coloration en noir et blanc 
        /// </summary>
        public void BlackAndWhite()
        {
            for(int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    double averagePixel = (pixels[x, y].R + pixels[x, y].G + pixels[x, y].B) / 3;
                    if (averagePixel > 256 / 2)
                    {
                        pixels[x, y].R = 255;
                        pixels[x, y].G = 255;
                        pixels[x, y].B = 255;
                    }
                    else
                    {
                        pixels[x, y].R = 0;
                        pixels[x, y].G = 0;
                        pixels[x, y].B = 0;
                    }
                }
            }
        }



        /// <summary>
        /// Coloration en échelle de gris 
        /// </summary>
        public void GrayScale()
        {
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    byte averagePixel = (byte)((pixels[x, y].R + pixels[x, y].G + pixels[x, y].B) / 3);
                    pixels[x, y].R = averagePixel;
                    pixels[x, y].G = averagePixel;
                    pixels[x, y].B = averagePixel;
                }
            }
        }



        /* Traitement d'image : TD 3 */



        /// <summary>
        /// Rotation de l'image d'angle quelconque
        /// </summary>
        /// <param name="angle"> Angle de rotation en degrés </param>
        public void Rotate(double angle)
        {
            double angleRad = angle * Math.PI / 180.0;

            // Calculer la taille de la nouvelle image
            int newHeight = 2 * height;
            int newWidth = 2 * width;

            // Créer une nouvelle matrice de pixels de la taille de l'image après rotation
            Pixel[,] rotatedPixels = new Pixel[newHeight, newWidth];

            // Initialisation matrice avec des pixels noirs pour remplir zones non utilisées
            for (int x = 0; x < newHeight; x++)
            {
                for (int y = 0; y < newWidth; y++)
                {
                    rotatedPixels[x, y] = new Pixel(0, 0, 0);
                }
            }

            // Calculer les offsets pour centrer l'image dans la nouvelle matrice
            int offsetX = (newHeight - height) / 2;
            int offsetY = (newWidth - width) / 2;

            // Parcourir chaque pixel de la nouvelle matrice
            for (int x = 0; x < newHeight; x++)
            {
                for (int y = 0; y < newWidth; y++)
                {
                    // Calculer les coordonnées correspondantes dans l'ancienne matrice
                    double oldX = Math.Cos(angleRad) * (x - offsetX - height / 2) + Math.Sin(angleRad) * (y - offsetY - width / 2) + height / 2;
                    double oldY = -Math.Sin(angleRad) * (x - offsetX - height / 2) + Math.Cos(angleRad) * (y - offsetY - width / 2) + width / 2;

                    // Arrondir les coordonnées à l'entier le plus proche
                    int intOldX = (int)Math.Round(oldX);
                    int intOldY = (int)Math.Round(oldY);

                    // Vérifier si les coordonnées sont valides
                    if (intOldX >= 0 && intOldX < height && intOldY >= 0 && intOldY < width)
                    {
                        // Copier le pixel de l'ancienne matrice dans la nouvelle matrice
                        rotatedPixels[x, y] = pixels[intOldX, intOldY];
                    }
                }
            }

            // Mettre à jour les dimensions et la matrice de pixels
            height = newHeight;
            width = newWidth;
            pixels = rotatedPixels;
        }

        
        
        /// <summary>
        /// Redimensionnement (agrandissement ou réduction) de l'image d'un coefficient quelconque
        /// </summary>
        /// <param name="coeff"> Coefficient de redimensionnement </param>
        public void Resize(double coeff)
        {
            int newHeight = (int)(height * coeff);
            int newWidth = (int)(width * coeff);

            Pixel[,] resizedPixels = new Pixel[newHeight, newWidth];

            for (int x = 0; x < newHeight; x++)
            {
                for (int y = 0; y < newWidth; y++)
                {
                    resizedPixels[x, y] = new Pixel(0, 0, 0);
                }
            }

            for (int x = 0; x < newHeight; x++)
            {
                for (int y = 0; y < newWidth; y++)
                {
                    resizedPixels[x, y] = pixels[(int)(x / coeff), (int)(y / coeff)];
                }
            }
            height = newHeight;
            width = newWidth;
            pixels = resizedPixels;
        }


        
        /* Appliquer un filtre générique : TD 4 */

        
        /// <summary>
        /// Enumeration des differents types de convolution 
        /// </summary>
        public enum ConvolutionType
        {
            flou,
            repoussage,
            detecContours,
            renfBords
        }



        /// <summary>
        /// Fonction filtre qui va appliquer une convolution sur la matrice de pixels
        /// </summary>
        /// <param name="convolutionType">type de convolution souhaitée en parametre</param>
        public void Filter(ConvolutionType convolutionType)
        {
            double[,] matriceConv;
            if (convolutionType == ConvolutionType.flou)
            {
                // Matrice de convolution pour le flou
                matriceConv = new double[,]
                {
                    {1.0 / 9.0, 1.0 / 9.0, 1.0 / 9.0},
                    {1.0 / 9.0, 1.0 / 9.0, 1.0 / 9.0},
                    {1.0 / 9.0, 1.0 / 9.0, 1.0 / 9.0}
                };
            }
            else if (convolutionType == ConvolutionType.repoussage)
            {
                // Matrice de convolution pour le repoussage
                matriceConv = new double[,]
                {
                    {-2, -1, 0},
                    {-1, 1, 1},
                    {0, 1, 2}
                };
            }
            else if (convolutionType == ConvolutionType.detecContours)
            {
                // Matrice de convolution pour la detection des bords
                matriceConv = new double[,]
                {
                    {0, 1, 0},
                    {1, -4, 1},
                    {0, 1, 0}
                };
            }
            else
            {
                // Matrice de convolution pour le renforcement des bords
                matriceConv = new double[,]
                {
                    {0, 0, 0},
                    {-1, 1, 0},
                    {0, 0, 0}
                };
            }

            int matriceTaille = matriceConv.GetLength(0);
            int matriceRayon = matriceTaille / 2;
           

            // Copie la matrice de pixels 
            Pixel[,] copiePixels = new Pixel[height, width];
            
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    copiePixels[x, y] = new Pixel(pixels[x, y].R, pixels[x, y].G, pixels[x, y].B);
                }
            }

            // Application de la convolution a chaque pixel
            for (int x = matriceRayon; x < height - matriceRayon; x++)
            {
                for (int y = matriceRayon; y < width - matriceRayon; y++)
                {
                    double sommeR = 0.0;
                    double sommeG = 0.0;
                    double sommeB = 0.0;
                    for (int i = 0; i < matriceTaille; i++)
                    {
                        for (int j = 0; j < matriceTaille; j++)
                        {
                            int pixelX = x + i - matriceRayon;
                            int pixelY = y + j - matriceRayon;
                            sommeR += matriceConv[i, j] * pixels[pixelX, pixelY].R;//Convolution
                            sommeG += matriceConv[i, j] * pixels[pixelX, pixelY].G;//Convolution
                            sommeB += matriceConv[i, j] * pixels[pixelX, pixelY].B;//Convolution
                        }
                    }
                    // Vérifie que les valeurs de chaque pixel sont comprises entre 0 et 255
                    copiePixels[x, y].R = (byte)Math.Round(Math.Min(Math.Max(sommeR, 0), 255));
                    copiePixels[x, y].G = (byte)Math.Round(Math.Min(Math.Max(sommeG, 0), 255));
                    copiePixels[x, y].B = (byte)Math.Round(Math.Min(Math.Max(sommeB, 0), 255));
                }
            }

            // Copie des pixels modifiés dans la matrice de pixels d'origine
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    pixels[x, y] = copiePixels[x, y];
                }
            }
        }



        /* Créer ou extraire une image nouvelle (TD 5) */



        /// <summary>
        /// Fonction fractale pour les triangles de Sierpinski
        /// </summary>
        public void Sierpinski()
        {
            // création de la matrice de pixels
            Pixel[,] image2 = new Pixel[height, width];
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    pixels[x, y] = new Pixel(255, 255, 255);
                }
            }

            // coordonnées des sommets du triangle
            int x1 = 0;
            int y1 = width - 1;
            int x2 = height - 1;
            int y2 = width - 1;
            int x3 = height / 2;
            int y3 = 0;
            // dessin du triangle initial
            for (int y = y1; y < y2 - y1; y++)
            {
                for (int x = x1; x < x2 - x1; x++)
                {
                    pixels[y, x] = new Pixel(255, 255, 255);
                }
            }
            for (int y = y2; y < y3 - y2; y++)
            {
                for (int x = x2; x < x3 - x2; x++)
                {
                    pixels[y, x] = new Pixel(255, 255, 255);
                }
            }
            for (int y = y3; y < y1 - y3; y++)
            {
                for (int x = x3; x < x1 - x3; x++)
                {
                    pixels[y, x] = new Pixel(255, 255, 255);
                }
            }
            int iterations = 12;
            SierpinskiRecu(image2, x1, y1, x2, y2, x3, y3, iterations);
        }



        /// <summary>
        /// Appel récursif pour les triangles 
        /// </summary>
        /// <param name="image2"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        /// <param name="iterations"></param>
        private void SierpinskiRecu(Pixel[,] image2, int x1, int y1, int x2, int y2, int x3, int y3, int iterations)
        {
            if (iterations == 0)
            {
                return;
            }
            else
            {
                // calcul des coordonnées des milieux des côtés du triangle
                int x12 = (x1 + x2) / 2;
                int y12 = (y1 + y2) / 2;
                int x23 = (x2 + x3) / 2;
                int y23 = (y2 + y3) / 2;
                int x31 = (x3 + x1) / 2;
                int y31 = (y3 + y1) / 2;
                // dessin des nouveaux triangles
                for (int x = x12; x <= x23; x++)
                {
                    for (int y = y12; y <= y31; y++)
                    {
                        pixels[x, y] = new Pixel(0, 0, 0);
                    }
                }

                for (int x = x23; x <= x31; x++)
                {
                    for (int y = y23; y <= y12; y++)
                    {
                        pixels[x, y] = new Pixel(0, 0, 0);
                    }
                }

                for (int x = x31; x <= x12; x++)
                {
                    for (int y = y31; y <= y23; y++)
                    {
                        pixels[x, y] = new Pixel(0, 0, 0);
                    }
                }
                SierpinskiRecu(pixels, x1, y1, x12, y12, x31, y31, iterations - 1);
                SierpinskiRecu(pixels, x12, y12, x2, y2, x23, y23, iterations - 1);
                SierpinskiRecu(pixels, x31, y31, x23, y23, x3, y3, iterations - 1);
            }
        }



        /// <summary>
        /// Fonction pour l'ensemble de mandelbrot ( ensemble des suites convergentes complexes)
        /// </summary>
        /// <param name="maxIterations"></param>
        public void Mandelbrot(int maxIterations)
        {

            // boucle sur les pixels de l'image
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // conversion des coordonnées du pixel en nombre complexe
                    Complex c = new Complex(
                       x * (3.0 / width) - 2.0,
                       y * (2.0 / height) - 1.0);

                    // initialisation de la suite
                    Complex z = Complex.Zero;

                    // itération de la fonction complexe
                    int iterations = 0;
                    while (z.Magnitude < 2.0 && iterations < maxIterations)
                    {
                        z = z * z + c;
                        iterations++;
                    }

                    // coloration du pixel en fonction du nombre d'itérations
                    if (iterations == maxIterations)
                    {
                        // le point appartient probablement à l'ensemble de Mandelbrot
                        pixels[y, x] = new Pixel(0, 0, 0);
                    }
                    else
                    {
                        // le point n'appartient pas à l'ensemble de Mandelbrot
                        // on utilise une interpolation linéaire pour déterminer la couleur
                        double t = (double)iterations / maxIterations;
                        int r = (int)(9 * (1 - t) * t * t * t * 255);
                        int g = (int)(15 * (1 - t) * (1 - t) * t * t * 255);
                        int b = (int)(8.5 * (1 - t) * (1 - t) * (1 - t) * t * 255);

                        pixels[y, x] = new Pixel((byte)r, (byte)g, (byte)b);
                    }
                }
            }
        }


        /// <summary>
        /// Fonction pour l'ensemble de Julia (meme principe que mandelbrot mais avec une suite de référence différente)
        /// </summary>
        /// <param name="maxIterations"></param>
        public void Julia(int maxIterations)
        {
            double cx = -0.8;
            double cy = 0.156;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // conversion des coordonnées du pixel en nombre complexe
                    Complex c = new Complex(
                        (x - (width / 2.0)) / (width / 4.0),
                        (y - (height / 2.0)) / (height / 4.0));

                    // initialisation de la suite
                    Complex z = c;

                    // itération de la fonction complexe
                    int iterations = 0;
                    while (z.Magnitude < 2.0 && iterations < maxIterations)
                    {
                        z = z * z + new Complex(cx, cy);
                        iterations++;
                    }

                    // coloration du pixel en fonction du nombre d'itérations
                    if (iterations == maxIterations)
                    {
                        // le point appartient probablement à l'ensemble de Julia
                        pixels[y, x] = new Pixel(0, 0, 0);
                    }
                    else
                    {
                        // le point n'appartient pas à l'ensemble de Julia
                        // on utilise une interpolation linéaire pour déterminer la couleur
                        double t = (double)iterations / maxIterations;
                        int r = (int)(9 * (1 - t) * t * t * t * 255);
                        int g = (int)(15 * (1 - t) * (1 - t) * t * t * 255);
                        int b = (int)(8.5 * (1 - t) * (1 - t) * (1 - t) * t * 255);
                        pixels[y, x] = new Pixel((byte)r, (byte)g, (byte)b);
                    }
                }
            }
        }



        /// <summary>
        /// Cacher un texte dans une image de manière aléatoire
        /// </summary>
        /// <param name="text"> Message à cacher </param>
        /// <returns> Clé permettant de retrouver le message caché </returns>
        public string SteganoHideText(string text)
        {
            byte[] bytesToHide = Encoding.UTF8.GetBytes(text);

            if (width * height < bytesToHide.Length * 8)
            {
                throw new Exception("L'image est trop petite pour cacher ces données");
            }

            Pixel[,] stegano = pixels;

            // Génération aléatoire d'une seed
            Random rndTemp = new Random();
            int seed = rndTemp.Next();

            // Générateur aléatoire à partir de la seed
            Random rnd = new Random(seed);

            int[] positions = new int[bytesToHide.Length * 8];

            for (int i = 0; i < bytesToHide.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int position;
                    do
                    {
                        position = rnd.Next(0, width * height);
                    } while (positions.Contains(position));

                    int pixelX = position / width;
                    int pixelY = position % width;

                    positions[i * 8 + j] = position;

                    // Mettre le j-ème bit de bytesToHide[i] dans le LSB de la couleur rouge du pixel
                    stegano[pixelX, pixelY].R = (byte)((stegano[pixelX, pixelY].R & ~1) | ((bytesToHide[i] >> j) & 1));
                }
            }
            pixels = stegano;
            return $"{seed};{bytesToHide.Length}";
        }



        /// <summary>
        /// Récupérer un texte caché dans une image à partir d'une clé
        /// </summary>
        /// <param name="key"> Clé permettant de récupérer le message </param>
        /// <returns> Message caché </returns>
        public string SteganoShowText(string key)
        {
            int seed;
            int bytesToReveal;
            try
            {
                string[] parts = key.Split(';');
                seed = int.Parse(parts[0]);
                bytesToReveal = int.Parse(parts[1]);
            }
            catch (Exception e)
            {
                throw new Exception($"{e}");
            }
            
            Random rnd = new Random(seed);

            byte[] bytes = new byte[bytesToReveal];
            int[] positions = new int[bytesToReveal * 8];

            for (int i = 0; i < bytesToReveal; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int position;
                    do
                    {
                        position = rnd.Next(0, width * height);
                    } while (positions.Contains(position));

                    int pixelX = position / width;
                    int pixelY = position % width;

                    positions[i * 8 + j] = position;

                    byte bit = (byte)(pixels[pixelX, pixelY].R & 1);
                    bytes[i] |= (byte)(bit << j);
                }
            }
            return Encoding.UTF8.GetString(bytes);
        }



        /// <summary>
        /// Cacher une image dans une autre image de manière aléatoire
        /// </summary>
        /// <param name="toHide"> Image à cacher </param>
        /// <returns> Clé permettant de retrouver l'image cachée </returns>
        public string SteganoHideImage(MyImage toHide)
        {
            if (height < toHide.Height || width < toHide.Width)
            {
                throw new Exception("The image is too small to hide this data");
            }

            // Génération aléatoire d'une seed 
            Random rndTemp = new Random();
            int seed = rndTemp.Next();

            // Générateur aléatoire à partir de la seed
            Random rnd = new Random(seed);

            int[] positions = new int[this.height * this.width];
            int index = 0;

            for (int x = 0; x < toHide.Height; x++)
            {
                for (int y = 0; y < toHide.Width; y++)
                {
                    int position;
                    do
                    {
                        position = rnd.Next(0, this.height * this.width);
                    } while (positions.Contains(position));

                    positions[index] = position;
                    index++;

                    int pixelX = position / this.width;
                    int pixelY = position % this.width;

                    byte byteOriginRed = this.pixels[pixelX, pixelY].R;
                    byte byteHideRed = toHide.Pixels[x, y].R;

                    byte byteOriginGreen = this.pixels[pixelX, pixelY].G;
                    byte byteHideGreen = toHide.Pixels[x, y].G;

                    byte byteOriginBlue = this.pixels[pixelX, pixelY].B;
                    byte byteHideBlue = toHide.Pixels[x, y].B;

                    pixels[pixelX, pixelY].R = (byte)((byteOriginRed & 0xF0) | ((byteHideRed & 0xF0) >> 4));
                    pixels[pixelX, pixelY].G = (byte)((byteOriginGreen & 0xF0) | ((byteHideGreen & 0xF0) >> 4));
                    pixels[pixelX, pixelY].B = (byte)((byteOriginBlue & 0xF0) | ((byteHideBlue & 0xF0) >> 4));
                }
            }
            return $"{seed};{toHide.Height};{toHide.Width}";
        }



        /// <summary>
        /// Récupérer une image cachée dans une autre image à partir d'une clé
        /// </summary>
        /// <param name="key"> Clé permettant de récupérer l'image cachée </param>
        /// <returns> Image cachée </returns>
        public MyImage SteganoShowImage(string key)
        {
            // Extrait la seed et les dimensions de l'image cachée
            string[] parts = key.Split(';');
            int seed = int.Parse(parts[0]);

            int hiddenHeight = int.Parse(parts[1]);

            int hiddenWidth = int.Parse(parts[2]);

            // Re-crée l'image cachée
            MyImage hidden = new MyImage("new", height: hiddenHeight, width: hiddenWidth);

            // Créer un générateur de nombre pseudo-aléatoires basé sur la seed --> Nombre générés sont les mêmes que ceux SteganoHideImage
            Random rnd = new Random(seed);

            int[] positions = new int[this.height * this.width];

            int index = 0;

            for (int x = 0; x < hidden.Height; x++)
            {
                for (int y = 0; y < hidden.Width; y++)
                {
                    int position;
                    do
                    {
                        position = rnd.Next(0, this.height * this.width);
                    } while (positions.Contains(position));

                    positions[index] = position;
                    index++;


                    int pixelX = position / this.width;
                    int pixelY = position % this.width;

                    byte byteOriginRed = this.pixels[pixelX, pixelY].R;
                    byte byteOriginGreen = this.pixels[pixelX, pixelY].G;
                    byte byteOriginBlue = this.pixels[pixelX, pixelY].B;

                    hidden.Pixels[x, y].R = (byte)((byteOriginRed & 0x0F) << 4);
                    hidden.Pixels[x, y].G = (byte)((byteOriginGreen & 0x0F) << 4);
                    hidden.Pixels[x, y].B = (byte)((byteOriginBlue & 0x0F) << 4);
                }
            }
            return hidden;
        }



        /* Innovation */



        /// <summary>
        /// Envoyer une image sur IPFS
        /// </summary>
        /// <returns> URL de la gateway/site permettant de consulter l'image envoyée sur IPFS </returns>
        public async Task<string> UploadToIpfs()
        {
            string tempFileName = "temp_ipfs";
            FromImageToFile(tempFileName);
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string fullname = path + "\\" + tempFileName + ".bmp";

            string apiKey = "e40c7582-21a8-449a-b6fe-ceedba2e754f";

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.nftport.xyz/v0/files"),
                Headers =
                {
                    { "accept", "application/json" },
                    { "Authorization", apiKey },
                }
            };

            var fileStream = File.OpenRead(fullname);
            var form = new MultipartFormDataContent();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/bmp");
            form.Add(fileContent, "file", tempFileName);

            request.Content = form;
            try
            {
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var json = await JsonConvert.DeserializeObject<dynamic>(content);
                string ipfsUrl = await json.ipfs_url;
                return "Copiez cela dans votre navigateur web : " + ipfsUrl;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "none";
            }
        }



        /// <summary>
        /// Récupérer une image depuis IPFS à partir de son CID
        /// </summary>
        /// <param name="cid"> cid de l'image à récupérer </param>
        /// <returns> Tableau de byte représentant l'image </returns>
        public async Task<byte[]> RetrieveFromIpfs(string cid)
        {
            var ipfs = new IpfsClient("https://gateway.ipfs.io");
            var file = await ipfs.FileSystem.ReadFileAsync(cid);
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }





        /* JPEG - début de développement non abouti */



        
        /// <summary>
        /// Fonciton qui transoforme le RGB en YCBCR
        /// </summary>
        public void TransfoCouleur()
        {
            Pixel[,] copiePixels = new Pixel[height, width];

            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    copiePixels[x, y] = new Pixel(pixels[x, y].R, pixels[x, y].G, pixels[x, y].B);//crée une copie de la matrice
                }
            }


            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int a = copiePixels[i, j].R;
                    int b = copiePixels[i, j].G;
                    int c = copiePixels[i, j].B;
                    copiePixels[i, j].R = (byte)(0.299 * a + 0.087 * b + 0.114 * c);//utilise la formule du cours pour changer de domaine 
                    copiePixels[i, j].G = (byte)(-0.1687 * a + -0.3313 * b + 0.5 * c + 128);//utilise la formule du cours pour changer de domaine 
                    copiePixels[i, j].B = (byte)(0.5 * a + -0.4187 * b + -0.0813 * c + 128);//utilise la formule du cours pour changer de domaine 

                }
            }

            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    pixels[x, y] = copiePixels[x, y];//recopie la matrice
                }
            }

        }
        /// <summary>
        /// Echantillonage JPEG , garde seulement quelques valeurs Cb et Cr (chrominance) mais toute la luminance (format 4-2-2)
        /// </summary>
        public void Echantillonage()
        {
            Pixel[,] copiePixels = new Pixel[height, width];

            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    copiePixels[x, y] = new Pixel(pixels[x, y].R, pixels[x, y].G, pixels[x, y].B);//copie de la matrice 
                }
            }
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width - 1; j++)
                {
                    if (j % 2 == 0)//pour supprimer les valeurs cb et cr des colonnes 
                    {
                        int a = copiePixels[i, j].R;
                        int b = copiePixels[i, j].G;
                        int c = copiePixels[i, j].B;
                        copiePixels[i, j].R = (byte)a;
                        copiePixels[i, j].G = (byte)((b + copiePixels[i, j + 1].G) / 2);//on fait la moyenne des cb et cr des deux pixels adjacents
                        copiePixels[i, j].B = (byte)((c + copiePixels[i, j + 1].B) / 2);//on fait la moyenne des cb et cr des deux pixels adjacents
                    }
                    else
                    {
                        int a = copiePixels[i, j].R;
                        int b = copiePixels[i, j].G;
                        int c = copiePixels[i, j].B;
                        copiePixels[i, j].R = (byte)a;
                        copiePixels[i, j].G = (byte)a;//on garde juste la luminance
                        copiePixels[i, j].B = (byte)a;
                    }

                }
            }

            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    pixels[x, y] = copiePixels[x, y];//recopie de la matrice 
                }
            }

        }
        /// <summary>
        /// Transformer cosinus discrete ou DCT en anglais et quantification en meme temps 
        /// traiement similaire à celui d'une transformer de Fourier
        /// Les foncitons ont été diviser en plusieurs pour éviter les quadruples boucles for dans tous les sens 
        /// </summary>
        public void DCTCOMP()
        {

            double[,] matriceQuant = new double[,]//matrice de quantification 
            {
                {3,5,7,9,11,13,15,17},
                {5,7,9,11,13,1,17,19},
                {7,9,11,13,15,17,19,21},
                {9,11,13,15,17,19,21,23},
                {11,13,15,17,19,21,23,25},
                {13,15,17,19,21,23,25,27},
                {15,17,19,21,23,25,27,29},
                {17,19,21,23,25,27,29,31},

            };
            double[,] matriceQuant2 = new double[,]//deuxieme matrice de quantification (utilisé majoritairement pour des tests)
            {
                {16,11,10,16,24,40,51,61},
                {12,12,14,19,26,58,60,55},
                {14,13,16,24,40,57,69,56},
                {14,17,22,29,51,87,80,62},
                {18,22,37,56,68,109,103,77},
                {24,35,55,64,81,104,113,92},
                {49,64,78,87,103,121,120,101},
                {72,92,95,98,112,100,103,99},

            };

            for (int x = 0; x < height; x += 8)
            {
                for (int y = 0; y < width; y += 8)
                {
                    int[,] sousMatrice = new int[8, 8];
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            sousMatrice[i, j] = pixels[x + i, y + j].R;//on commence par diviser l'image en plusieurs sous matrice de 8x8 pixels

                        }
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            pixels[x + i, y + j].R = (byte)(Math.Round(DCT(sousMatrice)[i, j] / matriceQuant[i, j]));//on applique la division de la matrice quant sur le resulat de la DCT
                            pixels[x + i, y + j].G = (byte)(Math.Round(DCT(sousMatrice)[i, j] / matriceQuant[i, j]));//on applique la division de la matrice quant sur le resulat de la DCT
                            pixels[x + i, y + j].B = (byte)(Math.Round(DCT(sousMatrice)[i, j] / matriceQuant[i, j]));//on applique la division de la matrice quant sur le resulat de la DCT


                        }
                    }

                }
            }
        }
        /// <summary>
        /// DCT partie calcul 
        /// </summary>
        /// <param name="matrice"></param>
        /// <returns></returns>
        public int[,] DCT(int[,] matrice)
        {
            int[,] matrice2 = new int[8, 8];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    matrice2[i, j] = (int)Calcul(i, j, matrice);//on prends chaque pixels de la matrice et on calcule la somme des cosinus associée
                }
            }
            return matrice2; // on retourne la matrice avec toutes ses valeures

        }
        /// <summary>
        /// Fonction pour le calcul brut de chaque valeur 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="matrice"></param>
        /// <returns></returns>
        public double Calcul(int i, int j, int[,] matrice)//on prends une seule valeur de la matrice en entrée (i,j nous indique la valeur)
        {
            double somme = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    double cosx = (x == 0) ? 1.0 / Math.Sqrt(2) : 1.0;//Equivalent de la fonction indicatrice mais on y assoie 1/rac(2) si une des deux valeurs vaut 0
                    double cosy = (y == 0) ? 1.0 / Math.Sqrt(2) : 1.0;//Equivalent de la fonction indicatrice mais on y assoie 1/rac(2) 
                    somme += cosx * cosy * matrice[x, y] * Math.Cos((2 * x + 1) * i * Math.PI / 16) * Math.Cos((2 * y + 1) * j * Math.PI / 16);//application de la formule 
                }
            }
            return 0.25 * somme;//on retourne 1/4 de la somme 
        }
        
    }
}