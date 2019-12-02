using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Petronas.Services.Social.Constants;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.Contracts.FunctionInput;
using Petronas.Services.Social.Models;
using Petronas.Services.Social.Services;

namespace Petronas.Services.Social.Helpers
{
    public static class QueueHelper
    {

        private static async Task SendMessageToQueue(Constants.Enums.UpdateAction action, string payload, QueueMessageContract queueContract, QueueService service)
        {
            switch (action)
            {
                case Constants.Enums.UpdateAction.Increment:
                    await service.AddMessage(QueueActions.UpdatePartial, queueContract, payload);
                    break;
                case Constants.Enums.UpdateAction.Decrement:
                    await service.AddMessage(QueueActions.UpdatePartial, queueContract, payload);
                    break;
            }
        }

        private static (PostQueueContract queueContract, string payload) CreateContract(IncreaseCountInput increaseCountInput)
        {
            var queueContract = new PostQueueContract
            {
                Id = increaseCountInput.Id,
                ApplicationId = increaseCountInput.ApplicationId,
                ClientId = increaseCountInput.ClientId,
                UserId = Guid.NewGuid().ToString(),// For testing only,
                Environment = increaseCountInput.Environment
            };

            var payload = JsonConvert.SerializeObject(new PostContract()
            {
                PropertyName = increaseCountInput.PropertyName,
                Action = increaseCountInput.Action,
                UpdateValue = 1,
                Environment = increaseCountInput.Environment
            });

            return (queueContract, payload);

        }

        public static async Task IncreaseCountInPost<T>(T input, string propertyName, Constants.Enums.UpdateAction action, Constants.Enums.ClientFeature clientFeature, bool isPointIncrease) where T : FunctionInputBase
        {
            var increaseCountInput = new IncreaseCountInput
            {
                Id = input.Id,
                ClientId = input.ClientId,
                ApplicationId = input.ApplicationId,
                PropertyName = propertyName, 
                Action = action,
                Feature = clientFeature,
                Environment = input.Environment
            };
            var contract = CreateContract(increaseCountInput);
            var postQueueService = new QueueService(QueueNames.Posts);
            //Increase  count
            await SendMessageToQueue(action, contract.payload, contract.queueContract, postQueueService);

            //Increase point .
            if (isPointIncrease)
            {
                var featureReesponseModel = await input.ClientFeatureService.GetClientFeature(increaseCountInput.ApplicationId, increaseCountInput.ClientId, increaseCountInput.Feature, increaseCountInput.Environment);
                ClientFeature feature = new ClientFeature();
                if(featureReesponseModel !=null)
                {
                    feature = featureReesponseModel.Value as ClientFeature;
                }
                if (feature != null && feature.Point > 0)
                {
                    var payload = JsonConvert.SerializeObject(new PostContract
                    {
                        PropertyName = "TotalPoint",
                        Action = increaseCountInput.Action,
                        UpdateValue = feature.Point
                    });
                    await SendMessageToQueue(increaseCountInput.Action, payload, contract.queueContract, postQueueService);
                }
            }
        }
    }
}
