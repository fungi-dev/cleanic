namespace Cleanic.Application
{
    using Cleanic.Core;
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IViewStore
    {
        Task<AggregateView> Load(AggregateViewInfo aggregateViewInfo, String aggregateId);
        Task<AggregateView> Load(AggregateViewInfo aggregateViewInfo, Expression<Func<AggregateView, Boolean>> filterExpression);
        Task Save(AggregateView aggregateView);
    }
}