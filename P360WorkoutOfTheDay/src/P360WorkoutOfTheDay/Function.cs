using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization;
using HtmlAgilityPack;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace P360WorkoutOfTheDay {
    public class Function {

        public async Task<string> FunctionHandler(string input, ILambdaContext context) {
            var date = DateTime.Now;

            // get the list of workouts
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("http://perform-360.com/home/members-area/todays-workout-2/");
            var strContents = await response.Content.ReadAsStringAsync();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strContents);
            var latestWeeksTrainingLink = htmlDoc
                .DocumentNode
                .SelectNodes("//a")
                .Select(x => x.Attributes["href"]?.Value)
                .Where(x => x != null)
                .Where(x => x.Contains("weeks-training-"))
                .First();

            // find the days of the week
            response = await httpClient.GetAsync(latestWeeksTrainingLink);
            strContents = await response.Content.ReadAsStringAsync();
            htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strContents);
            var tabs = htmlDoc
                .DocumentNode
                .SelectNodes("//div[contains(@class, 'tab-pane')]")
                .Skip(1);
            var tabsByDay = new HtmlNode[7];
            
            var day = 0;
            
            var i = 0;
            foreach(var tab in tabs) {
                tabsByDay[i++] = tab; 
            }
            var classTypes = new string[] { "daily challenge", "shred", "olympic weightlifting", "muscle", "kettlebell core" };
            var textByTypeOfWorkout = new Dictionary<string, string>();
            var sb = new StringBuilder();
            string currentWorkout = null;
            foreach (var node in tabsByDay[day].DescendantsAndSelf()) {
                if (!node.HasChildNodes) {
                    string text = node.InnerText;
                    var isHeading = false;
                    foreach(var t in classTypes) {
                        if(text.ToLowerInvariant().Contains(t.ToLowerInvariant())) {
                            if(currentWorkout != null) {
                                textByTypeOfWorkout[currentWorkout] = sb.ToString();
                            }
                            sb.Clear();
                            currentWorkout = t;
                            isHeading = true;
                            break;
                        }
                    }
                    if (!isHeading && !string.IsNullOrEmpty(text)) {
                        sb.AppendLine(text.Trim());
                    }
                }
            }
            return textByTypeOfWorkout["shred"];
        }
    }
}
