﻿using Microsoft.AspNetCore.Mvc;
using Zenon;
using Zenon.Model.NoM.Json;
using Zenon.Utils;
using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.FusePlasma
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapFusePlasmaEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost("/{accountIndex}/fuse", async (
                    IWalletService wallet,
                    INodeService client,
                    [Validate] AccountIndex accountIndex,
                    [FromBody][Validate] FusePlasmaRequest request
                    ) =>
                    {
                        await client.ConnectAsync();

                        // Access wallet account from index
                        var account = await wallet.GetAccountAsync(accountIndex.value);

                        // Retrieve wallet account address
                        var address = await account.GetAddressAsync();

                        var amount = AmountUtils.ExtractDecimals(request.Amount, Constants.CoinDecimals);

                        // Create block
                        var block = client.Api.Embedded.Plasma.Fuse(request.Address, amount);

                        // Send block
                        var response = await client.SendAsync(block, account);

                        return response.ToJson();
                    })
                .WithName("FusePlasma")
                .WithDescription("Fuses QSR to an address to generate plasma")
                .Produces<JAccountBlockTemplate>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }
    }
}