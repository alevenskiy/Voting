using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Voting
{
    public class Pair<TKey, TValue>
    {
        private TKey _key;
        private TValue _value;

        public TKey Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
            }
        }
        public TValue Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public Pair() { }

        public Pair(TKey Key, TValue Value)
        {
            this.Key = Key;
            this.Value = Value;
        }
    }

    internal class Program
    {
        static string ReadInput(string filename)
        {
            string data = "";

            try
            {
                using (Stream stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    StreamReader streamReader = new StreamReader(stream);
                    data = streamReader.ReadToEnd();
                }
            }
            catch
            {
                Console.WriteLine("File does not open");
            }

            return data;
        }

        static string Analysis(string data)
        {
            StringBuilder result = new StringBuilder();

            string[] lines = data.Split('\n');
            int linesIter = 0;

            //прочитать количество тестовых блоков и определиться с циклом хождения по ним
            int blocksNum = Convert.ToInt32(lines[linesIter++]);

            int candNum;
            List<Pair<string, int>> cands;

            for (int bloksIter = 0; bloksIter < blocksNum; bloksIter++)
            {
                linesIter++;

                //прочитать список кандидатов
                candNum = Convert.ToInt32(lines[linesIter++]);
                cands = new List<Pair<string, int>>();

                for (int i = 0; i < candNum; i++)
                {
                    cands.Add(new Pair<string, int>(lines[linesIter++], 0));
                }

                //прочитать бюллетени
                string[] ballotStr;
                List<List<int>> ballot = new List<List<int>>();
                int ballotInd = 0;
                while (linesIter < lines.Length && lines[linesIter] != string.Empty && lines[linesIter] != "\r")
                {
                    ballot.Add(new List<int>());
                    ballotStr = lines[linesIter++].Split(' ');
                    for (int i = 0; i < candNum; i++)
                    {
                        ballot[ballotInd].Add(Convert.ToInt32(ballotStr[i]));
                    }
                    ballotInd++;
                }

                //определиться с циклом поиска победителя
                List<int> losersIndex = new List<int>();
                List<int> winnersIndex = new List<int>();
                while (winnersIndex.Count == 0)
                {
                    int allScore = 0;
                    foreach (Pair<string, int> cand in cands)
                    {
                        cand.Value = 0;
                    }

                    //определиться с циклом рассмотрения бюллетеней
                    foreach (List<int> currBallot in ballot)
                    {
                        int score = candNum;
                        for (int i = 0; i < candNum; i++)
                        {
                            //расставить кандидатам их баллы
                            allScore += score;
                            cands[currBallot[i] - 1].Value += score--;
                        }
                    }


                    //определить победителей и проигравших
                    int loserScore = allScore;
                    int winnerScore = 0;
                    for (int i = 0; i < candNum; i++)
                    {
                        if (winnerScore == cands[i].Value)
                        {
                            winnersIndex.Add(i);
                        }
                        if (winnerScore < cands[i].Value)
                        {
                            winnerScore = cands[i].Value;
                            winnersIndex.Clear();
                            winnersIndex.Add(i);
                        }

                        if (loserScore == cands[i].Value)
                        {
                            losersIndex.Add(i);
                        }
                        if (loserScore > cands[i].Value)
                        {
                            loserScore = cands[i].Value;
                            losersIndex.Clear();
                            losersIndex.Add(i);
                        }
                    }

                    //отсеять проигравших и их баллы 
                    if (winnerScore != loserScore)
                    {
                        for (int i = 0; i < losersIndex.Count; i++)
                        {
                            cands.RemoveAt(losersIndex[i]);
                            foreach (List<int> currBallot in ballot)
                            {
                                currBallot.Remove(losersIndex[i] + 1);
                                for (int scoreInd = 0; scoreInd < currBallot.Count; scoreInd++)
                                {
                                    if (currBallot[scoreInd] > losersIndex[i] + 1)
                                    {
                                        currBallot[scoreInd]--;
                                    }
                                }
                            }
                            candNum--;
                        }
                        losersIndex.Clear();
                        winnersIndex.Clear();
                    }
                }

                //сохранить информацию о победителе блока
                foreach (Pair<string, int> cand in cands)
                {
                    result.AppendLine(cand.Key);
                }
                result.AppendLine();
            }
            return result.ToString();
        }

        static void Main(string[] args)
        {
            //прочитать данные
            string data = ReadInput(args[0]);

            string result = Analysis(data);

            Console.WriteLine(result);
        }
    }
}
