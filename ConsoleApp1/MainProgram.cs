using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using JiebaNet.Segmenter;
using System.Net.Http.Headers;
using System.Linq;
using System.IO;

class MainProgram
{
    static int segCount;
    static string tokenType;
    static string token;
    static string tokenForTest;
    static string newsAppend;
    static Dictionary<string, decimal> wordTF =new Dictionary<string, decimal>();
    static List<News> news = new List<News>();   
    static List<KeywordData> keydata = new List<KeywordData>();
    static List<string> wordsWithoutSw = new List<string>();
    static string[] words;
    static string[] stopwords;
    static Boolean success;

    static async Task Main(string[] args)
    {
        #region 取得token

        using (HttpClient client = new HttpClient())  //建立 HttpClient
        using (HttpResponseMessage response = await client.GetAsync("https://api2.ifeel.com.tw/pro/auth?member_account=iiidsi_sa&client_secretkey=daa75a16f538c13be9e97cf91acdd9c8"))  //使用 async 方法從網路 url 上取得回應
        using (HttpContent content = response.Content)   //將網路取得回應的內容設定給 httpcontent
        {
            string result = await content.ReadAsStringAsync();  // 將 httpcontent 轉為 string

            if (result != "Unauthorized Access")  //成功取得token
            {
                Console.WriteLine("Get token successfully" + "\n");
                dynamic dy = JsonConvert.DeserializeObject<dynamic>(result);   //將result轉換為dynamic物件，方便資料讀取
                tokenType = dy.token_type;
                token = dy.access_token;
                Console.WriteLine("token type: " + tokenType);
                Console.WriteLine("token: " + token + "\n");
            }
            else   //取得token失敗
                Console.WriteLine("Get token unsuccessfully" + "\n");
        }
        #endregion

        #region 驗證token有效性

        using (HttpClient client = new HttpClient())
        {
            tokenForTest = tokenType + " " + token;  //串聯token type與token碼，以利之後header驗證使用
            client.DefaultRequestHeaders.Add("Authorization", tokenForTest);  // 指定 authorization header
            using (HttpResponseMessage response = client.PostAsync("https://api2.ifeel.com.tw/pro/auth", null).Result)  //使用 async 方法從網路 url 上取得回應
            using (HttpContent content = response.Content)   //將網路取得回應的內容設定給 httpcontent
            {
                string result = await content.ReadAsStringAsync();  // 將 httpcontent 轉為 string

                if (result != "Unauthorized Access")   //驗證成功
                {
                    Console.WriteLine("Token accessible,Information:" + "\n");
                    dynamic dy = JsonConvert.DeserializeObject<dynamic>(result);  //將result轉換為dynamic物件，方便資料讀取
                    Console.WriteLine(result);
                    success = true;
                }
                else  //驗證失敗
                {
                    Console.WriteLine("Token not accessible" + "\n");
                    success = false;
                }
            }
        }
        #endregion

        #region 抓取新聞內容

        //token驗證成功，可抓取新聞資料
        if (success == true)   
        {
            //抓取2018-04-20的資料
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", tokenForTest);  // 指定 authorization header
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); //指定content type header
                var paraBefore = new Dictionary<string, string>()  //定義傳入參數
                {
                        {
                            "date", "2018-04-20"
                        },
                        {
                            "keyword", "麻疹"
                        },
                 };
                var paraAfter = new StringContent(JsonConvert.SerializeObject(paraBefore), Encoding.UTF8, "application/json"); //轉換傳入參數的編碼與資料格式
                using (HttpResponseMessage response = client.PostAsync("https://api2.ifeel.com.tw/pro/document/news/all", paraAfter).Result)  //使用 async 方法從網路 url 上取得回應
                using (HttpContent content = response.Content)   //將網路取得回應的內容設定給 httpcontent
                {
                    string result = await content.ReadAsStringAsync();  // 將 httpcontent 轉為 string
                    dynamic dy = JsonConvert.DeserializeObject<dynamic>(result);  //將result轉換為dynamic物件，方便資料讀取

                    if (dy["_status"] == "Success")  //抓取成功
                    {
                        Console.WriteLine("Get " + paraBefore["date"]+ " news data successfully!" + "\n");

                        //依序將抓到的新聞內容存進News物件，並存進news list裡
                        for (int i = 0; i < dy["data"].Count; i++)
                        {
                            news.Add(new News(dy["data"][i]["content"].ToString()));
                            //Console.WriteLine(dy["data"][i]["content"].ToString() + "\n");
                        }
                    }
                    else  //抓取失敗
                    {
                        Console.WriteLine("Get " + paraBefore["date"] + " news data unsuccessfully!" + "\n");
                    }
                }

                //接著抓取2018-04-21的資料
                paraBefore = new Dictionary<string, string>()   //定義傳入參數
                {
                        {
                            "date", "2018-04-21"
                        },
                        {
                            "keyword", "麻疹"
                        },
                    };
                paraAfter = new StringContent(JsonConvert.SerializeObject(paraBefore), Encoding.UTF8, "application/json");  //轉換傳入參數的編碼與資料格式
                using (HttpResponseMessage response = client.PostAsync("https://api2.ifeel.com.tw/pro/document/news/all", paraAfter).Result)  //使用 async 方法從網路 url 上取得回應
                using (HttpContent content = response.Content)   //將網路取得回應的內容設定給 httpcontent
                {
                    string result = await content.ReadAsStringAsync();  // 將 httpcontent 轉為 string
                    dynamic dy = JsonConvert.DeserializeObject<dynamic>(result);   //將result轉換為dynamic物件，方便資料讀取

                    if (dy["_status"] == "Success")   //抓取成功
                    {
                        Console.WriteLine("Get " + paraBefore["date"] + " news data successfully!" + "\n");

                        //依序將抓到的新聞內容存進News物件，並存進news list裡
                        for (int i = 0; i < dy["data"].Count; i++)
                        {
                            news.Add(new News(dy["data"][i]["content"].ToString()));
                            //Console.WriteLine(dy["data"][i]["content"].ToString() + "\n");
                        }
                    }
                    else   //抓取失敗
                    {
                        Console.WriteLine("Get " + paraBefore["date"] + " news data unsuccessfully!" + "\n");
                    }
                }
            }
            #endregion

            #region 文章斷詞與關鍵詞分析

            //合併個別文章
            for (int i=0; i < news.Count;i++)
            {
                newsAppend += news[i].getNews();               
            }

            var seg = new JiebaSegmenter();   //建立Jieba分詞物件
            var segments = seg.Cut(newsAppend);    //進行斷詞

            segCount = 0;
            //計算分詞個數
            foreach (var segment in segments)
            {
                segCount++;
            }

            words = new string[segCount];
            int c = 0;
            //將分詞集合裡的詞依序存進words array
            foreach (var segment in segments)
            {
                words[c] = segment;
                c++;
            }

            //從本地端讀進stopwords.txt file，並存進stopwords陣列
            stopwords = File.ReadAllLines(System.IO.Directory.GetCurrentDirectory() + "\\Resources\\stopwords.txt"); 

            for(int i=0;i<words.Length;i++)
            {           
                for (int j=0;j<stopwords.Length;j++)
                {
                    //若分詞屬於stopword，則從words list刪除該分詞
                    if(words[i] == stopwords[j])
                    {
                        words[i] = "Deleted!";
                        break;
                    }
                }

                if (words[i] != "Deleted!" && words[i] != " ")
                {
                    wordsWithoutSw.Add(words[i]);
                }
            }

            int count;
            //計算TF值
            for (int i = 0; i < wordsWithoutSw.Count; i++)
            {
                count = 1;
                if(wordTF.ContainsKey(wordsWithoutSw[i]) == false)   //wordTF dictionary裡無紀錄值才計算，以避免重複算的問題
                {
                    for (int j = i + 1; j < wordsWithoutSw.Count; j++)  
                    {                     
                        if (wordsWithoutSw[i].Equals(wordsWithoutSw[j]))
                        {
                            count++;                           
                        }
                    }
                    wordTF.Add(wordsWithoutSw[i], ((decimal)count / wordsWithoutSw.Count));  //將計算後的次數加入到wordTF dictionary
                }              
            }

            //取出前20大關鍵詞
            var top20 = wordTF.OrderByDescending(pair => pair.Value).Take(20)    
                                .ToDictionary(pair => pair.Key, pair => pair.Value);

            #endregion

            #region Json格式轉換與檔案輸出

            Console.WriteLine("Top 20 most used keyword:");

            //依序讀取20個關鍵字，將關鍵字資料存成KeywordData物件，並存進keydata list
            foreach (KeyValuePair<string, decimal> top in top20)
            {
                Console.WriteLine("keyword = {0}, TF = {1}", top.Key, top.Value);
                KeywordData kd = new KeywordData();
                kd.text = top.Key;
                kd.size = (int)(top.Value * 4000);
                keydata.Add(kd);
            }

            var dataForOutput = JsonConvert.SerializeObject(keydata); //將list序列化為JSON字串      
            dataForOutput = "data = '" + dataForOutput + "';";   //轉換成HTML可讀取的資料格式
            //Console.WriteLine(dataForOutput);
            System.IO.File.WriteAllText(@"C:\Users\User\Desktop\資策會測驗\III program test\website\data.json", dataForOutput);   //將json檔輸出至本地端
            #endregion
        }
    }


}
