using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using UnityEngine;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using GraphQL.Client.Abstractions;
using UnityEditor.PackageManager.Requests;

public class HelloWorldButton : MonoBehaviour
{
    private const string API_URL = "https://zrzxcskrlfcsxmvuusj2knj6iu.appsync-api.us-east-2.amazonaws.com/graphql";
    private const string WSS_API_URL = "wss://zrzxcskrlfcsxmvuusj2knj6iu.appsync-realtime-api.us-east-2.amazonaws.com/graphql";
    private const string API_KEY = "da2-jdorukduzzdbbgrfubngjzi24a";

    public class ResultType
    {
        public string name { get; set; }
        public int age { get; set; }
    }

    public class QueryResponse
    {
        public ResultType getHoge { get; set; }
    }

    public class MutationResponse
    {
        public ResultType createHoge { get; set; }
    }

    public class SubscriptionResponse
    {
        public ResultType subscribeToHoge { get; set; }
    }

    private void OnDestroy()
    {
        _subscription?.Dispose();
    }

    private class AppSyncHeader
    {
        [JsonProperty("host")] public string Host { get; set; }

        [JsonProperty("x-api-key")] public string ApiKey { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string ToBase64String()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(ToJson()));
        }
    }

    public class AuthorizedAppSyncHttpRequest : GraphQLHttpRequest
    {
        private readonly string _authorization;

        public AuthorizedAppSyncHttpRequest(GraphQLRequest request, string authorization) : base(request)
            => _authorization = authorization;

        public override HttpRequestMessage ToHttpRequestMessage(GraphQLHttpClientOptions options, IGraphQLJsonSerializer serializer)
        {
            HttpRequestMessage result = base.ToHttpRequestMessage(options, serializer);
            result.Headers.Add("X-Api-Key", _authorization);
            return result;
        }
    }

    private IDisposable _subscription;

    public async void OnClick()
    {
        var graphQLClient = new GraphQLHttpClient(API_URL, new NewtonsoftJsonSerializer());
        graphQLClient.HttpClient.DefaultRequestHeaders.Add("x-api-key", API_KEY);

        var query = new GraphQLRequest
        {
            Query = @"
            query MyQuery {
                getHoge {
                    name
                    age
                }
            }",
        };

        Debug.Log($"Query is {query.Query}");

        var response = await graphQLClient.SendQueryAsync<QueryResponse>(query, CancellationToken.None);

        Debug.Log(response.Data.getHoge.name + ":" + response.Data.getHoge.age);
    }

    public async void OnClick2()
    {
        string host = "zrzxcskrlfcsxmvuusj2knj6iu.appsync-api.us-east-2.amazonaws.com";
        string whost = "zrzxcskrlfcsxmvuusj2knj6iu.appsync-realtime-api.us-east-2.amazonaws.com";

        var graphQLClient = new GraphQLHttpClient($"https://{host}/graphql", new NewtonsoftJsonSerializer());

        AppSyncHeader appSyncHeader = new AppSyncHeader
        {
            Host = host,
            ApiKey = API_KEY,
        };

        string header = appSyncHeader.ToBase64String();

        graphQLClient.Options.WebSocketEndPoint = new Uri($"wss://{whost}/graphql?header={header}&payload=e30=");
        graphQLClient.Options.PreprocessRequest = (req, client) =>
        {
            GraphQLHttpRequest result = new AuthorizedAppSyncHttpRequest(req, API_KEY)
            {
                ["data"] = JsonConvert.SerializeObject(req),
                ["extensions"] = new
                {
                    authorization = appSyncHeader,
                }
            };
            return Task.FromResult(result);
        };

        await graphQLClient.InitializeWebsocketConnection();

        Debug.Log("Initialized a web scoket connection.");

        GraphQLRequest request = new GraphQLRequest
        {
            Query = @"
            subscription MySubscription {
              subscribeToHoge {
                name
                age
              }
            }",
        };

        var subscriptionStream = graphQLClient.CreateSubscriptionStream<SubscriptionResponse>(request, ex => { Debug.Log(ex); });
        _subscription = subscriptionStream.Subscribe(
            response => Debug.Log(response.Data.subscribeToHoge.name),
            exception => Debug.Log(exception),
            () => Debug.Log("Completed."));
    }

    public async void OnClick3()
    {
        var graphQLClient = new GraphQLHttpClient(API_URL, new NewtonsoftJsonSerializer());
        graphQLClient.HttpClient.DefaultRequestHeaders.Add("x-api-key", API_KEY);

        var request = new GraphQLRequest
        {
            Query = @"
                mutation MyMutation {
                    createHoge(age: 10, name: """") {
                        name
                        age
                    }
                }
            ",
        };

        var response = await graphQLClient.SendQueryAsync<MutationResponse>(request, CancellationToken.None);

        Debug.Log(response.Data.createHoge.age);
    }
}