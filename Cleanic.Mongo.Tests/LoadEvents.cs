using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;
using Xunit;

namespace Cleanic.Framework.Tests
{
    [FeatureDescription(
        @"Как репозиторий,
        чтобы отдать агрегат в текущем состоянии,
        хочу загружать события агрегата из базы данных.")]
    [Collection("Sequential")]
    public partial class LoadEvents
    {
        [Scenario]
        public async Task SimplestLoadingById()
        {
            await Runner
                .AddStep("Когда в базе лежит одно событие для агрегата.", async _ => await When_there_is_aggregate_with_one_event())
                .AddStep("Репозиторий его может загрузить по идентификатору.", async _ => await Then_repo_can_load_it_by_id())
                .RunAsync();
        }

        [Scenario]
        public async Task SimplestLoadingByEventsList()
        {
            await Runner
                .AddStep("Когда в базе лежит одно событие для агрегата.", async _ => await When_there_is_aggregate_with_one_event())
                .AddStep("Репозиторий его может загрузить, указав мету для этого события.", async _ => await Then_repo_can_load_it_by_event_meta())
                .RunAsync();
        }
    }
}