using AutoMapper;
using Cook_Book_Shared_Code.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cook_Book_Client_Desktop
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RecipeModel, RecipeModelDisplay>();
            CreateMap<RecipeModelDisplay, RecipeModel>();
        }
    }
}
