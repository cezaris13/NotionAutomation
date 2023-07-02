using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NotionAutomationButtonAutomation.Extensions;
using NotionAutomationButtonAutomation.Objects;

namespace NotionAutomationButtonAutomation
{
    public class FilterFactory : IFilterFactory
    {
        private readonly Guid m_toDoListId;
        private readonly Guid m_collectionId;

        public FilterFactory(IConfiguration configuration)
        {
            m_toDoListId = configuration.GetValue<Guid>("todoListId");
            m_collectionId = configuration.GetValue<Guid>("collectionId");
        }

        public FilterObject CreateTodoTomorrowFilter(Guid spaceId)
        {
            return new FilterObject
            {
                Source = new Source
                {
                    Type = "collection",
                    Id = m_collectionId,
                    SpaceId = spaceId,
                },
                CollectionView = new CollectionView
                {
                    Id = m_toDoListId,
                    SpaceId = spaceId,
                },
                Loader = new Loader
                {
                    Type = "reducer",
                    Filter = new FiltersList
                    {
                        FiltersRec = new List<FiltersList>
                        {
                            new FiltersList
                            {
                                Filters = new List<FilterObjectWithProperty>
                                {
                                    new FilterObjectWithProperty
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
                                    new FilterObjectWithProperty
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
                    Reducers = new Reducers
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
                    }
                }
            };
        }

        public FilterObject CreateTodoFilter(Guid spaceId)
        {
            return new FilterObject
            {
                Source = new Source
                {
                    Type = "collection",
                    Id = m_collectionId,
                    SpaceId = spaceId,
                },
                CollectionView = new CollectionView
                {
                    Id = m_toDoListId,
                    SpaceId = spaceId,
                },
                Loader = new Loader
                {
                    Type = "reducer",
                    Filter = new FiltersList
                    {
                        FiltersRec = new List<FiltersList>
                        {
                            new FiltersList
                            {
                                Filters = new List<FilterObjectWithProperty>
                                {
                                    new FilterObjectWithProperty
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
                                    new FilterObjectWithProperty
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
                    Reducers = new Reducers
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
                    }
                }
            };
        }

        public FilterObject CreateEventFilter(Guid spaceId)
        {
            return new FilterObject
            {
                Source = new Source
                {
                    Type = "collection",
                    Id = m_collectionId,
                    SpaceId = spaceId,
                },
                CollectionView = new CollectionView
                {
                    Id = m_toDoListId,
                    SpaceId = spaceId,
                },
                Loader = new Loader
                {
                    Type = "reducer",
                    Filter = new FiltersList
                    {
                        FiltersRec = new List<FiltersList>
                        {
                            new FiltersList
                            {
                                Filters = new List<FilterObjectWithProperty>
                                {
                                    new FilterObjectWithProperty
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
                                    new FilterObjectWithProperty
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
                    Reducers = new Reducers
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
                    }
                }
            };
        }
    }
}