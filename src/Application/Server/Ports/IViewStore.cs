namespace Cleanic.Application
{
    using Cleanic.Core;
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IViewStore
    {
        Task<View> Load(ViewInfo viewInfo, String entityId);
        Task<View[]> Load(ViewInfo viewInfo, Expression<Func<View, Boolean>> filterExpression);
        Task Save(View view);
    }
}