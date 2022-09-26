using System.Diagnostics;
namespace MoogleEngine;

public class Search
{
    static string[] ExecuteQuery(string s)
    //Devuelve un array de las palabras de la query normalizadas sin modificar los caracteres de los operadores
    {
        char[] separator = new char[] {'@', ',', ';', ':', '.', '[', ']', '/', '-' , '\'', '(', ')', '#', ' '};
        s = s.ToLower();
        string[] list = s.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        for(int i =0; i<list.Count(); i++)
        {
            list[i] = Documents.Accent(list[i]);
        }
        return list;
    }

    static int [] VectorQuery(string [] list)
    //Devuelve el vector correspondiente a la query
    {
        int[] vec = new int [Documents.vocabulary.Count()];
        for(int i = 0; i<vec.Length; i++)
        {
            for(int j = 0; j<list.Length; j++)
            {
                if(string.IsNullOrWhiteSpace(Documents.vocabulary[i]))
                {
                    continue;
                }
                if(Documents.vocabulary[i] == list[j])
                {
                    vec[i] += 1;
                }
            }
        }
    return vec;
    }

    static SearchItem[] Ordenar(float []sc, string[] tt, string []snp)  
    //Ordena los resultados de la busqueda por su score
    {
        bool cambios = false;
        SearchItem[] Orden = new SearchItem[Documents.Count];
        float[] sc2 = new float[sc.Length];
        string[] tt2 = new string[tt.Length];
        string[] snp2 = new string[snp.Length];

            sc.CopyTo(sc2,0);
            tt.CopyTo(tt2,0);
            snp.CopyTo(snp2,0);
            

        for(int i = 0; i< sc.Length-1; i++)
        {
            for(int j = i+1; j<sc.Length; j++)
            {

                if(sc[i]<sc[j])
                {
                    float temp = sc2[i];
                    sc2[i] = sc2[j];
                    sc2[j] = temp;

                    string tmptit = tt2[i];
                    tt2[i] = tt2[j];
                    tt2[j] = tmptit;

                    string tmpsnip = snp2[i];
                    snp2[i] = snp2[j];
                    snp2[j] = tmpsnip;
                    cambios = true;
                }
                
            }
        }

        for(int i = 0; i<Documents.Count; i++)
        {
            Orden[i] = new SearchItem(tt2[i],snp2[i],sc[i]);
        }

        if(cambios)
        {
            Orden = Ordenar(sc2, tt2, snp2);
        }
        return Orden;
    }

    static SearchItem [] Redimencionar(SearchItem [] respuesta)
    //Elimina los resultados vacios de la busqueda
    {
        int x = 0;
        for(int i = 0; i<Documents.Count;i++)
        {
            if(respuesta[i].Score > 0.5)
            {
                x++;
                
                if(x == 15)
                {
                    break;
                }
            }
        }
        SearchItem[] nuevaRespuesta = new SearchItem[x];
        for(int i = 0; i<x;i++)
        {
            nuevaRespuesta[i] = respuesta[i];
        }
        return nuevaRespuesta;
    }

    public static SearchItem[] Result(string query)
    //Devuelve el resultado de la busqueda
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        SearchItem[] respuesta = new SearchItem[Documents.Count];

        if(string.IsNullOrWhiteSpace(query))
        {
            Moogle.sugerencia = "";
            return new SearchItem[1] {new SearchItem("No se ha detectado una consulta","Sic Mundus Creatus Est",1f)};
        }
        string[] enter = ExecuteQuery(query);
        (int, string, string)[] VecOp = new (int, string, string)[enter.Length];
        (string, int)[] rel = new (string, int)[enter.Length];

        for(int i = 0; i < enter.Count(); i++)
        {
            if(enter[i].Contains("*"))
            {
                int c = enter[i].Length;
                enter[i] =  enter[i].Replace("*","");
                int b = enter[i].Length;
                int a = c - b;
                rel[i] = (enter[i], a);
            }
            
            if(enter[i].Contains("~"))
            {
                try
                {
                    enter[i] = enter[i].Replace("~","");
                    VecOp[i] = (1,enter[i - 1], enter[i+1]);
                }catch
                {
                    return new SearchItem[1] {new SearchItem ("La sintaxis no es correca", "Por favor intente colocar el operador de la siguiente forma 'Roma ~ Grecia' ",0.3f)};
                }
            }
            if(enter[i].Contains("!"))
            {
                enter[i] = enter[i].Replace("!","");
                VecOp[i] = (2,enter[i],"");
            }
            if(enter[i].Contains("^"))
            {
                enter[i] = enter[i].Replace("^","");
                VecOp[i] = (3,enter[i],"");
            }
            
            if(!Documents.vocabulary.Contains(enter[i]))
            {
                enter[i]  = Moogle.Suggestion(enter[i]);
            }
        }
        
        Moogle.sugerencia = string.Join(" ",enter);

        int [] queryVector = VectorQuery(enter);
        Moogle.queryRel = Moogle.Peso(enter,queryVector,Documents.frecuency);

        Operator.Importance(enter, Moogle.queryRel, rel);
        
        float [] score = new float [Documents.Count];

        for(int i = 0; i<Documents.Count; i++)
        {
            score [i] = Moogle.Relevance(Moogle.queryRel,Documents.pesos[i]);
        }
        
        for(int i = 0; i < enter.Length; i++)
        {
            if(VecOp[i].Item1 > 0)
            {
                Operator.Containment(score,VecOp[i].Item2,VecOp[i].Item1,VecOp[i].Item3);
            }
        }

        Documents.Order(enter);

        string [] snippet = new string [Documents.Count];
        for(int i = 0; i<Documents.Count; i++)
        {
            if(score[i] > 0.5)
            {
                snippet[i] = Documents.SnippetLine(i, enter);
            }
        }

        respuesta = Ordenar(score, Documents.tittle, snippet);
        
        SearchItem[] output = Redimencionar(respuesta);
        
        if(output == null || output.Length == 0)
        {
            return new SearchItem[1] {new SearchItem ("No se encontro respuesta", "Intente con otras palabras clave",0.3f)};
        }

        stopwatch.Stop();
        System.Console.WriteLine("");
        System.Console.WriteLine("Busqueda : {0}",stopwatch.Elapsed);
        System.Console.WriteLine("");
        stopwatch.Reset();
        
        return output;
    }
}