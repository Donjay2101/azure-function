using Petronas.Services.Social.Constants.Enums;

namespace Petronas.Services.Social.Contracts.FunctionInput
{

    public class LikeFunctionInput : FunctionInputBase
    {
        public string PostId { get; set; }
        public LikeTypes LikeType { get; set; }
        public string TypeId { get; set; }

    }

    public class PostFunctionInput : FunctionInputBase { }

    public class CommentFunctionInput : FunctionInputBase
    {
        public string PostId { get; set; }
    }


    public class ClientFunctionInput:FunctionInputBase{}

    public class ClientFeatureFunctionInput: FunctionInputBase{}

    public class ApplicationFunctionInput : FunctionInputBase{}

    public class HashTagFunctionInput : FunctionInputBase{}

    public class IncreaseCountInput : FunctionInputBase
    {
        public string PropertyName { get; set; }
        public UpdateAction Action { get; set; }
        public ClientFeature Feature{get;set;}
    }
    public class EnvironmentFunctionInput : FunctionInputBase{}
}