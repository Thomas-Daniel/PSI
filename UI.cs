using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PSI
{
    class UI
    {
        public MyImage image;

        /// <summary>
        /// Tous les menus fonctionnent de la même manière.
        /// Les commentaires ne seront explicités que pour le menu principal
        /// </summary>
        public UI()
        {
            MenuPrincipal();
        }


        
        /// <summary>
        /// Menu principal avec message de bienvenue et différentes options de manipulation d'image
        /// </summary>
        private async void MenuPrincipal()
        {
            string fullName = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\welcome.txt";
            string welcome = File.ReadAllText(fullName);

            // Liste des différentes options sur le menu principal
            string[] mainOptions = { "1 - Créer une image", "2 - Charger une image", "3 - Modifier une image", "4 - Cacher/Révéler des données dans une image (stéganographie)", "5 - Sauvegarder une image", "6 - Ouvrir une image","7 - Quitter" };
            int selectedIndex = 0;

            // Cache le curseur de l'utilisateur
            Console.CursorVisible = false;
            while (true)
            {
                Console.Clear();
                // Affiche le message de bienvenue contenu dans le fichier "welcome.txt"
                Console.WriteLine(welcome);

                // Afficher les options du menu
                for (int i = 0; i < mainOptions.Length; i++)
                {
                    // Surligne en vert l'option sélectionnée
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine(mainOptions[i]);
                    Console.ResetColor();
                }

                // Capturer l'entrée de l'utilisateur
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                // Déplacer le curseur en fonction de l'entrée de l'utilisateur
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = Math.Max(0, selectedIndex - 1);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = Math.Min(mainOptions.Length - 1, selectedIndex + 1);
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Option sélectionnée : {mainOptions[selectedIndex]}\n");
                        Console.ResetColor();
                        
                        // Action en fonction du choix utilisateur
                        string selection = mainOptions[selectedIndex];
                        switch (selection)
                        {
                            case "1 - Créer une image":
                                Console.WriteLine("Entrez les dimensions (hauteur et largeur) de l'image à créer\n");
                                int height = SecureIntInput();
                                int width = SecureIntInput();
                                this.image = new MyImage("new", height: height, width: width);
                                CreateImageMenu();
                                break;

                            case "2 - Charger une image":
                                LoadImageMenu();
                                break;

                            case "3 - Modifier une image":
                                ModifyMenu();
                                break;

                            case "4 - Cacher/Révéler des données dans une image (stéganographie)":
                                SteganoMenu();
                                break;

                            case "5 - Sauvegarder une image":
                                string tempo = await SaveImageMenu();
                                Console.WriteLine(tempo);
                                break;
                            case "6 - Ouvrir une image":
                                string filename = OpenFileMenu();
                                string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
                          
                                Process.Start("explorer.exe", $"{path}\\{filename}.bmp");
                                break;
                            case "7 - Quitter":
                                return;
                        }
                        MenuPrincipal();
                        return;
                }
            }
        }



        /// <summary>
        /// Menu de création d'images
        /// </summary>
        private void CreateImageMenu()
        {
            string[] options = { "1 - Créer une image noire", "2 - Créer une image blanche", "3 - Créer une image multicolore", "4 - Créer une fractale", "5 - Retour" };
            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();                
                // Afficher les options du menu
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine(options[i]);
                    Console.ResetColor();
                }

                // Capturer l'entrée de l'utilisateur
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                // Déplacer le curseur en fonction de l'entrée de l'utilisateur
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = Math.Max(0, selectedIndex - 1);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = Math.Min(options.Length - 1, selectedIndex + 1);
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.ResetColor();

                        string selection = options[selectedIndex];
                        switch (selection)
                        {
                            case "1 - Créer une image noire":
                                this.image.Blacken();
                                break;

                            case "2 - Créer une image blanche":
                                this.image.Whiten();
                                break;

                            case "3 - Créer une image multicolore":
                                this.image.RandomColor();
                                break;

                            case "4 - Créer une fractale":                          
                                FractaleMenu();                                
                                break;

                            case "5 - Retour":
                                return;
                        }
                        Console.WriteLine("Appuyez sur ENTRER pour continuer");
                        // Permet à l'utilisateur de voir le résultat des ses actions avant de retourner au menu principal
                        Console.ReadLine();
                        return;
                }
            }
        }



        /// <summary>
        /// Menu des fractales
        /// </summary>
        private void FractaleMenu()
        {
            string[] options = { "1 - Sierpinski", "2 - Mandelbrot", "3 - Julia", "4 - Retour" };
            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();

                Console.WriteLine("Quel modèle de fractale souhaitez-vous générer ?\n");
                // Afficher les options du menu
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine(options[i]);
                    Console.ResetColor();
                }

                // Capturer l'entrée de l'utilisateur
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                // Déplacer le curseur en fonction de l'entrée de l'utilisateur
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = Math.Max(0, selectedIndex - 1);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = Math.Min(options.Length - 1, selectedIndex + 1);
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Option sélectionnée : {options[selectedIndex]}\n");
                        Console.ResetColor();

                        string fractaleChoice = "";

                        string selection = options[selectedIndex];
                        switch (selection)
                        {
                            case "1 - Sierpinski":
                                fractaleChoice = "Sierpinski";
                                this.image.Sierpinski();
                                break;

                            case "2 - Mandelbrot":
                                fractaleChoice = "Mandelbrot";
                                this.image.Mandelbrot(200);
                                break;

                            case "3 - Julia":
                                fractaleChoice = "Julia";
                                this.image.Julia(200);
                                break;

                            case "4 - Retour":
                                return;
                        }
                        Console.WriteLine($"La fractale de {fractaleChoice} de dimension {image.Width}x{image.Height} a été générée avec succès");
                        Console.WriteLine("Appuyez sur ENTRER pour continuer");
                        // Permet à l'utilisateur de voir le résultat des ses actions avant de retourner au menu principal
                        Console.ReadLine();
                        return;
                }
            }
        }



        /// <summary>
        /// Menu de chargement d'une image
        /// </summary>
        private void LoadImageMenu()
        {
            string[] options = { "1 - Charger une image depuis un fichier", "2 - Charger une image depuis IPFS", "3 - Retour" };
            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();

                // Afficher les options du menu
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine(options[i]);
                    Console.ResetColor();
                }

                // Capturer l'entrée de l'utilisateur
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                // Déplacer le curseur en fonction de l'entrée de l'utilisateur
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = Math.Max(0, selectedIndex - 1);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = Math.Min(options.Length - 1, selectedIndex + 1);
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.ResetColor();

                        string selection = options[selectedIndex];
                        switch (selection)
                        {
                            case "1 - Charger une image depuis un fichier":
                                string filename = OpenFileMenu();
                                this.image = new MyImage("file", filename: filename);
                                break;

                            case "2 - Charger une image depuis IPFS":
                                Console.WriteLine("IPFS (InterPlanetary File System) est un protocole pair à pair de distribution de contenu qui ne dépend pas de serveurs centralisés)\nEntrez le cid (Content Identifiers) du fichier stocké sur IPFS\n");
                                string cid = Console.ReadLine();
                                image = new MyImage("ipfs", cid: cid);
                                break;

                            case "3 - Retour":
                                return;
                        }
                        Console.WriteLine("L'image a été chargée avec succès");
                        Console.WriteLine("Appuyez sur ENTRER pour continuer");
                        // Permet à l'utilisateur de voir le résultat des ses actions avant de retourner au menu principal
                        Console.ReadLine();
                        return;
                }
            }
        }



        /// <summary>
        /// Menu d'ouverture d'image avec affichage des images dans le répertoire courant
        /// </summary>
        /// <returns> Nom du fichier à ouvrir choisi par l'utilisateur </returns>
        public string OpenFileMenu()
        {
            string imageName = "";
            Dictionary<string, Action> dispatchTable = new Dictionary<string, Action>();
            string[] options = FileList();
            int selectedIndex = 0;

            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                dispatchTable.Add(options[i], () =>
                {
                    imageName = options[index];
                    Console.WriteLine($"Le fichier {options[index]} a été ouvert avec succès.");
                });
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Choisissez un fichier à ouvrir parmi les suivants (dans le dossier du projet) :\n");

                // Afficher les options du menu
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine(options[i]);
                    Console.ResetColor();
                }

                // Capturer l'entrée de l'utilisateur
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                // Déplacer le curseur en fonction de l'entrée de l'utilisateur
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = Math.Max(0, selectedIndex - 1);
                        break;

                    case ConsoleKey.DownArrow:
                        selectedIndex = Math.Min(options.Length - 1, selectedIndex + 1);
                        break;

                    case ConsoleKey.Enter:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.ResetColor();

                        string selection = options[selectedIndex];
                        if (dispatchTable.ContainsKey(selection))
                        {
                            dispatchTable[selection]();
                        }
                        // Enlève extension .bmp du nom de l'image
                        string cleaned = Path.GetFileNameWithoutExtension(imageName);
                        return cleaned;
                }
            }
        }



        /// <summary>
        /// Menu de modification d'image
        /// </summary>
        private void ModifyMenu()
        {
            if (this.image is null)
            {
                Console.WriteLine("Vous n'avez pas créé ou chargé d'image\nRetour au menu principal");
                Console.ReadLine();
                return;
            }
            string[] options = { "1 - Noir et blanc", "2 - Gris", "3 - Rotation", "4 - Agrandissement/Réduction", "5 - Détection de contours", "6 - Renforcement des bords", "7 - Flou", "8 - Repoussage", "9 - Huffman", "10 - RandomCouleur","11 - Retour" };
            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Quelle(s) modification(s) souhaitez-vous faire ?");

                // Afficher les options du menu
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine(options[i]);
                    Console.ResetColor();
                }

                // Capturer l'entrée de l'utilisateur
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                // Déplacer le curseur en fonction de l'entrée de l'utilisateur
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = Math.Max(0, selectedIndex - 1);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = Math.Min(options.Length - 1, selectedIndex + 1);
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Option sélectionnée : {options[selectedIndex]}\n");
                        Console.ResetColor();

                        string selection = options[selectedIndex];
                        switch (selection)
                        {
                            case "1 - Noir et blanc":
                                image.BlackAndWhite();
                                Console.WriteLine("L'image a été passée en noir et blanc");
                                break;

                            case "2 - Gris":
                                image.GrayScale();
                                Console.WriteLine("L'image a été passée à une échelle de gris");
                                break;

                            case "3 - Rotation":
                                Console.WriteLine("De combien de degrés souhaitez-vous faire tourner l'image (sens horaire) ?\n");
                                double angle = SecureDoubleInput();
                                image.Rotate(angle);
                                Console.WriteLine($"L'image a été tournée de {angle} degrés");
                                break;

                            case "4 - Agrandissement/Réduction":
                                Console.WriteLine("De quel facteur voulez-vous agrandir/réduire l'image ?\n");
                                double coeff = SecureDoubleInput();
                                image.Resize(coeff);
                                if (coeff > 1){Console.WriteLine($"L'image a été agrandie d'un coefficient {coeff}");}
                                else if (coeff == 1) {Console.WriteLine("L'image n'a pas été modifiée");}
                                else {Console.WriteLine($"L'image a été réduite d'un coefficient {coeff}");}                                
                                break;

                            case "5 - Détection de contours":
                                image.Filter(MyImage.ConvolutionType.detecContours);
                                Console.WriteLine("L'image a été modifiée");
                                break;

                            case "6 - Renforcement des bords":
                                image.Filter(MyImage.ConvolutionType.renfBords);
                                Console.WriteLine("L'image a été modifiée");
                                break;

                            case "7 - Flou":
                                image.Filter(MyImage.ConvolutionType.flou);
                                Console.WriteLine("L'image a été modifiée");
                                break;

                            case "8 - Repoussage":
                                image.Filter(MyImage.ConvolutionType.repoussage);
                                Console.WriteLine("L'image a été modifiée");
                                break;

                            case "9 - Huffman":
                                HuffmanMenu();
                                return;
                            case "10 - RandomCouleur":
                                image.Whiten();
                                image.RandomColor();
                                return;
                            case "11 - Retour":
                                return;
                        }
                        Console.WriteLine("Appuyez sur ENTRER pour continuer");
                        // Permet à l'utilisateur de voir le résultat des ses actions avant de retourner au menu principal
                        Console.ReadLine();
                        return;
                }
            }
        }



        /// <summary>
        /// Menu pour la compression/décompression Huffman
        /// </summary>
        private void HuffmanMenu()
        {
            string[] options = { "1 - Compression", "2 - Décompression", "3 - Retour" };
            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();

                // Afficher les options du menu
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine(options[i]);
                    Console.ResetColor();
                }

                // Capturer l'entrée de l'utilisateur
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                // Déplacer le curseur en fonction de l'entrée de l'utilisateur
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = Math.Max(0, selectedIndex - 1);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = Math.Min(options.Length - 1, selectedIndex + 1);
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Option sélectionnée : {options[selectedIndex]}\n");
                        Console.ResetColor();

                        string key;
                        string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
                        string imageFile = $"huffman_compressed";
                        string codeFile = $"{path}\\huffman_code.txt";
                        Huffman huffman;

                        string selection = options[selectedIndex];
                        switch (selection)
                        {
                            case "1 - Compression":
                                Console.WriteLine("Compression en cours");
                                huffman = new Huffman(this.image);
                                key = $"{huffman.Encoder(this.image)};{this.image.Height};{this.image.Width}";
                                // Sauvegarde l'image originale
                                this.image.FromImageToFile(imageFile);

                                // Ecris la clé dans le fichier huffman_compressed.txt
                                File.WriteAllText(codeFile, key);
                                Console.WriteLine("Votre image compressée a été enregistrée dans le répertoire courant sous le nom : huffman_compressed.txt");
                                break;

                            case "2 - Décompression":
                                // Récupère les informations de la clé (données image compressée, hauteur et largeur de l'image)
                                string[] parts = File.ReadAllText(codeFile).Split(';');
                                
                                // Crée image qui sera ensuite l'image décompressée 
                                MyImage decode = new MyImage("new", height: int.Parse(parts[1]), width: int.Parse(parts[2]));
                                // Récupère image compressée
                                MyImage huff = new MyImage("file", filename: imageFile);
                                huffman = new Huffman(huff);                
                                // Le tableau de pixels de decode devient celui de l'image compressée qui vient d'être décompressée
                                decode.Pixels = huffman.Decoder(parts[0], int.Parse(parts[1]), int.Parse(parts[2]));
                                decode.FromImageToFile("huffman_uncompressed");
                                Console.WriteLine("Votre image décompressée a été enregistrée dans le répertoire courant sous le nom : huffman_uncompressed.bmp");
                                break;

                            case "3 - Retour":                                
                                return;
                        }
                        Console.WriteLine("Appuyez sur ENTRER pour continuer");
                        // Permet à l'utilisateur de voir le résultat des ses actions avant de retourner au menu principal
                        Console.ReadLine();
                        return;
                }
            }
        }



        /// <summary>
        /// Menu pour la stéganographie
        /// </summary>
        private void SteganoMenu()
        {
            if (this.image is null)
            {
                Console.WriteLine("Vous n'avez pas créé ou chargé d'image\nRetour au menu principal");
                Console.ReadLine();
                return;
            }
            string[] options = { "1 - Cacher du texte", "2 - Récupérer du texte", "3 - Cacher une image", "4 - Récupérer une image", "5 - Retour" };
            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();

                // Afficher les options du menu
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine(options[i]);
                    Console.ResetColor();
                }

                // Capturer l'entrée de l'utilisateur
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                // Déplacer le curseur en fonction de l'entrée de l'utilisateur
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = Math.Max(0, selectedIndex - 1);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = Math.Min(options.Length - 1, selectedIndex + 1);
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Option sélectionnée : {options[selectedIndex]}\n");
                        Console.ResetColor();

                        string msg, key, filename;

                        string selection = options[selectedIndex];
                        switch (selection)
                        {
                            case "1 - Cacher du texte":
                                Console.WriteLine("Quel message souhaitez-vous cacher ?\n");
                                msg = Console.ReadLine();

                                try
                                {
                                    key = image.SteganoHideText(msg);
                                }
                                catch (Exception)
                                {
                                    Console.ReadLine();
                                    break;
                                }
                                Console.WriteLine($"Veuillez copier cette clé, c'est la seule manière de récupérer les données cachées puis appuyez sur ENTRER\n");
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(key + "\n");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("Pensez à sauvegarder l'image porteuse dans le menu principal");
                                break;


                            case "2 - Récupérer du texte":
                                Console.WriteLine("Entrez la clé permettant de récupérer les données\n");
                                key = Console.ReadLine();
                                try
                                {
                                    msg = image.SteganoShowText(key);
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("ERREUR : la clé est incorrecte");
                                    break;
                                }
                                Console.WriteLine($"Le message caché est : ");
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write(msg);
                                Console.ForegroundColor = ConsoleColor.White;
                                break;


                            case "3 - Cacher une image":
                                // Au cas où l'utilisateur a ouvert l'image à cacher au lieu de l'image qui cache
                                Console.WriteLine("L'image que vous manipulez actuellement est l'image porteuse du secret, si vous souhaitez la changer ou que vous vous êtes trompés avec l'image à cacher, entrez Oui (O) sinon laissez blanc ou entrez Non (N)");
                                string choice = Console.ReadLine().ToLower();
                                if (choice == "o" || choice == "oui")
                                {
                                    Console.WriteLine("Choisissez le nom du fichier de l'image porteuse");
                                    filename = OpenFileMenu();
                                    this.image = new MyImage("file", filename: filename);
                                }

                                Console.WriteLine("Choisissez l'image que vous voulez cacher");
                                filename = OpenFileMenu();
                                MyImage toHide = new MyImage("file", filename: filename);

                                try
                                {
                                    key = this.image.SteganoHideImage(toHide);
                                }
                                catch (Exception)
                                {
                                    Console.ReadLine();
                                    break;
                                }
                                Console.WriteLine($"\nVeuillez copier cette clé, c'est la seule manière de récupérer les données cachées puis appuyez sur ENTRER\n");
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(key + "\n");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("Pensez à sauvegarder l'image porteuse dans le menu principal");
                                break;


                            case "4 - Récupérer une image":
                                Console.WriteLine("Entrez la clé permettant de récupérer les données\n");
                                key = Console.ReadLine();

                                try
                                {
                                    MyImage hidden = image.SteganoShowImage(key);
                                    hidden.FromImageToFile("secret_image");
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("ERREUR : la clé est incorrecte");
                                    break;
                                }                                
                                Console.WriteLine($"L'image cachée a été enregistrée sous le nom : ");
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("secret_image.bmp");
                                Console.ForegroundColor = ConsoleColor.White;
                                break;

                            case "5 - Retour":
                                return;
                        }
                        Console.WriteLine("Appuyez sur ENTRER pour continuer");
                        // Permet à l'utilisateur de voir le résultat des ses actions avant de retourner au menu principal
                        Console.ReadLine();
                        return;
                }
            }
        }

        

        /// <summary>
        /// Menu sauvegarde image
        /// </summary>
        /// <returns></returns>
        public async Task<string> SaveImageMenu()
        {
            if (this.image is null)
            {
                Console.WriteLine("Vous n'avez pas créé ou chargé d'image\nRetour au menu principal");
                Console.ReadLine();
                return "";
            }
            string[] options = { "1 - Sauvegarder l'image dans un fichier", "2 - Sauvegarder l'image sur IPFS", "3 - Retour" };
            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();

                // Afficher les options du menu
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine(options[i]);
                    Console.ResetColor();
                }

                // Capturer l'entrée de l'utilisateur
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                // Déplacer le curseur en fonction de l'entrée de l'utilisateur
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = Math.Max(0, selectedIndex - 1);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = Math.Min(options.Length - 1, selectedIndex + 1);
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Option sélectionnée : {options[selectedIndex]}\n");
                        Console.ResetColor();

                        string selection = options[selectedIndex];
                        switch (selection)
                        {
                            case "1 - Sauvegarder l'image dans un fichier":
                                Console.WriteLine("Entrez le nom du fichier où l'image sera sauvegardée (attention à ne pas écraser une image préexistante)\n");
                                string filename = Console.ReadLine();

                                string fullname = this.image.FromImageToFile(filename);
                                Process.Start("explorer.exe", fullname);
                                break;

                            case "2 - Sauvegarder l'image sur IPFS":
                                string ipfsUrl = await image.UploadToIpfs();
                                Console.WriteLine("Voici le lien de votre image sur la gateway ipfs.io : " + ipfsUrl);
                                break;

                            case "3 - Retour":
                                return "";
                        }
                        Console.WriteLine("Appuyez sur ENTRER pour continuer");
                        // Permet à l'utilisateur de voir le résultat des ses actions avant de retourner au menu principal
                        Console.ReadLine();
                        return "";
                }
            }
        }
        


        /// <summary>
        /// Saisie sécurisée d'entier
        /// </summary>
        /// <returns> Entier entré par l'utilisateur </returns>
        public int SecureIntInput()
        {
            string input;
            int value = 0;
            
            while (true)
            {
                input = Console.ReadLine();
                try
                {
                    value = int.Parse(input);
                    if (value > 0)
                    {
                        return value;
                    }
                    Console.WriteLine("La valeur entrée n'est pas strictement supérieure à 0");
                }
                catch (Exception)
                {
                    Console.WriteLine("La valeur entrée n'est pas un entier");
                }
            }
        }



        /// <summary>
        /// Saisie sécurisée double
        /// </summary>
        /// <returns> Double entré par l'utilisateur </returns>
        public double SecureDoubleInput()
        {
            string input;
            double value = 0.0;

            while (true)
            {
                input = Console.ReadLine();
                try
                {
                    value = double.Parse(input);
                    if (value > 0)
                    {
                        return value;
                    }
                    Console.WriteLine("La valeur entrée n'est pas strictement supérieure à 0");
                }
                catch (Exception)
                {
                    Console.WriteLine("La valeur entrée n'est pas un double");
                }
            }
        }



        /// <summary>
        /// Lister les fichiers dans le répertoire courant
        /// </summary>
        /// <returns> Liste des noms des fichiers dans le répertoire courant </returns>
        public static string[] FileList()
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\";
            // Liste des fichiers bmp dans le répertoire courant
            string[] files = Directory.GetFiles(@path, "*.bmp");
            // On supprime l'extension .bmp du nom
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Replace(@path, "");
            }
            return files;
        }
    }
}