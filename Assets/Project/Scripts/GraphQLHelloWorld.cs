using System;
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
using TMPro;

public class GraphQLHelloWorld : MonoBehaviour
{
    [SerializeField] private string _host = "";
    [SerializeField] private string _realtimeHost = "";
    [SerializeField] private string _apiKey = "";

    [SerializeField] private TMP_InputField _queryInputField;
    [SerializeField] private TMP_InputField _mutationInputField;
    [SerializeField] private TMP_InputField _subscriptionInputField;
        
    public class EventType
    {
        public string id { get; set; }
        public string name { get; set; }
        public string where { get; set; }
        public string when { get; set; }
        public string description { get; set; }
    }

    public class CommentType
    {
        public string eventId { get; set; }
        public string commentId { get; set; }
        public string content { get; set; }
        public string createdAt { get; set; }
    }

    public class QueryResponse
    {
        public EventType getEvent { get; set; }
    }

    public class CreateMutationResponse
    {
        public EventType createEvent { get; set; }
    }

    public class CreateCommentResponse
    {
        public CommentType commentOnEvent { get; set; }
    }

    public class SubscriptionResponse
    {
        public CommentType subscribeToEventComments { get; set; }
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

    public async void OnClickQuery()
    {
        GraphQLHttpClient graphQLClient = new GraphQLHttpClient($"https://{_host}/graphql", new NewtonsoftJsonSerializer());
        graphQLClient.HttpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

        GraphQLRequest query = new GraphQLRequest
        {
            Query = _queryInputField.text,
        };

        var response = await graphQLClient.SendQueryAsync<QueryResponse>(query, CancellationToken.None);

        Debug.Log($"[Query] {JsonConvert.SerializeObject(response.Data)}");
    }

    public async void OnClickMutation()
    {
        GraphQLHttpClient graphQLClient = new GraphQLHttpClient($"https://{_host}/graphql", new NewtonsoftJsonSerializer());
        graphQLClient.HttpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

        GraphQLRequest request = new GraphQLRequest
        {
            Query = _mutationInputField.text,
        };

        var response = await graphQLClient.SendQueryAsync<CreateCommentResponse>(request, CancellationToken.None);

        Debug.Log($"[Mutation] {JsonConvert.SerializeObject(response.Data)}");
    }

    public async void OnClickSubscription()
    {
        GraphQLHttpClient graphQLClient = new GraphQLHttpClient($"https://{_host}/graphql", new NewtonsoftJsonSerializer());

        AppSyncHeader appSyncHeader = new AppSyncHeader
        {
            Host = _host,
            ApiKey = _apiKey,
        };

        string header = appSyncHeader.ToBase64String();

        graphQLClient.Options.WebSocketEndPoint = new Uri($"wss://{_realtimeHost}/graphql?header={header}&payload=e30=");
        graphQLClient.Options.PreprocessRequest = (req, client) =>
        {
            GraphQLHttpRequest result = new AuthorizedAppSyncHttpRequest(req, _apiKey)
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
            Query = _subscriptionInputField.text,
        };

        var subscriptionStream = graphQLClient.CreateSubscriptionStream<SubscriptionResponse>(request, ex => { Debug.Log(ex); });
        _subscription = subscriptionStream.Subscribe(
            response => Debug.Log($"[Subscription] {JsonConvert.SerializeObject(response.Data)}"),
            exception => Debug.Log(exception),
            () => Debug.Log("Completed."));
    }
}