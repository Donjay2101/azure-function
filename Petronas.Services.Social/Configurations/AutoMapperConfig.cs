using AutoMapper;
using Petronas.Services.Social.Contracts;
using Petronas.Services.Social.Models;

namespace Petronas.Services.Social.Configurations
{
    public static class AutoMapperConfig
    {
        public static void Initialize()
        {
            try
            {
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<ApplicationContract, Application>();
                    cfg.CreateMap<ClientContract, Client>();
                    cfg.CreateMap<ClientFeatureContract, ClientFeature>();
                    cfg.CreateMap<PostContract, Post>();
                    cfg.CreateMap<CommentContract, Comment>();
                    cfg.CreateMap<LikeContract, Like>();
                    cfg.CreateMap<UserLogContract, UserLog>();
                    cfg.CreateMap<HashTagContract, HashTag>();
                });
            }
            catch (System.Exception)
            {
            }
        }

        public static TDestination MapObject<TSource, TDestination>(TSource source)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMissingTypeMaps = true;
                cfg.CreateMap<TSource, TDestination>();
            });

            var mapper = config.CreateMapper();
            return mapper.Map<TSource, TDestination>(source);
        }
    }
}
