using System;
using System.Collections.Generic;
using System.Net;

namespace Petronas.Services.Social.Constants
{
    public static class ErrorMessages
    {
        public class Application
        {
            public static readonly KeyValuePair<string, string> ApplicationIdNotValid = new KeyValuePair<string, string>("APP001", $"Application Id {NotValid}");
            public static readonly KeyValuePair<string, string> ApplicationIdIsRequired = new KeyValuePair<string, string>("APP002", $"Application Id {IsRequired}");
            public static readonly KeyValuePair<string, string> ApplicationNameIsRequired = new KeyValuePair<string, string>("APP003", $"Application Name {IsRequired}");
            public static readonly KeyValuePair<string, string> ApplicationNotFound = new KeyValuePair<string, string>("APP004", $"Application {NotFound}");
            public static readonly KeyValuePair<string, string> ApplicationDuplicated = new KeyValuePair<string, string>("APP005", $"Application Name {Duplicated}");
            public static readonly KeyValuePair<string, string> ApplicationNotFoundOrNotAllowed = new KeyValuePair<string, string>("APP006", $"Application not found or not allowed in this environment.");
            public static readonly KeyValuePair<string, string> PropertyNotDefined = new KeyValuePair<string, string>("APP007", $"{genericValue} is not defined in application.");
        }

        public class Client
        {
            public static readonly KeyValuePair<string, string> ClientIdNotValid = new KeyValuePair<string, string>("CLT001", $"Client Id {NotValid}");
            public static readonly KeyValuePair<string, string> ClientIdRequired = new KeyValuePair<string, string>("CLT002", $"Client Id { IsRequired}");
            public static readonly KeyValuePair<string, string> ClientNameRequired = new KeyValuePair<string, string>("CLT003", $"Client Name {IsRequired}");
            public static readonly KeyValuePair<string, string> ClientNotFound = new KeyValuePair<string, string>("CLT004", $"Client {NotFound}");
            public static readonly KeyValuePair<string, string> ClientDuplicated = new KeyValuePair<string, string>("CLT005", $"Client Name {Duplicated}");
        }

        public class ClientFeature
        {
            public static readonly KeyValuePair<string, string> ClientFeatureIdNotValid = new KeyValuePair<string, string>("CLF001", $"Client Feature Id {NotValid}");
            public static readonly KeyValuePair<string, string> ClientFeatureIdRequired = new KeyValuePair<string, string>("CLF002", $"Client Feature Id { IsRequired}");
            public static readonly KeyValuePair<string, string> ClientFeatureRequired = new KeyValuePair<string, string>("CLF003", $"Client Feature {IsRequired}");
            public static readonly KeyValuePair<string, string> ClientFeatureNotFound = new KeyValuePair<string, string>("CLF0004", $"Client Feature {NotFound}");
            public static readonly KeyValuePair<string, string> ClientFeatureDuplicated = new KeyValuePair<string, string>("CLF0005", $"Client feature {Duplicated}");
        }

        public class Comment
        {
            public static readonly KeyValuePair<string, string> CommentIdRequired = new KeyValuePair<string, string>("CMT0001", $"Comment Id  {IsRequired}");
            public static readonly KeyValuePair<string, string> CommentNotFound = new KeyValuePair<string, string>("CMT0002", $"Comment {NotFound}");
            public static readonly KeyValuePair<string, string> ContentRequired = new KeyValuePair<string, string>("CMT0003", $"Content  { IsRequired} for comment");
            public static readonly KeyValuePair<string, string> ParentIdRequired = new KeyValuePair<string, string>("CMT0004", $"Parent Id  { IsRequired} for comment");
            public static readonly KeyValuePair<string, string> ResourceIdRequired = new KeyValuePair<string, string>("CMT0005", $"Resource Id  { IsRequired}");
        }

        public class Common
        {
            public static readonly KeyValuePair<string, string> ContractCannotBeNull = new KeyValuePair<string, string>("CMN0001", $"Contract {CannotBeNull}");
            public static readonly KeyValuePair<string, string> UserIdIsRequired = new KeyValuePair<string, string>("CMN0002", $"User ID {IsRequired}");
            public static readonly KeyValuePair<string, string> PageSizeIsInvalid = new KeyValuePair<string, string>("CMN0003", $"Page size {NotValid}");
            public static readonly KeyValuePair<string, string> PointNotValid = new KeyValuePair<string, string>("CMN0004", $"Point cannot be a negative number.");
            public static readonly KeyValuePair<string, string> EnvironmentRequired = new KeyValuePair<string, string>("CMN0005", $"Environment {IsRequired}");
        }

        public class Post
        {
            public static readonly KeyValuePair<string, string> PostIdRequired = new KeyValuePair<string, string>("P001", $"Post Id {IsRequired}");
            public static readonly KeyValuePair<string, string> PostIdNotValid = new KeyValuePair<string, string>("P002", $"Post Id {NotValid}");
            public static readonly KeyValuePair<string, string> PostNotFound = new KeyValuePair<string, string>("P003", $"Post {NotFound}");
            public static readonly KeyValuePair<string, string> PostDuplicated = new KeyValuePair<string, string>("P004", $"Post Name {Duplicated}");
            public static readonly KeyValuePair<string, string> TitleRequired = new KeyValuePair<string, string>("P005", $"Title for the post {IsRequired}");
            public static readonly KeyValuePair<string, string> ContentRequired = new KeyValuePair<string, string>("P006", $"Content for the post {IsRequired}");
        }

         public class Like
        {
            public static readonly KeyValuePair<string, string> LikeIdRequired = new KeyValuePair<string, string>("L001", $"Like Id {IsRequired}");
            public static readonly KeyValuePair<string, string> LikeIdNotValid = new KeyValuePair<string, string>("L002", $"Like Id {NotValid}");
            public static readonly KeyValuePair<string, string> LikeNotFound = new KeyValuePair<string, string>("L003", $"Like {NotFound}");
            public static readonly KeyValuePair<string, string> LikeDuplicated = new KeyValuePair<string, string>("L004", $"Like Name {Duplicated}");
            public static readonly KeyValuePair<string, string> TypeRequired = new KeyValuePair<string, string>("L005", $"Type of Like {IsRequired}");
            public static readonly KeyValuePair<string, string> TypeIdRequired = new KeyValuePair<string, string>("L006", $"Type Id of Like {IsRequired}");
            public static readonly KeyValuePair<string, string> ResourceIdRequired = new KeyValuePair<string, string>("L007", $"Resource Id is  {IsRequired}");
        }

        public class Environment
        {
            public static readonly KeyValuePair<string, string> EnvironmentDuplicated = new KeyValuePair<string, string>("ENV001", $"Environment {Duplicated}");
            public static readonly KeyValuePair<string, string> EnvironmentNotAllowed = new KeyValuePair<string, string>("ENV002", $"Environment is not allowed.");
            public static readonly KeyValuePair<string, string> EnvironmentNotFound = new KeyValuePair<string, string>("ENV003", $"Environment {NotFound}");
            public static readonly KeyValuePair<string, string> EnvironmentDoesnotExist = new KeyValuePair<string, string>("ENV004", $"{genericValue} environment does not exist.");
        }


        public class HashTag
        {
            public static readonly KeyValuePair<string, string> HashTagNameRequired = new KeyValuePair<string, string>("HST001", $"HashTag Name {IsRequired}");
            public static readonly KeyValuePair<string, string> HashTagIdRequired = new KeyValuePair<string, string>("HST001", $"HashTag {Duplicated}");
            public static readonly KeyValuePair<string, string> HashTagNotAllowed = new KeyValuePair<string, string>("HST002", $"HashTag is not allowed.");
            public static readonly KeyValuePair<string, string> HashTagNotFound = new KeyValuePair<string, string>("HST003", $"HashTag {NotFound}");
            public static readonly KeyValuePair<string, string> HashTagDoesnotExist = new KeyValuePair<string, string>("HST004", $"{genericValue} HashTag does not exist.");
        }


        #region Application
        public static string ApplicationIdNotValid = $"Application Id {NotValid}";
        public static string ApplicationNotFound = $"Application {NotFound}";
        public static string ApplicationDuplicated = $"Application Name {Duplicated}";
        #endregion

        #region Client
        public static string ClientIdNotValid = $"Client Id {NotValid}";
        public static string ClientNotFound = $"Client {NotFound}";
        public static string ClientDuplicated = $"Client Name {Duplicated}";
        #endregion

        #region Client Feature
        public static string FeatureNotFound = $"Feature {NotFound}";
        public static string FeatureDuplicated = $"Feature {Duplicated}";
        #endregion

        #region Type
        public static string TypeIdNotValid = $"Type Id {NotValid}";
        public static string TypeNotFound = $"Type {NotFound}";
        #endregion

        #region Hub
        public static string HubNameNotValid = $"Hub Name {NotValid}";
        public const string HubLoadFailed = "Failed to load SignalR Hub Info.";
        #endregion

        #region Common
        public const string IsRequired = "is required.";
        public const string NotValid = "is not valid.";
        public const string NotFound = "is not found.";
        public const string Duplicated = "is duplicated.";
        public const string CannotBeNull = "cannot be null.";
        public const string ContractIsNull = "Contract cannot be null.";
        public const string TitleIsNull = "Title cannot be null.";
        public  static string genericValue = string.Empty;
        #endregion
    }
}
