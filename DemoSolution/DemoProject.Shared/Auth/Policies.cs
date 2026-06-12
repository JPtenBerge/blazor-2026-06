using DemoProject.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace DemoProject.Shared.Auth;

public class BobMensenMetHRequirement : IAuthorizationRequirement
{
}

public class BobMensenMetHHandler : AuthorizationHandler<BobMensenMetHRequirement, Person>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BobMensenMetHRequirement requirement,
        Person resource)
    {
        if (context.User.Identity?.Name == "Bob Smith" && resource.Name.StartsWith("H"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
