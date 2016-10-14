using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LinqToTwitter;
using System.Diagnostics;

namespace TUBES_STIMA_3
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        Program tw = new Program();
        public class Program
        {
            public class keyword//This is nested class for grouping keyword by category, e.g : Dinas kebersihan, etc.
            {
                public int id;
                public string str;
                public keyword()
                {
                    id = 0;
                    str = "";
                }
            }

            private const int UNDEF = 5;//for now, Forget about it 
            private List<keyword> key = new List<keyword>();//List of keyword for some category of organization e.g : Dinas kebersihan : Sampah; 
            private List<List<Status>> cat = new List<List<Status>>(UNDEF);//This list used for container of tweet
            private List<Status> tweets;//Container of tweet which is retrieved from twitter.com

            public List<keyword> getkey()
            {
                return key;
            }

            public List<List<Status>> getcat()
            {
                return cat;
            }

            public List<Status> gettweets()
            {
                return tweets;
            }
            //Authorize user to access API
            private SingleUserAuthorizer authorizer =
                 new SingleUserAuthorizer
                 {
                     CredentialStore = new
                     SingleUserInMemoryCredentialStore
                     {
                     //You can get this code from your twiiter account
                     ConsumerKey =
                           "wlb7bXrkknmUhqev3FFGyG3Ce",
                         ConsumerSecret =
                          "54KzL3Tt0OBFvD9Ah2tiRCHMT3xClg1MSE0iPNd3ccvw1R5Rw1",
                         AccessToken =
                            "167988305-idP2UDH5ral2b0LVKrsaunE886cZivEAR8pKLdTz",
                         AccessTokenSecret =
                        "TRjpdubS7JmY1sXv9VMOmxQr1fNFk8HnafB1g1d2YKeau"
                     }
                 };

            //Search tweet
            public void SearchTweets(string searchpat)
            {
                var twitterContext = new TwitterContext(authorizer);
                var srch =
                    Enumerable.SingleOrDefault(from search in twitterContext.Search
                                               where search.Type == SearchType.Search &&
                                               search.Query == searchpat && search.Count == 200
                                               select search
                                                );
                if (srch != null && srch.Statuses != null)
                {
                    tweets = srch.Statuses.ToList();
                }
            }

            //Add keyword to List and group it
            public void setKeyword(List<string> keywor)
            {
                for (int i = 0; i < keywor.Count; i++)
                {
                    int startidx = 0;
                    for (int j = 0; j < keywor[i].Length; j++)
                    {
                        if (keywor[i][j] == ';')
                        {
                            keyword keytemp = new keyword();
                            keytemp.str = keywor[i].Substring(startidx, j - startidx);
                            keytemp.id = i;
                            key.Add(keytemp);
                            startidx = j + 1;
                        }
                    }
                }
            }

            //Knuth Morris Prat preprocess Algorithm
            private List<int> fail_func(string pat)
            {
                List<int> arrOfBorder = new List<int>(pat.Length);
                int i, j, k;
                //initialize arrOfBorder with 0
                for (i = 0; i < pat.Length; i++)
                {
                    arrOfBorder.Add(0);
                }
                i = 1;//i = 1 because the arrOfBorder[0] always 0
                while (i < pat.Length)
                {
                    j = i;
                    k = 0;
                    while (j > 0 && pat[k] == pat[j])
                    {//Checking if the pattern has same prefix and suffix
                        arrOfBorder[i] += 1;
                        k++;
                        j--;
                    }
                    i++;
                }
                return arrOfBorder;
            }

            //This is the main Algorith of Knuth Morris Prat
            public void kmp()
            {
                List<List<int>> ListOfBorder = new List<List<int>>(key.Count);
                //initialization list of list
                for (int ct = 0; ct < UNDEF; ct++)
                {
                    cat.Add(new List<Status>());
                }

                //set the list of list 
                for (int br = 0; br < key.Count; br++)
                {
                    ListOfBorder.Add(fail_func(key[br].str));
                }
                int i = 0, k, j, l, m;
                bool found;
                Status st = new Status();
                while (i < tweets.Count)
                {
                    found = false;
                    j = k = l = m = 0;
                    st = tweets[i];//assign st with information on list of tweets, such as userID, user.ScreenName, Tweets, etc.
                    while (j < st.Text.Length && !found)
                    {
                        if (key[m].str[k] != st.Text[j])
                        {
                            if (k == 0)
                            {//if mismatch from start, it will cause the text shift by one character to right, like bruteforce
                                j++;
                                k = 0;
                            }
                            else if (k > 0)
                            {
                                j += k - ListOfBorder[m][k - 1] - 1;
                                k -= k - ListOfBorder[m][k - 1];
                            }
                        }
                        else
                        {
                            k++;
                            j++;
                        }
                        //if k = length of keyword then insert tweet data to list of category, and quit from loop
                        if (k == key[m].str.Length)
                        {
                            if (key[m].id == 0)
                            {
                                cat[0].Add(st);
                            }
                            else if (key[m].id == 1)
                            {
                                cat[1].Add(st);
                            }
                            found = true;
                        }

                        //if k = length of keyword, j = length of tweet and m < Length of list of key -1 than insert it to unknown category
                        if (!found && j == st.Text.Length && m < key.Count - 1)
                        {
                            m++;
                            j = 0;
                        }

                        //if k = length of keyword and j = length of tweet m < Length of list of key -1 than insert it to unknown category
                        else if (!found && j == st.Text.Length && m == key.Count - 1)
                        {
                            cat[UNDEF - 1].Add(st);
                        }
                    }
                    i++;
                }
            }

            void printTweet()
            {
                for (int i = 0; i < UNDEF; i++)
                {
                    if (i == 0)
                    {
                      
                        Console.WriteLine("Dinas Kebersihan");
                        Console.WriteLine("Jumlah : {0}", cat[i].Count);
                    }
                    else if (i == 1)
                    {
                        Console.WriteLine("Dinas Kesehatan");
                        Console.WriteLine("Jumlah : {0}", cat[i].Count);
                    }
                    else if (i == 4)
                    {
                        Console.WriteLine("Unknown");
                        Console.WriteLine("Jumlah : {0}", cat[i].Count);
                    }
                    for (int j = 0; j < cat[i].Count; j++)
                    {
                        if (cat[i][j] != null)
                        {
                            Console.WriteLine("User : {0}, Tweet : {1}", cat[i][j].User.ScreenNameResponse, cat[i][j].Text);
                        }
                        else
                            break;
                    }
                    Console.WriteLine();
                }
            }

            //testing list of keyword, forget it
            void printKeyword()
            {
                for (int i = 0; i < key.Count; i++)
                {
                    Console.Write("ID : {0}, TEXT : {1}", key[i].id, key[i].str);
                    Console.WriteLine();
                }
            }
            
            /*
//Main program
//follow instruction below
//1. Fill "Keyword twitter : " with keyword which we want the search on twitter.com e.g : #pemkot
//2. Fill "Dinas kebersihan : " with a word or many, separated with ';', ending with ';' too. e.g Sampah;TPS;. No white space between ';' and keyword 
static void Main(string[] args)
{
   string input, keyword;
   List<string> lstring = new List<string>();
   Console.Write("Keyword Twitter : ");
   input = Console.ReadLine();
   Console.Write("Dinas kebersihan : ");
   keyword = Console.ReadLine();
   lstring.Add(keyword);
   Console.Write("Dinas kesehatan : ");
   keyword = Console.ReadLine();
   lstring.Add(keyword);
   Program tw = new Program();
   tw.setKeyword(33..lstring);
   tw.SearchTweets(input);
   tw.kmp();
   tw.printTweet();
}
*/
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptResourceDefinition myScriptResDef = new ScriptResourceDefinition();
            myScriptResDef.Path = "~/Scripts/jquery-1.10.2.js";
            myScriptResDef.DebugPath = "~/Scripts/jquery-1.10.2.js";
            myScriptResDef.CdnPath = "http://ajax.microsoft.com/ajax/jQuery/jquery-1.4.2.min.js";
            myScriptResDef.CdnDebugPath = "http://ajax.microsoft.com/ajax/jQuery/jquery-1.4.2.js";
            ScriptManager.ScriptResourceMapping.AddDefinition("jquery", null, myScriptResDef);
        }

        void printTweet()
        {
            Literal1.Mode = LiteralMode.PassThrough;

            for (int i = 0; i < 5; i++)
            {
                if (i == 0)
                {
                    Literal1.Text += "<h3>Dinas Kebersihan</h3>";
                    Literal1.Text += "<p>Jumlah : " + Convert.ToString(tw.getcat()[i].Count) + "</p>";
                  //  Console.WriteLine("Dinas Kebersihan");
                   // Console.WriteLine("Jumlah : {0}", cat[i].Count);
                }
                else if (i == 1)
                {
                    Literal1.Text += "<h3>Dinas Kesehatan</h3> ";
                    Literal1.Text += "<p>Jumlah : " + Convert.ToString(tw.getcat()[i].Count) + "</p>";

                    //Console.WriteLine("Dinas Kesehatan");
                    //Console.WriteLine("Jumlah : {0}", cat[i].Count);
                }
                else if (i == 4)
                {
                    Literal1.Text += "<h3>Unknown</h3> ";
                    Literal1.Text += "<p>Jumlah : " + Convert.ToString(tw.getcat()[i].Count) + "</p>";

                    //Console.WriteLine("Unknown");
                    //Console.WriteLine("Jumlah : {0}", cat[i].Count);
                }
                for (int j = 0; j < tw.getcat()[i].Count; j++)
                {
                    if (tw.getcat()[i][j] != null)
                    {
                        if (j % 2 == 0)
                        {

                            Literal1.Text += "<div class='row bg-darkest-gray'><font color='white'> <br />";
                        }                        
                        Literal1.Text += "<div class = 'col-sm-2' ><img class = 'img-rounded img-centered ' src=" + Convert.ToString(tw.getcat()[i][j].User.ProfileImageUrlHttps) + " > </img></div>";
                        Literal1.Text += "<div class = 'col-sm-4' ><p>User : <a href='http://twitter.com/" + Convert.ToString(tw.getcat()[i][j].User.ScreenNameResponse)+ "' target='_blank'>@"+ Convert.ToString(tw.getcat()[i][j].User.ScreenNameResponse) + "</a>" + "</p><p> Tweet : " + Convert.ToString(tw.getcat()[i][j].Text) + "</p> </div>";
                        if (j%2 == 1 || j == tw.getcat()[i].Count - 1)
                        {
                        Literal1.Text += "</font></div> <br />"; 
                        }
                        //  Console.WriteLine("User : {0}, Tweet : {1}", cat[i][j].User.ScreenNameResponse, cat[i][j].Text);
                    }
                    else
                        break;
                }
               // Literal1.Text += "test dulu< br />";
                //Console.WriteLine();
            }
        }

        protected void Run_Click(object sender, EventArgs e)
        {
            if (IsValid && TextBox1.Text != "" && TextBox2.Text != "" && TextBox3.Text != "")
            {
                /*
                string url = HttpContext.Current.Request.Url.AbsoluteUri + "#Tweet";
                System.Diagnostics.Process.Start(url);*/
                Literal1.Text = "";
                string input = Convert.ToString(TextBox1.Text);
                

                tw.SearchTweets(input);
                
                List<string> lstring = new List<string>();
                string keyword = Convert.ToString(TextBox2.Text);
                lstring.Add(keyword);
                keyword = Convert.ToString(TextBox3.Text);
                lstring.Add(keyword);
                tw.setKeyword(lstring);
                tw.kmp();


                Literal1.Mode = LiteralMode.PassThrough;
                printTweet();


            }
        }
    }

   


}