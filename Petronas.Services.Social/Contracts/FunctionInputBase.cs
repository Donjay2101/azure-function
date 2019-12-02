using System;
using Microsoft.AspNetCore.Http;
using Petronas.Services.Social.Services.Interfaces;

public class FunctionInputBase
{
    public string Id { get; set; }
    public string ApplicationId { get; set; }
    public string ClientId { get; set; }
    public HttpRequest Request { get; set; }

    public string UserId
    {
        get
        {
            return Guid.NewGuid().ToString();  // For testing only
        }
    }

    public string ParentId{get;set;}
    public string Environment{get;set;}

    public IClientFeatureService ClientFeatureService{get;set;}

}