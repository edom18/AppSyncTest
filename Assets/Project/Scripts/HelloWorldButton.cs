using System;
using System.Threading;
using GraphQL;
using UnityEngine;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

public class HelloWorldButton : MonoBehaviour
{
    private const string API_URL = "https://zrzxcskrlfcsxmvuusj2knj6iu.appsync-api.us-east-2.amazonaws.com/graphql";
    private const string API_KEY = "da2-jdorukduzzdbbgrfubngjzi24a";

    public class ResultType
    {
        public string name { get; set; }
        public int age { get; set; }
    }

    public class HogeResponse
    {
        public ResultType getHoge { get; set; }
    }

    public class MutationResponse
    {
        public ResultType createHoge { get; set; }
    }
    
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
        
        var response = await graphQLClient.SendQueryAsync<HogeResponse>(query, CancellationToken.None);

        Debug.Log(response.Data.getHoge.name + ":" + response.Data.getHoge.age);
    }

    public async void OnClick2()
    {
        var graphQLClient = new GraphQLHttpClient(API_URL, new NewtonsoftJsonSerializer());
        graphQLClient.HttpClient.DefaultRequestHeaders.Add("x-api-key", API_KEY);
        
        var request = new GraphQLRequest
        {
            Query = @"
                subscription {
                    
                }
            ",
        };

        IObservable<GraphQLResponse<HogeResponse>> subscriptionStream = graphQLClient.CreateSubscriptionStream<HogeResponse>(request);
        var subscription = subscriptionStream.Subscribe(response =>
        {
            Debug.Log(response);
        });
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
