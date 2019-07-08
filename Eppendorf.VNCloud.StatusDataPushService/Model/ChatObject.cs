// <copyright file="ChatObject.cs" company="Eppendorf AG - 2018">
// Copyright (c) Eppendorf AG - 2018. All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace Eppendorf.VNCloud.StatusDataPushService.Model
{
    public class ChatObject
    {
        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
