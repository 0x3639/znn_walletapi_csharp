﻿using FluentValidation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Zenon;

namespace ZenonWalletApi.Models
{
    public record GetWalletAccountsRequest(int pageIndex = 0, int pageSize = Constants.RpcMaxPageSize)
    {
        [DefaultValue(0)]
        public int pageIndex { get; set; } = pageIndex;

        [DefaultValue(Constants.RpcMaxPageSize)]
        [Range(1, Constants.RpcMaxPageSize)]
        public int pageSize { get; set; } = pageSize;

        public class Validator : AbstractValidator<GetWalletAccountsRequest>
        {
            public Validator()
            {
                RuleFor(x => x.pageIndex).GreaterThanOrEqualTo(0);
                RuleFor(x => x.pageSize).GreaterThan(0).LessThanOrEqualTo(Constants.RpcMaxPageSize);
            }
        }
    }
}