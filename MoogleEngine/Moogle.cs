namespace MoogleEngine;
using System;
using System.Collections.Generic;

public class DocsFinal: IEquatable<DocsFinal>, IComparable<DocsFinal>
{
    public string? filename;
    public string? snippet;
    public float score;

    public List<String>? frases = new List<string>();

    public List<String>? Frases { get; set;}

    public string? Filename { get; set;}

    public string? Snippet  { get; set;}

    public float Score { get; set;}

    public override string ToString() {return filename + "-" + snippet + "-" + score;}

    public override bool Equals (object ?obj) 
    {
        if(obj == null) return false;
        DocsFinal ?d = obj as DocsFinal;
        if(d == null) return false;
        else return Equals(d);
    }

    public int CompareTo(DocsFinal ?d)
    {
        if(d == null) return 1;
        else return this.score.CompareTo(d.score);
    }

    public bool Equals(DocsFinal ?d)
    {
        if(d == null) return false;
        return this.score.Equals(d.score);
    }
};
public static class Moogle
{
    public static SearchResult Query(string query) 
    {

        string querysnippet = "";
        string sourcepath = @"..\Content";
        DirectoryInfo d = new DirectoryInfo(sourcepath);
        var arrayDirectory = Directory.EnumerateFiles(sourcepath);
        Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
        Dictionary<string, Dictionary<string, int>> dictFrecuencia = new Dictionary<string, Dictionary<string, int>>();
        Dictionary<string, Dictionary<string, double>> dictTF_IDF = new Dictionary<string, Dictionary<string, double>>();
        Dictionary<string, int> dictPrioridad = new Dictionary<string, int>();
        Dictionary<string, string> dictDistancia = new Dictionary<string, string>();
        List<String> files = new List<string> ();
        List<String> termsDoc = new List<string> ();
        List<DocsFinal> docs = new List<DocsFinal> ();
        Dictionary<string , List<string>> dictTempsnippet = new Dictionary<string, List<string>>();

        string [] tmpPrioridad = new string[100];
        string [] termsDistancia = new string[100];

        termsDoc = query.ToLower().Split(new char[] {'(',')',',',';','"','.','?','-', ' '}).Distinct().Where( d => d.Length > 3).ToList<string>();
        List<int> fraseFrecuencia = new List<int>();
        for (int i = 0; i < termsDoc.Count; i++)
        {
            tmpPrioridad = termsDoc[i].Split(new char[] {'*'}); 
            if(tmpPrioridad.Length > 1)
                dictPrioridad.Add(termsDoc[i], tmpPrioridad.Length);
            else
                dictPrioridad.Add(termsDoc[i], 1);

            termsDoc[i] = tmpPrioridad[tmpPrioridad.Length-1].ToLower();
        }
        double [] score = new double [arrayDirectory.Count()];
        string [] snippet = new string [arrayDirectory.Count()];

        for (int i = 0; i < termsDoc.Count; i++)
        {
            fraseFrecuencia.Add(numerosDocs(termsDoc[i].Substring(1,termsDoc[i].Length-1).ToLower()));        
        }

        termsDistancia = query.Split(new char[] {'~'}).ToArray<string>();
        List<string> tmp1 = new List<string>();
        List<string> tmp2 = new List<string>();
        if(termsDistancia.Length > 1){
            for (int i = 0; i < termsDistancia.Length - 1; i++)
            {
                tmp1 = termsDistancia[i].Split(new char[] {'*', '(',')',',',';','"','.','?','-', ' ','\0'}).Distinct().Where( d => d.Length > 3).ToList<string>();
                tmp2 = termsDistancia[i+1].Split(new char[] {'*', '(',')',',',';','"','.','?','-', ' ','\0'}).Distinct().Where( d => d.Length > 3).ToList<string>();
                dictDistancia.Add(tmp1[tmp1.Count-1].ToLower(), tmp2[0].ToLower());
            }
        }

        List<string> temp = new List<string>();
        for(int j = 0; j < arrayDirectory.Count(); j++)
        {
            string filecontent = File.ReadAllText(arrayDirectory.ElementAt(j));        
            Dictionary<string, int> dictTemp = new Dictionary<string, int>();
            Dictionary<string, double> dictTempD = new Dictionary<string, double>();
            List<String> tFrases = new List<string> ();  

                string [] words = filecontent.Split(new char[] {' ','(',')',',',';','"','.','!','?','-'});

                for (int i = 0; i < termsDoc.Count; i++)
                {  
                    if(fraseFrecuencia[i] > 0)  //frecuencia de la palabra sobre todos los textos
                    {         
                        if(termsDoc[i][0] == '^' || termsDoc[i][0] == '!')
                            temp = filecontent.ToLower().Split(termsDoc[i].Substring(1,termsDoc[i].Length-1).ToLower()).ToList<string>();
                        else
                            temp = filecontent.ToLower().Split(termsDoc[i].ToLower()).ToList<string>();    
                        dictTemp.Add(termsDoc[i], temp.Count - 1);

                        if (temp.Count == 1 && termsDoc[i][0] == '^')
                        {
                            dictTempD.Add(termsDoc[i], -1);
                        }
                        else if (temp.Count > 1)
                        {
                            if(termsDoc[i][0] == '!')
                            {
                                dictTempD.Add(termsDoc[i], -1);
                            }
                            else
                            {
                                List<string> phrases = filecontent.ToLower().Split(new char[] {'(',')',',',';','"','.','!','?','-', ' ','*','\0'}).Distinct().Where( d => d.Length > 3).ToList<string>();
                                int distanciaTotal = 0;
                                string? val = " ";
                                if(dictDistancia.TryGetValue(termsDoc[i], out val))
                                {
                                    int dist1 = phrases.IndexOf(val) - phrases.IndexOf(termsDoc[i]);
                                    int dist2 = phrases.LastIndexOf(val) - phrases.LastIndexOf(termsDoc[i]);
                                    int dist3 = phrases.LastIndexOf(val) - phrases.IndexOf(termsDoc[i]);

                                    if(dist2 > 0 && dist3 > 0)
                                    {
                                        if(dist1 <= dist2 && dist1 <= dist3)
                                            distanciaTotal = dist1;
                                        else if(dist2 <= dist1 && dist2 <= dist3)
                                            distanciaTotal = dist2;
                                        else if(dist3 <= dist1 && dist3 <= dist2)
                                            distanciaTotal = dist3;
                                    }
                                    else if (dist2 > 0 && dist3 < 0)
                                    {
                                        if(dist1 <= dist2)
                                            distanciaTotal = dist1;
                                        else
                                            distanciaTotal = dist2;
                                    }
                                    else if (dist2 < 0 && dist3 > 0)
                                    {
                                        if(dist1 <= dist3)
                                            distanciaTotal = dist1;
                                        else
                                            distanciaTotal = dist3;
                                    }        
                                    else
                                        distanciaTotal = dist1;                                                                            
                                }
                                                        
                                if(distanciaTotal > 0)
                                {
                                    dictTempD.Add(termsDoc[i], (1 - ((distanciaTotal*100)/(double)phrases.Count)/100) + (dictPrioridad.ElementAt(i).Value * (((temp.Count - 1)/(double)words.Length)*(Math.Log10(arrayDirectory.Count()/(double)fraseFrecuencia[i])))));    
                                }
                                else
                                {                                    
                                    dictTempD.Add(termsDoc[i], (dictPrioridad.ElementAt(i).Value * (((temp.Count - 1)/(double)words.Length)*(Math.Log10(arrayDirectory.Count()/(double)fraseFrecuencia[i])))));
                                }

                                List<string> temp1 = temp[0].Split(new char[] {'.'}).ToList();
                                List<string> temp2 = temp[1].Split(new char[] {'.'}).ToList();
                                tFrases.Add(temp1[temp1.Count()-1]+termsDoc[i]+temp2[0]);                                
                            }
                        }
                        else
                        {
                            if (termsDoc[i][0] != '^')
                            {                            
                                string [] phrases = filecontent.ToLower().Split(new char[] {'(',')',',',';','"','.','!','?','-', ' '}).Distinct().Where( d => d.Length > 3 && Calculatequery(termsDoc[i].ToLower(), d)).ToArray<string>();                
                                string [] totalPhrases = filecontent.ToLower().Split(new char[] {'(',')',',',';','"','.','!','?','-', ' '}).Distinct().Where( d => d.Length > 3).ToArray<string>();                
                                dictTempD.Add(termsDoc[i], phrases.Length/(double)totalPhrases.Length);       

                                if(phrases.Count() > 0)
                                {
                                    temp = filecontent.ToLower().Split(phrases[0]).ToList<string>();  
                                    List<string> temp1 = temp[0].Split(new char[] {'.'}).ToList();
                                    List<string> temp2 = temp[1].Split(new char[] {'.'}).ToList();                             
                                    tFrases.Add(temp1[temp1.Count()-1]+phrases[0]+" "+temp2[0]);                
                                }                                      
                            }
                            else
                            {
                                dictTempD.Add(termsDoc[i], -1);
                            }
                        }                        
                    }
                    else  
                    {
                        if (termsDoc[i][0] != '^')
                        {
                            string [] phrases = filecontent.ToLower().Split(new char[] {'(',')',',',';','"','.','!','?','-', ' '}).Distinct().Where( d => d.Length > 3 && Calculatequery(termsDoc[i].ToLower(), d)).ToArray<string>();                
                            string [] totalPhrases = filecontent.ToLower().Split(new char[] {'(',')',',',';','"','.','!','?','-', ' '}).Distinct().Where( d => d.Length > 3).ToArray<string>();                
                            dictTempD.Add(termsDoc[i], phrases.Length/(double)totalPhrases.Length); 
                            if(phrases.Count() > 0)
                            {
                                temp = filecontent.ToLower().Split(phrases[0]).ToList<string>();  
                                List<string> temp1 = temp[0].Split(new char[] {'.'}).ToList();
                                List<string> temp2 = temp[1].Split(new char[] {'.'}).ToList();                            
                                tFrases.Add(temp1[temp1.Count()-1]+phrases[0]+" "+temp2[0]);                 
                            }         
                        }
                        else
                        {
                            dictTempD.Add(termsDoc[i], -1);
                        }                
                    }
                }
                dictTempsnippet.Add(Path.GetFileName(arrayDirectory.ElementAt(j)), tFrases);
                dictFrecuencia.Add(Path.GetFileName(arrayDirectory.ElementAt(j)), dictTemp);  
                dictTF_IDF.Add(Path.GetFileName(arrayDirectory.ElementAt(j)), dictTempD);

                if (filecontent.ToLower().Contains(query.ToLower()))
                {  
                    score[j] = dictTempD.Values.Max()*dictTempD.Count();          
                }

        }

        int numerosDocs(string word)
        {
            int count = 0;
            foreach(var file in arrayDirectory)
            {
                string filecontent = File.ReadAllText(file);
                if (filecontent.ToLower().Contains(word))
                {      
                    count++;
                }
            }

            return count;
        }

        int DamerauLevenshteinDistance(string string1, string string2, int threshold)
        {
            if (string1.Equals(string2))
                return 0;

            if (String.IsNullOrEmpty(string1) || String.IsNullOrEmpty(string2))
                return (string1 ?? "").Length + (string2 ?? "").Length;


            if (string1.Length > string2.Length)
            {
                var tmp = string1;
                string1 = string2;
                string2 = tmp;
            }

            if (string2.Contains(string1))
                return string2.Length - string1.Length;

            var length1 = string1.Length;
            var length2 = string2.Length;

            var d = new int[length1 + 1, length2 + 1];

            for (var i = 0; i <= d.GetUpperBound(0); i++)
                d[i, 0] = i;

            for (var i = 0; i <= d.GetUpperBound(1); i++)
                d[0, i] = i;

            for (var i = 1; i <= d.GetUpperBound(0); i++)
            {
                var im1 = i - 1;
                var im2 = i - 2;
                var minDistance = threshold;

                for (var j = 1; j <= d.GetUpperBound(1); j++)
                {
                    var jm1 = j - 1;
                    var jm2 = j - 2;
                    var cost = string1[im1] == string2[jm1] ? 0 : 1;

                    var del = d[im1, j] + 1;
                    var ins = d[i, jm1] + 1;
                    var sub = d[im1, jm1] + cost;

                    d[i, j] = del <= ins && del <= sub ? del : ins <= sub ? ins : sub;

                    if (i > 1 && j > 1 && string1[im1] == string2[jm2] && string1[im2] == string2[jm1])
                        d[i, j] = Math.Min(d[i, j], d[im2, jm2] + cost);

                    if (d[i, j] < minDistance)
                        minDistance = d[i, j];
                }

                if (minDistance > threshold)
                    return int.MaxValue;
            }

            return d[d.GetUpperBound(0), d.GetUpperBound(1)] > threshold 
                ? int.MaxValue 
                : d[d.GetUpperBound(0), d.GetUpperBound(1)];
        }

        bool Calculatequery(string query, string frase)
        {
            double difference = Math.Abs(frase.Length - query.Length);
            double val = DamerauLevenshteinDistance(query , frase , 3) ;
            if(val <= difference)
            {
                return true;
            }
            return false;
        }
        
        double suma = 0;
        bool restriction = false;
        for(int i = 0; i < dictTF_IDF.Values.Count(); i++)
        {
            foreach(var valueD in dictTF_IDF.Values.ElementAt(i))
            {
                if(valueD.Value == -1)
                {
                    restriction = true;
                    break;
                }
                suma+=valueD.Value;
            }  
            if(restriction == false)
            {            
                score[i]+=(suma/(double)dictTF_IDF.Values.ElementAt(i).Count());
                if(score[i] > 0)
                {
                    DocsFinal dd = new DocsFinal();
                    dd.filename = dictTF_IDF.ElementAt(i).Key;

                    foreach(var frase in dictTempsnippet)
                    {
                        if(frase.Key == dd.filename)
                        {
                            dd.Frases = frase.Value;
                        }
                    }                                         
                    dd.score = (float)score[i];
                    docs.Add(dd);
                }
            }
            restriction = false;            
            suma = 0;
        }

        docs.Sort();
        docs.Reverse();

        SearchItem[] items = new SearchItem[docs.Count()];

        for(int i = docs.Count-1; i >= 0; i--) 
        {
            if(docs[i].Frases.Count() > 0)
                items[i] = new SearchItem(docs[i].filename ,docs[i].Frases[0], docs[i].score);
            else 
                items[i] = new SearchItem(docs[i].filename ,"", docs[i].score);

        }

        return new SearchResult(items , query);
    }
}
