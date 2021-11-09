using System;
using System.Text;
using System.Threading;
using GraphQL;
using UnityEngine;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;

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
        var headerStr = @" { ""host"": ""zrzxcskrlfcsxmvuusj2knj6iu.appsync-api.us-east-2.amazonaws.com"", ""x-api-key"": ""da2-jdorukduzzdbbgrfubngjzi24a"" }";
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes(headerStr));
        var graphQLClient = new GraphQLHttpClient($"wss://zrzxcskrlfcsxmvuusj2knj6iu.appsync-realtime-api.us-east-2.amazonaws.com/graphql?header={header}&payload=e30=", new NewtonsoftJsonSerializer());
        graphQLClient.Options.UseWebSocketForQueriesAndMutations = true;
        graphQLClient.Options.WebSocketEndPoint = new Uri($"wss://zrzxcskrlfcsxmvuusj2knj6iu.appsync-realtime-api.us-east-2.amazonaws.com/graphql?header={header}&payload=e30=");

        await graphQLClient.InitializeWebsocketConnection();

        Debug.Log("Initialized a web scoket connection.");

        var request = new GraphQLRequest
        {
            Query = @"
            subscription MySubscription {
              subscribeToHoge {
                name
                age
              }
            }",
        };

        var request2 = new GraphQLRequest
        {
            ["data"] = JsonConvert.SerializeObject(request),
            ["extensions"] = new {
                authorization = header,
            }
        };

        var subscriptionStream = graphQLClient.CreateSubscriptionStream<SubscriptionResponse>(request2, ex =>
        {
            Debug.Log(ex);
        });
        _subscription = subscriptionStream.Subscribe(
            response => Debug.Log(response),
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