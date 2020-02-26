using System;
using System.Linq;
using AutoMapper;
using ClientModel.Dtos.Mappings;

namespace ClientModel.Test.DataAccess
{
    public static class ClientModelMapperProvider
    {
        public static IMapper CreateAutoMapper()
        {
            var profiles = from t in typeof(AccountProfile).Assembly.GetTypes()
                where typeof(Profile).IsAssignableFrom(t)
                select (Profile)Activator.CreateInstance(t);

            var configuration = new MapperConfiguration(config =>
            {
                foreach (var profile in profiles)
                {
                    config.AddProfile(profile);
                }
            });

            return configuration.CreateMapper();
        }
    }
}