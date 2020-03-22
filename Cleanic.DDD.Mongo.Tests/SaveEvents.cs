using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;
using Xunit;

namespace Cleanic.Framework.Tests
{
    [FeatureDescription(
        @"Как репозиторий,
        чтобы зафиксировать агрегат в текущем состоянии,
        хочу сохранять события агрегата в базе данных.")]
    [Collection("Sequential")]
    public partial class SaveEvents
    {
        [Scenario]
        public async Task SimplestSaving()
        {
            await Runner
                .AddStep("Допустим есть одно событие для, ни разу не сохранённого ещё в базе, агрегата.", _ => Given_there_is_aggregate_with_one_event())
                .AddStep("После того, как репозиторий просит сохранить это событие", async _ => await When_repostory_save_first_event())
                .AddStep("в базе появляется коллекция событий этого агрегата и там есть один документ.", async _ => await Then_db_has_collection_with_one_document())
                .RunAsync();
        }

        [Scenario]
        public async Task OptimisticLocking()
        {
            await Runner
                .AddStep("Если репозиторий не верно указал количество уже существующих в базе событий,", async _ => await When_repostory_gives_wrong_events_count())
                .AddStep("он не может сохранить новое.", async _ => await Then_he_cant_save_new_one())
                .RunAsync();
        }
    }
}