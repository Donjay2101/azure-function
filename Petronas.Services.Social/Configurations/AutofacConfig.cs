using Autofac;
using AzureFunctions.Autofac.Configuration;
using Petronas.Services.Social.Repositories;
using Petronas.Services.Social.Repositories.Interfaces;
using Petronas.Services.Social.Services;
using Petronas.Services.Social.Services.Interfaces;

namespace Petronas.Services.Social.Configurations
{
    public class DIConfig
    {
        public DIConfig(string functionName)
        {
            DependencyInjection.Initialize(builder =>
            {
                builder.RegisterType<ApplicationRepository>().As<IApplicationRepository>();
                builder.RegisterType<ClientRepository>().As<IClientRepository>();
                builder.RegisterType<ClientFeatureRepository>().As<IClientFeatureRepository>();
                builder.RegisterType<PostRepository>().As<IPostRepository>();
                builder.RegisterType<CommentRepository>().As<ICommentRepository>();
                builder.RegisterType<LikeRepository>().As<ILikeRepository>();
                builder.RegisterType<UserLogRepository>().As<IUserLogRepository>();

                builder.RegisterType<ApplicationService>().As<IApplicationService>();
                builder.RegisterType<ClientService>().As<IClientService>();
                builder.RegisterType<ClientFeatureService>().As<IClientFeatureService>();
                builder.RegisterType<PostService>().As<IPostService>();
                builder.RegisterType<CommentService>().As<ICommentService>();
                builder.RegisterType<LikeService>().As<ILikeService>();
                builder.RegisterType<UserLogService>().As<IUserLogService>();

                builder.RegisterType<HashTagService>().As<IHashTagService>();
                builder.RegisterType<HashTagRepository>().As<IHashTagRepository>();

            }, functionName);
        }
    }

    public class DIConfig_Application
    {
        public DIConfig_Application(string functionName)
        {
            DependencyInjection.Initialize(builder =>
            {
                builder.RegisterType<ApplicationRepository>().As<IApplicationRepository>();
                builder.RegisterType<ApplicationService>().As<IApplicationService>();
            }, functionName);
        }
    }
}
