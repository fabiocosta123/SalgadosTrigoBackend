using Microsoft.AspNetCore.Authorization;

namespace SalgadosTrigo.Api.Authorization
{
    public static class RolePolicies
    {
        public const string Suppliers = "suppliers";
        public const string Costs = "costs";
        public const string Finance = "finance";
        public const string PaymentTerms = "payment-terms";


        public static void AddRolePolicies(this AuthorizationOptions options)
        {
            options.AddPolicy(Suppliers, AllowAllExceptSeller);
            options.AddPolicy(Costs, AllowAllExceptSeller);
            options.AddPolicy(Finance, AllowAllExceptSeller);
            options.AddPolicy(PaymentTerms, policy => policy.RequireRole("administrador"));
        }

        private static void AllowAllExceptSeller(AuthorizationPolicyBuilder policy)
        {
            policy.RequireRole("administrador", "gerente", "funcionario", "financeiro");
        }
    }
}
