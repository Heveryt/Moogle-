using System.Diagnostics;

namespace MoogleEngine;

public class Documents
    {
    public static string [] directories = new string [Counter()];
    public static int Count = directories.Length;
    public static Dictionary<string,List<int>> [] index = new Dictionary<string,List<int>>[Count];
    public static string [] vocabulary = new string [1];
    public static string [] tittle = new string [Count];
    public static string [][] documents = new string[Count][];
    public static string [][] docLines = new string[Count][];
    public static int [][] vectors = new int[Count][];
    public static float [][] pesos = new float [Count][];
    public static int [] frecuency = new int [1];

    static string Directories()
    //Devuelve la ruta de la carpeta Content 
    {        
        string dir =  @"../Content/";
        return dir;
    }

    public static void ProcessDirectories()
    //Develve un array de strings con las rutas de los documentos
    {
        DirectoryInfo di = new DirectoryInfo(Directories());
        FileInfo[] files = di.GetFiles("*.txt", SearchOption.AllDirectories);
        string [] path = new string[files.Length];
        for(int i =0 ; i< files.Length; i++)
            {
                path[i] = files[i].ToString();
            }
        directories = path;
    }
    
    static int Counter()
    //Devuelve la cantidad de archivos en la carpeta Content
    {
        ProcessDirectories();
        return directories.Length;
    }

    public static string Reader(string s, int pos)
    //Lee un archivo en la ruta recivida
    {
        string [] temp = File.ReadAllLines(s);
        docLines[pos] = temp;
        string lines = string.Join(" ", temp);
        return lines;
    }

        static string[] Execute(string s)
        //Devuelve un documento como un array de strings normalizado
        {
            char[] separator = new char[] {'@', ',', ';', ':', '.', '/', '-' , '\'', '[', ']', '(', ')', '#', '*', '!',' '};
            s = s.ToLower();
            string[] list = s.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            for(int i =0; i<list.Count(); i++)
            {
                list[i] = Accent(list[i]);
            }
            return list;
        }
        
    public static string Accent(string s)
    //Cambia las tildes de una palabra por sus respectivas letras
    {
        s = s.Replace('á','a');
        s = s.Replace('é','e');
        s = s.Replace('í','i');
        s = s.Replace('ó','o');
        s = s.Replace('ú','u');

        return s;
    }

    public static void Order(string [] enter)
    {
        float[] enterF = new float[enter.Length];
        for(int i = 0; i < enter.Length; i++)
        {
            for(int j = 0; j < vocabulary.Length; j++)
            {
                if(enter[i] == vocabulary[j])
                {
                    enterF[i] = Moogle.queryRel[j];
                    break;
                }
            }
        }

        Ordenation(enter, enterF);
    }

    public static void Ordenation(string [] enter, float[] enterF)
    //Ordena los elementos de enter en relacion con enterF
    {
        bool changes = false;
        for(int i = 0; i < enter.Length - 1; i++)
        {
            for(int j = i +1 ; j < enter.Length; j++)
            {
                if(enterF[i] < enterF[j])
                {
                    float temp = enterF[j];
                    string s = enter[j];
                    enterF[j] = enterF[i];
                    enter[j] = enter[i];
                    enterF[i] = temp;
                    enter[i] = s;
                    changes = true;
                }
            }
        }

        if(changes)
        {
            Ordenation(enter, enterF);
        }
    }

        static void Tittle()
        //Inicializa la variable tittle
        {
            string [] st = new string [Count];
            for(int i = 0 ; i< Count; i++)
            {
                st[i] = Path.GetFileNameWithoutExtension(directories[i]);
            }
            tittle = st;
        }
        
    public static string SnippetLine(int i, string [] enter)
    //Divide el documentos por lineas y donde la intersepcion con la consulta no sea nula calcula su score.
    //Luego devuelve la linea con mas score.
    {
        string line = "";
        string question = "";
        int x = 0;
        foreach(string s in enter)
        {
            if(index[i].ContainsKey(s))
            {
                question = s;
                x++;
                break;
            }
            if(x != 0)
            {
                break;
            }
        }
        for(int j = 0; j<docLines[i].Length; j++)
        {
            string [] list = Execute(docLines[i][j]);
            if(list.Contains(question))
            {
                return docLines[i][j];
            }
        }
        
        return line;
    }
        public static void Make()
        //Inicializa los valores de las variables de la clase Documents
        {
            Counter();

            Stopwatch stopwatch = new Stopwatch();

            System.Console.WriteLine("Creando vocabulario");
            stopwatch.Start();

            Dictionary <string, int> vocabularioDic = new Dictionary<string, int>();
            Dictionary <string, int> [] vecTemp = new Dictionary<string, int>[Count];

            for(int i = 0; i< Count; i++)
            {
                documents[i] = Execute(Reader(directories[i],i));
                vecTemp[i] = new Dictionary<string, int>(); 
                index[i] = new Dictionary<string, List<int>>();    
                
                Dictionary <string, int> temp = new Dictionary<string, int>();

                for(int j = 0; j < documents[i].Length; j++)
                {
                    string st = documents[i][j];
                    if(!vocabularioDic.ContainsKey(st))
                    {
                        vocabularioDic.Add(st,0);
                    }

                    if(vocabularioDic.ContainsKey(st))
                    {
                        if(!vecTemp[i].ContainsKey(st))
                        {
                            vecTemp[i].Add(st,0);
                            index[i].Add(st,new List<int>());
                        }

                        if(!temp.ContainsKey(st))
                        {
                            vocabularioDic[st] ++;
                            temp.Add(st,0);
                        }
                    }
                    
                    if(vecTemp[i].ContainsKey(st))
                    {
                        vecTemp[i][st] ++;
                        index[i][st].Add(j);
                    }
                }
            }

            vocabulary = new string [vocabularioDic.Count];
            vocabularioDic.Keys.CopyTo(vocabulary, 0);
            frecuency = new int [vocabulary.Length];
            vocabularioDic.Values.CopyTo(frecuency,0);

            stopwatch.Stop();
            System.Console.WriteLine(stopwatch.Elapsed);
            stopwatch.Reset();

            System.Console.WriteLine("Creando vectores");
            stopwatch.Start();
            for(int i = 0; i < directories.Length; i++)
            {
                pesos[i] = new float[vocabulary.Length];
                vectors[i] = new int [vocabulary.Length];
                for(int j = 0; j < vocabulary.Length; j++)
                {
                    string temp = vocabulary[j];
                    if(vecTemp[i].ContainsKey(temp))
                    {
                        vectors[i][j] = vecTemp[i][temp];
                        pesos[i][j] = Moogle.Ponderar(vectors[i][j],documents[i].Length,frecuency[j]);
                    }
                }
                vecTemp[i].Values.CopyTo(vectors[i],0);
            }

            stopwatch.Stop();
            System.Console.WriteLine(stopwatch.Elapsed);
            stopwatch.Reset();

            System.Console.WriteLine("Guardando titulos");
            Tittle();
        }
    }