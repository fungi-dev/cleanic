using System;
using System.Collections.Generic;
using System.Linq;

namespace Cleanic.Application
{
    public class Authorization
    {
        public virtual void IncorporateGrantsFromOpenIdConnectClaims(String userId, String userGrantClaim)
        {
            if (String.IsNullOrWhiteSpace(userGrantClaim)) throw new ArgumentNullException(nameof(userGrantClaim));

            if (!_grants.TryGetValue(userId, out var userGrants)) _grants.Add(userId, userGrants = new List<Grant>());

            foreach (var aggregate in userGrantClaim.Split('|'))
            {
                var aggregateGrants = aggregate.Split(':');
                var grant = new Grant { AggregateType = aggregateGrants[0] };

                if (aggregateGrants.Length > 1 && aggregateGrants[1].Length > 0)
                {
                    grant.ActionTypes = aggregateGrants[1].Split('&').ToList();
                }

                if (aggregateGrants.Length > 2 && aggregateGrants[2].Length > 0)
                {
                    grant.AggregateIds = aggregateGrants[2].Split('&').ToList();
                }

                userGrants.Add(grant);
            }
        }

        public virtual Boolean IsAllowed(String userId, ActionInfo actionInfo, String aggregateId)
        {
            if (!_grants.TryGetValue(userId, out var userGrants)) return false;
            var aggGrant = userGrants.SingleOrDefault(x => String.Equals(x.AggregateType, actionInfo.Aggregate.Name, StringComparison.OrdinalIgnoreCase));
            if (aggGrant == null) return false;
            if (aggGrant.AggregateIds.Any())
            {
                if (!aggGrant.AggregateIds.Any(x => String.Equals(x, aggregateId, StringComparison.OrdinalIgnoreCase))) return false;
            }
            if (aggGrant.ActionTypes.Any())
            {
                if (!aggGrant.ActionTypes.Any(x => String.Equals(x, actionInfo.Name, StringComparison.OrdinalIgnoreCase))) return false;
            }

            return true;
        }

        public virtual Boolean IsAllowed(String userId, AggregateInfo aggregateInfo, String aggregateId)
        {
            if (!_grants.TryGetValue(userId, out var userGrants)) return false;
            var aggGrant = userGrants.SingleOrDefault(x => String.Equals(x.AggregateType, aggregateInfo.Name, StringComparison.OrdinalIgnoreCase));
            if (aggGrant == null) return false;
            if (aggGrant.AggregateIds.Any())
            {
                if (!aggGrant.AggregateIds.Any(x => String.Equals(x, aggregateId, StringComparison.OrdinalIgnoreCase))) return false;
            }

            return true;
        }

        private readonly Dictionary<String, List<Grant>> _grants = new Dictionary<String, List<Grant>>();

        private class Grant
        {
            public String AggregateType { get; set; }
            public List<String> AggregateIds { get; set; } = new List<String>();
            public List<String> ActionTypes { get; set; } = new List<String>();
        }
    }
}