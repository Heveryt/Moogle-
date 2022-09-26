namespace MoogleEngine;


public static class Moogle
{
    public static SearchResult Query(string query) {
        // Modifique este método para responder a la búsqueda

        SearchItem[] items = Search.Result(query);

        return new SearchResult(items, sugerencia);
    }
    public static string sugerencia = "";
    public static float [] queryRel = null;
    
    public static string Suggestion(string s)
    {
        int min = s.Length;
        string respuesta = "";

        for(int i = 0; i < Documents.vocabulary.Count(); i++)
        {
            string t = Documents.vocabulary[i];
            if(Comparer(s,t)< min)
            {
                min = Comparer(s,t);
                respuesta = t;
                if(min == 1)
                {
                    break;
                }
            }   
        }
        
        if(!string.IsNullOrEmpty(respuesta))
        {

            return respuesta;
        }

        return s;
    }
    static int Comparer(string s, string t)
    {
        //Devuelve un entero con la distancia entre dos palabras
        int n = s.Length;
        int m = t.Length;
        int [,] d = new int [n+1,m+1];
    
        if(n==0)
        {
            return m;
        }
    
        if(m==0)
        {
            return n;
        }
        for(int i = 0; i<=n; d[i, 0] = i++)
        {}

        for(int j = 0; j<=m; d[0, j] = j++)
        {}

        for(int i = 1; i <=n; i++)
        {
            for(int j = 1; j <=m; j++)
            {
                int transf = (s[i-1]==t[j-1]) ? 0 : 1;

                d[i,j] = Math.Min(Math.Min(d[i-1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + transf);

            }
        }
        return d[n, m];
    }    
    public static float Ponderar(int nFrec, int max, int nDocs)
        //Calcula la relevancia de una palabra para la consulta
        // nFrec == Cantidad de veces que se repite en el documento
        // nDocs == Documentos en los que aparece
        // max == Total de palabras del documento
    {
        float a = 0.5f;
        int docs = Documents.Count;
        float tf;
        if(nFrec == 0)
        {
            tf = 0;
        }else {
            tf = nFrec/((float)max * (float)nFrec);
        }
        float idf;
        if(nDocs == 0)
        {
            idf = 0;
        }else {
            idf = (float)Math.Log((double)docs/(double)nDocs);
        }
        float peso = (a + (1-a)*tf)*idf;
        return peso;
    }

    public static float Relevance(float [] consulta, float [] doc)
    {
        //Realiza la suma de productos escalares de los vectores
        float score = 0f;
        for(int i = 0 ; i < consulta.Length; i++)
        {
            if(consulta[i] == 0 || doc[i] == 0)
            {
                continue;
            }else
            {
                score = score + (consulta[i] * doc[i])/(float)Math.Sqrt(Math.Abs(consulta[i]*doc[i]));
            }
        }

        return score;
    }

    public static float [] Peso(string[] list, int[] v, int[] rep)
    //Asigna los valores de relevancia con uso de Ponderar
    {
        float [] peso = new float [Documents.vocabulary.Length];
        for(int i = 0; i<list.Count(); i++)
        {
            try
            {
                int j = Array.IndexOf(Documents.vocabulary,list[i]);
                peso[j] = Ponderar(v[j],list.Count(), Documents.frecuency[j]);
            }
            catch
            {
                continue;
            }
        }
        return peso;
    }
}