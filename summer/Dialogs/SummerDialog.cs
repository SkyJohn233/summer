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

        private String GetRandomResString()
        {
            String[] randomResponseString = { $"不是很懂你们大佬在说什么，请问我一点关于俱乐部的问题吧！", $"啊咧咧，我刚刚开小差了，请换种方式再说一下吧", $"主人还没有教我这些哦，请问我一些俱乐部的问题吧" };
            Random ran = new Random();
            int key = ran.Next(0, randomResponseString.Length - 1);
            return randomResponseString[key];
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
                try
                {
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
                        answer = GetRandomResString();
                    else if (searchResults[0].Score > 97.0)
                        answer = "嘿嘿，就知道你要问我这个，你可算问对人了。" + searchResults[0].Answer;
                    else if (searchResults[0].Score < 50.0)
                        answer = "不好意思，我对我的回答不很自信，我觉得你可能在问：" + searchResults[0].Questions[0] + "我觉得可能的答案是：" + searchResults[0].Answer;
                    await context.PostAsync(answer);
                }
                catch(HttpRequestException)
                {
                    await context.PostAsync("我好像连不上微软认知服务了。。。");
                }
                
            }
            context.Wait(MessageReceivedAsync);
        }

        
    }
}

