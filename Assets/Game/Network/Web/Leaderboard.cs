using UnityEngine;
using UnityEngine.Networking;
using System;

using SciFi.Util;

namespace SciFi.Network.Web {
    public static class Leaderboard {
        static string GetLeaderboardHostUrl() {
            var url = Config.GetKey("leaderboard server");
            if (string.IsNullOrEmpty(url)) {
                return null;
            }
            if (!url.StartsWith("http://") && !url.StartsWith("https://")) {
                url = "http://" + url;
            }
            if (url.EndsWith("/")) {
                url = url.Substring(0, url.Length - 1);
            }
            return url;
        }

        static UnityWebRequest CreateRequest(string path, string method = "GET") {
            var baseUrl = GetLeaderboardHostUrl();
            if (baseUrl == null) {
                return null;
            }
            if (method == "GET") {
                return UnityWebRequest.Get(baseUrl + path);
            } else if (method == "POST") {
                return UnityWebRequest.Post(baseUrl + path, "");
            } else {
                return null;
            }
        }

        public static UnityWebRequest GetCompetitorStatsRequest(uint playerId) {
            return CreateRequest("/player/" + playerId + "/stats/competitors");
        }

        /// Returns null on error.
        public static PlayerStats[] GetCompetitorStatsResult(UnityWebRequest finishedRequest) {
            if (!finishedRequest.isDone) {
                return null;
            }
            if (finishedRequest.isError) {
                return null;
            }
            return JsonArray.From<PlayerStats>(finishedRequest.downloadHandler.text);
        }

        public static UnityWebRequest PostMatchResultsRequest(MatchResult matchResult) {
            return CreateRequest(
                Uri.EscapeUriString(string.Format(
                    "/match/new?auth={0}&winner={1}&players={2}",
                    "secret",
                    matchResult.winner,
                    JsonUtility.ToJson(matchResult.players)
                )),
                "POST"
            );
        }
    }
}