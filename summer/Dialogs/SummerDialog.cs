using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace summer.Dialogs
{
    [Serializable]
    public class SummerDialog:IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var client = new HttpClient();
            var knowledgeBaseId = "bb5fa08b-3f70-4453-9da4-879e12657f84";
            var SubscriptionKey = "285d4c7ef5d4430895df6fb82b8219c8";
            if (activity != null)
            {
                var body = $"{{\"question\": \"{activity.Text}\"}}";
                
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);

                var uri = "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0/knowledgebases/" + knowledgeBaseId + "/generateAnswer";

                HttpResponseMessage response;

                byte[] byteData = Encoding.UTF8.GetBytes(body);

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PostAsync(uri, content);
                
                }

                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                JObject resp = JObject.Parse(json);
                IList<JToken> results = resp["answers"].Children().ToList();
                IList<QnAMakerResult> searchResults = new List<QnAMakerResult>();
                foreach (JToken re in results)
                {
                    QnAMakerResult searchResult = JsonConvert.DeserializeObject<QnAMakerResult>(re.ToString());
                    searchResults.Add(searchResult);
                }

                var answer = searchResults[0].Answer;
                if (answer.Equals("No good match found in the KB"))
                    answer = "不是很懂你们大佬在说什么，请问我一点关于俱乐部的问题吧！";
                if (searchResults[0].Score < 50.0)
                    answer = "不好意思，我对我的回答不很自信，我觉得你可能在问：" + searchResults[0].Questions[0] + "我觉得可能的答案是：" + searchResults[0].Answer;
                await context.PostAsync(answer);
            }
            context.Wait(MessageReceivedAsync);
        }

        
    }
}

