using System;
using NotionAutomationButtonAutomation.Objects;

namespace NotionAutomationButtonAutomation
{
    public interface IFilterFactory
    {
        FilterObject CreateTodoTomorrowFilter(Guid spaceId);
        FilterObject CreateTodoFilter(Guid spaceId);
        FilterObject CreateEventFilter(Guid spaceId);
    }
}