using System;
using NotionTaskAutomation.Objects;

namespace NotionTaskAutomation;

public interface IFilterFactory
{
    FilterObject CreateTodoTomorrowFilter(Guid spaceId);
    FilterObject CreateTodoFilter(Guid spaceId);
    FilterObject CreateEventFilter(Guid spaceId);
}