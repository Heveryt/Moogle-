namespace MoogleEngine;
public static class Operator
{
   public static void Containment(float [] score,string s,int n,string t)
    // Aplica operadores de "!" "^" "~"
    // 1 == "~"
    // 2 == "!"
    // 3 == "^"
    {
        if(n == 2)
        {
            for(int i = 0; i<score.Length; i++)
            {
                if(Documents.documents[i].Contains(s))
                {
                    score[i] = 0;
                }
            }
        }

        if(n == 3)
        {
            for(int i = 0; i<score.Length; i++)
            {
                if(!Documents.documents[i].Contains(s))
                {
                    score[i] = 0;
                }
            }
        }

        if(n == 1)
        {
            for(int i = 0; i < Documents.Count; i++)
            {
                int min = int.MaxValue;
                if(Documents.index[i].ContainsKey(s) && Documents.index[i].ContainsKey(t))
                {
                    
                    for(int j = 0; j < Documents.index[i][s].Count; j++)
                    {
                        for(int k = 0; k < Documents.index[i][t].Count; k++)
                        {
                            int temp = Math.Abs(Documents.index[i][s][j] - Documents.index[i][t][k]);
                            if(temp < min)
                            {
                                min = temp;
                                if(min == 1)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                score[i] = score[i] + 1/min;
            }
        }
        
    }

    public static void Importance(string[] enter, float [] queryRel, (string, int)[] rel)
    {
        for(int i = 0; i < enter.Length; i++)
        {
            if(!string.IsNullOrEmpty(rel[i].Item1))
            {
                string temp = rel[i].Item1;   
                int l = Array.IndexOf(Documents.vocabulary,temp);
                queryRel[l] = queryRel[l] * 2*(rel[i].Item2);
            }
        }
    }
}