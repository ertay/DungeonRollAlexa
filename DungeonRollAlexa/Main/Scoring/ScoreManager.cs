using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using DungeonRollAlexa.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonRollAlexa.Main.Scoring
{
    public class ScoreManager
    {
        public List<Score> HighScores { get; set; }

        public ScoreManager()
        {
            HighScores = new List<Score>();
        }

        /// <summary>
        /// Updates the score if it is a new high score
        /// </summary>
        /// <param name="newScore"></param>
        /// <returns></returns>
        public bool UpdateHighScore(Score newScore)
        {
            // check if this hero has a high score
            var score = HighScores.FirstOrDefault(s => s.HeroType == newScore.HeroType);
            if(score == null)
            {
                // this hero does not have a previous high score
                // add new score and return
                HighScores.Add(newScore);
                return true;
            }
            // we already have a score with this hero, let's check if new score is higher
            if(newScore.Points > score.Points)
            {
                // update old score with the new high score
                score.Points = newScore.Points;
                return true;
            }

            // the new score is not a high score
            return false;
        }

        public async Task<string> GetFormattedHighScores(string userId)
        {
            bool loaded = await LoadScoresFromDb(userId);
            if(!loaded || HighScores.Count < 1)
            {
                return "You haven't set any high scores yet. Say new game to start a new game. ";
            }

            string message = "Here is a list of your high scores with each hero. ";
            int count = 1;
            foreach (var item in HighScores.OrderByDescending(s => s.Points))
            {
                message += $"{count}. {Utilities.GetShortHeroName(item.HeroType)}, {item.Points} points with the rank of {item.Rank}. ";
                count++;
            }
            return message;
        }


        public async Task<bool> LoadScoresFromDb(string userId)
        {
            bool success = false;
            var credentials = new BasicAWSCredentials(Environment.GetEnvironmentVariable("AWSAccessId"), Environment.GetEnvironmentVariable("AWSAccessSecret"));
            var client = new AmazonDynamoDBClient(credentials, Amazon.RegionEndpoint.USEast1);
            try
            {
                var table = Table.LoadTable(client, "DungeonRoll");
                var item = await table.GetItemAsync(userId);
                if (item == null || !item.ContainsKey("HighScores"))
                {
                    success = false;
                    return success;
                }

                string jsonScores = item["HighScores"];
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto};
                HighScores = JsonConvert.DeserializeObject<List<Score>>(jsonScores, settings);

                
                Console.WriteLine("High scores loaded from db.");
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return success;
        }

        public async Task<bool> SaveScoresToDb(string userId)
        {
            bool success = false;
            // prepare the scores that will be stored in dynamo db
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto};
            string jsonScores = JsonConvert.SerializeObject(HighScores, settings);
            var credentials = new BasicAWSCredentials(Environment.GetEnvironmentVariable("AWSAccessId"), Environment.GetEnvironmentVariable("AWSAccessSecret"));

            var client = new AmazonDynamoDBClient(credentials, Amazon.RegionEndpoint.USEast1);
            try
            {
                var table = Table.LoadTable(client, "DungeonRoll");
                var item = new Document();
                item["UserId"] = userId;
                item["HighScores"] = jsonScores;

                await table.UpdateItemAsync(item);
                Console.WriteLine("Saved scores to DynamoDB.");
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return success;
        }


    }
}
