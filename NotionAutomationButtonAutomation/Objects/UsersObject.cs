using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NotionAutomationButtonAutomation.Objects
{
    public class UsersObject
    {
        [JsonPropertyName("object")] 
        public string Object { get; set; }

        [JsonPropertyName("results")]
        public List<UserObject> Results { get; set; }

        public string NextCursor { get; set; }

        public bool HasMore { get; set; }

        public string Type { get; set; }

        public object User { get; set; }
    }

    public class UserObject
    {
        [JsonPropertyName("object")] 
        public string Object { get; set; }
        
        [JsonPropertyName("id")] 
        public Guid Id { get; set; }
        
        [JsonPropertyName("name")] 
        public string Name { get; set; }
        
        [JsonPropertyName("avatar_rul")] 
        public string AvatarUrl { get; set; }
        
        [JsonPropertyName("type")] 
        public string Type { get; set; }
        
        [JsonPropertyName("person")] 
        public object Person { get; set; }
        
        [JsonPropertyName("bot")] 
        public object Bot { get; set; }
    }
}