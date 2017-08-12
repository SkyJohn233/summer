using System.Collections.Generic;
using Newtonsoft.Json;

namespace summer.Dialogs
{
    public class QnAMakerResult
    {
        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; }

        [JsonProperty(PropertyName = "questions")]
        public List<string> Questions { get; set; }

        [JsonProperty(PropertyName = "score")]
        public double Score { get; set; }
    }
}