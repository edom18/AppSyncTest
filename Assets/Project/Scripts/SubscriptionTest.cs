using System;
using System.Threading;
using GraphQL;
using UnityEngine;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

public class SubscriptionTest : MonoBehaviour
{
    private const string API_URL = "https://fpvzdxtqmvey3n5fuhumkfs45u.appsync-api.us-east-2.amazonaws.com/graphql";
    private const string API_KEY = "da2-3opmwtujrrha3o4vakhze65xmm";

    public class EventType
    {
        public string id;
        public string name;
        public string where;
        public string when;
        public string description;
    }

    public class GetEventResponse
    {
        public EventType getEvent { get; set; }
    }

    public async void OnClick()
    {
        var graphQLClient = new GraphQLHttpClient(API_URL, new NewtonsoftJsonSerializer());
        graphQLClient.HttpClient.DefaultRequestHeaders.Add("x-api-key", API_KEY);

        var query = new GraphQLRequest
        {
            Query = @"
            query GetEvent($id: ID!) {
                getEvent(id: $id) {
                    id
                    name
                    description
                }
            }",

            Variables = new
            {
                id = "c49ceb83-17f5-43b3-a511-98c3721841d2"
            },
        };

        Debug.Log($"Query is {query.Query}");

        var response = await graphQLClient.SendQueryAsync<GetEventResponse>(query, CancellationToken.None);

        Debug.Log(response.Data.getEvent.name + ":" + response.Data.getEvent.description);
    }

    // public async void OnClick2()
    // {
    //     var graphQLClient = new GraphQLHttpClient(API_URL, new NewtonsoftJsonSerializer());
    //     graphQLClient.HttpClient.DefaultRequestHeaders.Add("x-api-key", API_KEY);
    //
    //     var request = new GraphQLRequest
    //     {
    //         Query = @"
    //             subscription {
    //                 
    //             }
    //         ",
    //     };
    //
    //     IObservable<GraphQLResponse<HogeResponse>> subscriptionStream = graphQLClient.CreateSubscriptionStream<HogeResponse>(request);
    //     var subscription = subscriptionStream.Subscribe(response => { Debug.Log(response); });
    // }
}