using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CoreCodeCamp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using CoreCodeCamp.Models;


namespace CoreCodeCamp.Data
{
    public class CampProfile : Profile 
    {
        public CampProfile()
        {

            //'this' indicates it's from the Profile class
            this.CreateMap<Camp, CampModel>()
            //this tells the mapper when looking at a member object under venue, search the venue name under the Location entity
                .ForMember(c => c.Venue, o => o.MapFrom(m => m.Location.VenueName))
                .ReverseMap();

            this.CreateMap<Talk, TalkModel>()
                .ReverseMap();

            this.CreateMap<Speaker, SpeakerModel>()
                .ReverseMap();

        }
    }
}