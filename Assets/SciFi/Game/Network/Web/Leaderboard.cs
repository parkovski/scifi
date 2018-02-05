using UnityEngine;
using UnityEngine.Networking;
using System;

#if ENABLE_FACEBOOK
using Facebook.Unity;
#endif

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

        static bool CheckFinishedRequest(UnityWebRequest request) {
            if (!request.isDone) {
                Debug.LogWarning("Unfinished request");
                return false;
            }
            if (request.isNetworkError) {
                Debug.LogWarning("Request error: " + request.error);
                return false;
            }
            return true;
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
            if (!CheckFinishedRequest(finishedRequest)) {
                return null;
            }
            return JsonArray.FromJson<PlayerStats>(finishedRequest.downloadHandler.text);
        }

        public static UnityWebRequest PostMatchResultsRequest(MatchResult matchResult) {
            return CreateRequest(
                Uri.EscapeUriString(string.Format(
                    "/match/new?auth={0}&winner={1}&players={2}",
                    "secret",
                    matchResult.winner,
                    JsonArray.ToJson(matchResult.players)
                )),
                "POST"
            );
        }

        public static UnityWebRequest GetPlayerIdForNameRequest(string name) {
            return CreateRequest("/player/id-for-name/" + Uri.EscapeDataString(name));
        }

        public static int GetPlayerIdForNameResult(UnityWebRequest finishedRequest) {
            if (!CheckFinishedRequest(finishedRequest)) {
                return -1;
            }
            int id;
            if (!int.TryParse(finishedRequest.downloadHandler.text, out id)) {
                return -1;
            }
            return id;
        }

#if ENABLE_FACEBOOK
        public static YieldPromise<ulong, string> GetFacebookIdForAccessToken(string accessToken) {
            var query = "/me?access_token=" + Uri.EscapeDataString(accessToken);
            var promise = new YieldPromise<ulong, string>();
            FB.API(query, HttpMethod.GET, result => {
                if (string.IsNullOrEmpty(result.Error)) {
                    promise.Reject(result.Error);
                } else {
                    var fbUserIdStr = result.ResultDictionary["id"].ToString();
                    ulong fbUserId;
                    if (ulong.TryParse(fbUserIdStr, out fbUserId)) {
                        promise.Resolve(fbUserId);
                    } else {
                        promise.Reject("Couldn't parse Facebook user ID");
                    }
                }
            });
            return promise;
        }

        public static UnityWebRequest GetPlayerIdForFacebookIdRequest(ulong fbid) {
            return CreateRequest("/player/id-for-fbid/" + fbid);
        }
#endif

        public static int GetPlayerIdForFacebookIdResult(UnityWebRequest finishedRequest) {
            if (!CheckFinishedRequest(finishedRequest)) {
                return -1;
            }
            int id;
            if (!int.TryParse(finishedRequest.downloadHandler.text, out id)) {
                return -1;
            }
            return id;
        }
    }
}