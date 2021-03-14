using AutoMapper;
using Common;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WebFramework.CustomMapping;

namespace Api.UnitTest
{
    public static class AutoMapperSingleton
    {
        private static IMapper _mapper;
        public static IMapper Mapper
        {
            get
            {
                if (_mapper == null)
                {
                    // Auto Mapper Configurations
                    var mappingConfig = new MapperConfiguration(mc =>
                    {
                        mc.AddCustomMappingProfile();
                    });
                    IMapper mapper = mappingConfig.CreateMapper();
                    _mapper = mapper;
                }
                return _mapper;
            }
        }

        //public static void AddCustomMappingProfile(this IMapperConfigurationExpression config)
        //{
        //    config.AddCustomMappingProfile(Assembly.GetEntryAssembly(), typeof(JwtService).Assembly);
        //}

        //public static void AddCustomMappingProfile(this IMapperConfigurationExpression config, params Assembly[] assemblies)
        //{
        //    var allTypes = assemblies.SelectMany(a => a.ExportedTypes);

        //    var list = allTypes.Where(type => type.IsClass && !type.IsAbstract &&
        //        type.GetInterfaces().Contains(typeof(IHaveCustomMapping)))
        //        .Select(type => (IHaveCustomMapping)Activator.CreateInstance(type));

        //    var profile = new CustomMappingProfile(list);

        //    config.AddProfile(profile);
        //}
    }
}
