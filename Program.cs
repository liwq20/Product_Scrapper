using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using HtmlAgilityPack; // dotnet add package HtmlAgilityPack dotnet add package HtmlAgilityPack.CssSelectors
using System.Data.SQLite; // dotnet add package System.Data.SQLite


namespace MyApp
{
    internal class Scraper
    {   
        private static readonly string BaseUrl = "https://www.newegg.com/p/pl?d=";
        private static readonly Dictionary<int, string> SortMethods = new Dictionary<int, string>
        {
            { 0, "Featured Items" },
            { 1, "Lowest Price" },
            { 2, "Highest Pricee" },
            { 3, "Best Selling" },
            { 4, "Best Rating" },
            { 5, "Most Reviews" },
        };
        private HtmlWeb web = new HtmlWeb();
        private bool debug;
        private static readonly string dbName = "data.db";
        private static readonly string tableName = "ScrapedData";
        string createQuery = $@"CREATE TABLE IF NOT EXISTS [{tableName}]
                              (
                                [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                [Title] VARCHAR(255) NOT NULL,
                                [Price] REAL(20) NOT NULL,
                                [Ratings] VARCHAR(20) NOT NULL,
                                [Url] VARCHAR(255) NOT NULL
                              )";
        private static readonly string connectionString = $"Data Source={dbName}";
        SQLiteConnection connection = new SQLiteConnection(connectionString);

        public Scraper(bool debug=true){
            this.debug = debug;

            connection.Open();

            SQLiteCommand command = new SQLiteCommand(createQuery, connection);
            command.ExecuteNonQuery();

            if(debug){
                System.Data.DataTable tables = connection.GetSchema("Tables");

                DebugPrint(text: "Tables:");
                foreach (System.Data.DataRow row in tables.Rows)
                {
                    var tableName = row["TABLE_NAME"].ToString();
                    if(tableName != "sqlite_sequence"){
                        DebugPrint(text: tableName);
                    }
                }
            }
            connection.Close();
        }

        public void DbGetTableColumns(){
            connection.Open();
    
            System.Data.DataTable schemaTable = connection.GetSchema("Columns", new string[] { null, null, tableName});
            
            foreach (System.Data.DataRow row in schemaTable.Rows)
            {
                string columnName = row["COLUMN_NAME"].ToString();
                string dataType = row["DATA_TYPE"].ToString();
                
                Console.WriteLine($"Column name: {columnName}, Data type: {dataType}");
            }
            
            connection.Close();
        }

        public void DbInsertRow(string title, decimal price, string rating, string url){
            DebugPrint(text: $"Inserting row with params:");
            DebugPrint(text: $"title: {title}");
            DebugPrint(text: $"price: {price}");
            DebugPrint(text: $"rating: {rating}");
            DebugPrint(text: $"url: {url}");
            
            connection.Open();

            string sqlCommand = $@"INSERT INTO {tableName} (Title, Price, Ratings, Url) VALUES (@Title, @Price, @Ratings, @Url)";
            SQLiteCommand command = new SQLiteCommand(sqlCommand, connection);
        
            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@Price", price);
            command.Parameters.AddWithValue("@Ratings", rating);
            command.Parameters.AddWithValue("@Url", url);

            // DebugPrint(text: $"sqlCommand: {command.CommandText}");
            int rowsAffected = command.ExecuteNonQuery();
            if(rowsAffected == 0){
                ColorPrint(text: "[ERROR] 0 rows affected when inserting", color: ConsoleColor.Red);
                ColorPrint(text: $"sqlCommand: {command.CommandText}", color: ConsoleColor.Red);
                ColorPrint(text: "Used parameteres:", color: ConsoleColor.Red);
                foreach (SQLiteParameter parameter in command.Parameters)
                {
                    ColorPrint($"{parameter.ParameterName}: {parameter.Value}", color: ConsoleColor.Red);
                }
            }

            DebugPrint(text: $"rowsAffected: {rowsAffected}");

            connection.Close();
        }
        
        public void DbBulkInsert(Dictionary<int, Tuple<string, decimal, string, string>> data){
            foreach (KeyValuePair<int, Tuple<string, decimal, string, string>> kvp in data)
            {   
                var row = kvp.Value;
                DbInsertRow(row.Item1, row.Item2, row.Item3, row.Item4);
            }
        }
        public List<Tuple<int, string, decimal, string, string>> DbFetchData(int maxRows=0, string filter=""){
            connection.Open();

            string sql = "";
            string limit = "";

            if(maxRows != 0){
                limit = $" LIMIT {maxRows}";
            }

            if(filter.Equals("")){
                sql = $"SELECT * FROM {tableName}{limit}";
            }
            else{
                sql = $"SELECT * FROM {tableName} WHERE {filter}{limit}";
            }
            

            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            
            List<Tuple<int, string, decimal, string, string>> records = new List<Tuple<int, string, decimal, string, string>>();
            while (reader.Read())
            {     
                int rowId = reader.GetInt32(0);
                string title = reader.GetString(1);
                decimal price = reader.GetDecimal(2);
                string rating = reader.GetString(3);
                string url = reader.GetString(4);

                Tuple<int, string, decimal, string, string> row = new Tuple<int, string, decimal, string, string>
                (rowId, title, price, rating, url);

                records.Add(row); 
            }
            connection.Close();

            return records;
        }

        public void DbTruncateTable(){
            connection.Open();

            Console.WriteLine($"TRUNCATE TABLE {tableName}");
            SQLiteCommand command = new SQLiteCommand($"DELETE FROM {tableName}", connection); // there is no TRUNCATE command in sqlite
            command.ExecuteNonQuery();

            ColorPrint("[INFO] Table truncated");

            connection.Close();
        }

        public int DbDeleteRecord(string filter){
            connection.Open();

            SQLiteCommand command = new SQLiteCommand($"DELETE FROM {tableName} WHERE {filter}", connection);
            int rowsAffected = command.ExecuteNonQuery();

            DebugPrint($"Number of deleted rows: {rowsAffected}");

            connection.Close();

            return rowsAffected;
        }

        public HtmlAgilityPack.HtmlNode QueryInNode(HtmlAgilityPack.HtmlNode parent, string tag, string attr, string attr_name){

            HtmlAgilityPack.HtmlNode node = parent.Descendants(tag).
            First(x => x.Attributes[attr] != null && x.Attributes[attr].Value == attr_name);

            return node;
        }

        public void ColorPrint(string text, ConsoleColor color = ConsoleColor.Green){
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void DebugPrint(string text){
            if(debug){
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"[DEBUG] {text}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public string ClearString(string text, string pattern){
            DebugPrint(text: $"Text: {text}");
            DebugPrint(text: $"Pattern: {pattern}");

            Regex regex = new Regex(pattern);
            Match match = regex.Match(text);

            DebugPrint(text: $"Match: {match.ToString()}");

            return match.ToString();
        }

        public void SaveToCsv(string outputPath, List<string> titles, List<string> prices, List<string> ratings,
                             List<string> urls, bool append=false){
            bool addHeader = false;

            if (!File.Exists(outputPath)){
                addHeader = true;
            }

            try{
                DebugPrint(text: $"append: {append}");
                DebugPrint(text: $"outputPath: {outputPath}");
                
                using(var w = new StreamWriter(outputPath, append: append))
                {
                    if(addHeader){
                        string row = $"Title,Price,Ratings,Url";
                        w.WriteLine(row);
                        w.Flush();
                    }

                    for(int i=0; i<urls.Count(); i++)
                    {
                        string row = $"{titles[i]},{prices[i]},{ratings[i]},{urls[i]}";
                        
                        DebugPrint(text: $"row: {row}");
                        w.WriteLine(row);
                        w.Flush();
                    }
                }

                ColorPrint(text: "[INFO] Saved to csv");
            }
            catch(System.IO.DirectoryNotFoundException){
                ColorPrint(text: "[ERROR] Folder not found", color: ConsoleColor.Red);
            }
            catch (Exception ex){
                ColorPrint(text: $"[ERROR] Unknown error while saving to csv file: {ex}", color: ConsoleColor.Red);
            }

        }
        
        public int GetPagesNum(string phrase, int sortMethod){
            string current_url = $"{BaseUrl}{phrase}&Order={sortMethod}&page=1";

            DebugPrint(text: $"current_url: {current_url}");

            var document = web.Load(current_url);
            var pagesRaw = document.QuerySelector("#app > div.page-content > section > div > div > div.row-body > div:nth-child(1) > div > div > div.row-body > div.row-body-inner > div > div:nth-child(1) > div.list-tool-pagination > span");

            DebugPrint(text: $"pagesRaw.InnerText: {pagesRaw.InnerText}");
            DebugPrint(text: $"pagesRaw.InnerText: {pagesRaw.InnerText.Split("/").ToString()}");

            return Int32.Parse(pagesRaw.InnerText.Split("/")[1]);
        }
        
        public decimal ClearPrice(string price){
            decimal resPrice;

            if(price == ""){
                resPrice = 0;
            }
            else{
                resPrice = decimal.Parse(price.Replace(",", "").Replace(".", ","));
            }

            return resPrice;
        }
        public Dictionary<int, Tuple<string, decimal, string, string>> GetData(string phrase, int sortMethod, int maxPages,
                                                        string outputPath="", bool append=false){
            Stopwatch watch = Stopwatch.StartNew();
            
            ColorPrint(text: "[INFO] Starting...");

            DebugPrint(text: $"phrase: {phrase}");
            DebugPrint(text: $"sortMethod: {sortMethod}");
            DebugPrint(text: $"maxPages: {maxPages}");
            DebugPrint(text: $"outputPath: {outputPath}");
            DebugPrint(text: $"append: {append}");

            List<string> urls = new List<string>();
            List<string> titles = new List<string>();
            List<string> prices = new List<string>();
            List<string> ratings = new List<string>();

            phrase = phrase.Replace(" ", "+");
            int pages = GetPagesNum(phrase: phrase, sortMethod: sortMethod);

            if(maxPages > pages){
                maxPages = pages;
            }

            if(maxPages == 0){
                maxPages = pages;
            }

            ColorPrint(text: $"[INFO] {pages} pages found, maxPages is set to: {maxPages}");

            for (int i = 1; i <= pages+1; i++)
            {   
                string currentUrlP = $"{BaseUrl}{phrase}&Order={sortMethod}&page={i}";
                var documentP = web.Load(currentUrlP);

                var items = documentP.DocumentNode.SelectNodes("//div[contains(@class, 'item-cell')]");
                // Console.WriteLine(currentUrlP);

                DebugPrint(text: $"Number of items on page: {items.Count()}");
                foreach (var item in items)
                {   
                    try{
                        var titleRaw = QueryInNode(item, "a", "class", "item-title");

                        string itemTitle = titleRaw.InnerText;
                        string itemUrl = titleRaw.Attributes["href"].Value;

                        var priceRaw = QueryInNode(item, "ul", "class", "price");
                        string itemPrice = ClearString(priceRaw.InnerText, @"\d{1,3}(?:[.,]\d{3})*(?:[.,]\d{2})");
                        
                        string finalRating = "";
                        try{
                            var rating = QueryInNode(item, "a", "class", "item-rating").Attributes["title"].Value;
                            var ratingNum = QueryInNode(item, "a", "class", "item-rating").InnerText;
                            finalRating = $"{rating} {ratingNum}";
                        }
                        catch(System.InvalidOperationException){
                            finalRating = "None";
                        }
                        
                        // DebugPrint(text: $"itemTitle: {itemTitle}");
                        // DebugPrint(text: $"itemUrl: {itemUrl}");
                        // DebugPrint(text: $"itemPrice: {itemPrice}");
                        // DebugPrint(text: $"finalRating: {finalRating}");

                        urls.Add(itemUrl);
                        titles.Add(itemTitle);
                        prices.Add(itemPrice);
                        ratings.Add(finalRating);

                    }
                    catch (System.InvalidOperationException ex){
                        DebugPrint(text: $"{ex}");
                    }
                    catch (Exception ex){
                        ColorPrint(text: $"[ERROR] Unknown error while scraping: {ex}", color: ConsoleColor.Red);
                    }
                }

                ColorPrint(text: $"[INFO] page {i}/{maxPages} scraped");
                if(i == maxPages){
                    DebugPrint(text: $"Max pages: {i}, {maxPages}");
                    break;
                }
            }

            if(outputPath != "" & outputPath.EndsWith(".csv")){
                SaveToCsv(outputPath: outputPath, titles: titles, prices: prices, ratings: ratings, urls: urls, append: append);
            }

            DebugPrint(text: $"Titles count: {titles.Count()}");
            DebugPrint(text: $"Prices count: {prices.Count()}");
            DebugPrint(text: $"Ratings count: {ratings.Count()}");
            DebugPrint(text: $"Urls count: {urls.Count()}");

            Dictionary<int, Tuple<string, decimal, string, string>> scrapedData = 
            new Dictionary<int, Tuple<string, decimal, string, string>>();

            for (int i = 0; i < titles.Count(); i++)
            {   
                Tuple<string, decimal, string, string> row = new Tuple<string, decimal, string, string>
                (titles[i], ClearPrice(prices[i]), ratings[i], urls[i]);

                scrapedData.Add(i, row);
            }

            DebugPrint(text: $"scrapedData: {scrapedData}");
            watch.Stop();
            
            ColorPrint(text: $"[INFO] Finished in: {watch.Elapsed}");

            return scrapedData;
        }
        static void Main(string[] args)
        {   
            Scraper scraper = new Scraper(debug: true);
            scraper.DbGetTableColumns();
            scraper.DbTruncateTable();

            var recordsAfterTruncate = scraper.DbFetchData(maxRows: 0);
            Console.WriteLine(recordsAfterTruncate.Count());

            var data = scraper.GetData(phrase: "rtx 3080", sortMethod: 0, maxPages: 2);

            scraper.DbBulkInsert(data);
            var recordsBefore = scraper.DbFetchData(maxRows: 400);
            Console.WriteLine(recordsBefore.Count());

            scraper.DbDeleteRecord(filter: "Price > 800");
            var recordsAfter = scraper.DbFetchData(maxRows: 400);
            Console.WriteLine(recordsAfter.Count());

            foreach (var item in recordsAfter)
            {
                Console.WriteLine(item);
            }
        }
    }
}
