using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using WebApplication1.Controllers;
using WebApplication1.Data;

namespace WebApplication1.Models
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Approval, ApprovalDto>().ReverseMap();

            CreateMap<Approval, SupplementDto>().ReverseMap();

            CreateMap<Approval, AuditModel>();

            CreateMap<ApprovalSearchParameter, ApprovalResult>().ReverseMap();
        }
    }
}
