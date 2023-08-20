using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NotionTaskAutomation.Extensions;
using NotionTaskAutomation.Objects;

namespace NotionTaskAutomation;

public class FilterFactory : IFilterFactory
{
    private readonly Guid m_toDoListId;
    private readonly Guid m_collectionId;

    public FilterFactory(IConfiguration configuration)
    {
        m_toDoListId = configuration.GetValue<Guid>("todoListId");
        m_collectionId = configuration.GetValue<Guid>("collectionId");
    }

    public FilterObject CreateTodoTomorrowFilter(Guid spaceId) =>
        new()
        {
            Source = CreateSource(spaceId),
            CollectionView = CreateCollectionView(spaceId),
            Loader = new Loader
            {
                Type = "reducer",
                Filter = new FiltersList
                {
                    FiltersRec = new List<FiltersList>
                    {
                        new()
                        {
                            Filters = new List<FilterObjectWithProperty>
                            {
                                new()
                                {
                                    FilterObject = new OneFilterObject
                                    {
                                        Value = new Value
                                        {
                                            Type = "exact",
                                            ValueString = States.TodoTomorrow.ToDescriptionString()
                                        },
                                        Operator = "enum_is"
                                    },
                                    Property = "^OE@"
                                },
                                new()
                                {
                                    FilterObject = new OneFilterObject
                                    {
                                        Value = new Value
                                        {
                                            Type = "relative",
                                            ValueString = "today"
                                        },
                                        Operator = "date_is"
                                    },
                                    Property = "cZ:C"
                                }
                            },
                            Operator = "and"
                        }
                    },
                    Operator = "and"
                },
                UserTimeZone = "Europe/Vilnius",
                Reducers = CreateReducers()
            }
        };

    public FilterObject CreateTodoFilter(Guid spaceId) =>
        new()
        {
            Source = CreateSource(spaceId),
            CollectionView = CreateCollectionView(spaceId),
            Loader = new Loader
            {
                Type = "reducer",
                Filter = new FiltersList
                {
                    Filters = new List<FilterObjectWithProperty>
                    {
                        new()
                        {
                            FilterObject = new OneFilterObject
                            {
                                Value = new Value
                                {
                                    Type = "exact",
                                    ValueString = States.Todo.ToDescriptionString()
                                },
                                Operator = "enum_is"
                            },
                            Property = "^OE@"
                        },
                        new()
                        {
                            FilterObject = new OneFilterObject
                            {
                                Value = new Value
                                {
                                    Type = "relative",
                                    ValueString = "tomorrow"
                                },
                                Operator = "date_is"
                            },
                            Property = "cZ:C"
                        }
                    },
                    Operator = "and"
                },
                UserTimeZone = "Europe/Vilnius",
                Reducers = CreateReducers()
            }
        };

    public FilterObject CreateEventFilter(Guid spaceId) =>
        new()
        {
            Source = CreateSource(spaceId),
            CollectionView = CreateCollectionView(spaceId),
            Loader = new Loader
            {
                Type = "reducer",
                Filter = new FiltersList
                {
                    Filters = new List<FilterObjectWithProperty>
                    {
                        new()
                        {
                            FilterObject = new OneFilterObject
                            {
                                Value = new Value
                                {
                                    Type = "exact",
                                    ValueString = States.Event.ToDescriptionString()
                                },
                                Operator = "enum_is"
                            },
                            Property = "^OE@"
                        },
                        new()
                        {
                            FilterObject = new OneFilterObject
                            {
                                Value = new Value
                                {
                                    Type = "relative",
                                    ValueString = "today"
                                },
                                Operator = "date_is_before"
                            },
                            Property = "cZ:C"
                        }
                    },
                    Operator = "and"
                },
                UserTimeZone = "Europe/Vilnius",
                Reducers = CreateReducers()
            }
        };

    private Reducers CreateReducers() =>
        new()
        {
            Results = new Results
            {
                Type = "results",
                Limit = 1000
            },
            Total = new Total
            {
                Type = "aggregation",
                Aggregation = new Aggregation
                {
                    Aggregator = "count"
                }
            }
        };

    private CollectionView CreateCollectionView(Guid spaceId) =>
        new()
        {
            Id = m_toDoListId,
            SpaceId = spaceId,
        };

    private Source CreateSource(Guid spaceId) =>
        new()
        {
            Type = "collection",
            Id = m_collectionId,
            SpaceId = spaceId,
        };
}