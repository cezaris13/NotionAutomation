using NotionAutomation.Objects;

namespace NotionAutomationTests;

public static class ObjectFactory {
    public static StatesObject CreateStatesObject(List<string> states) {
        var options = states.Select(p => new Options { Name = p }).ToList();

        return new StatesObject {
            Properties = new Properties {
                Status = new StatusObject {
                    Select = new SelectObject {
                        Options = options
                    }
                }
            }
        };
    }

    public static List<NotionDatabaseRule> CreateNotionDatabaseRules(int size, Guid ruleId = new(),
        Guid databaseId = new()) {
        List<NotionDatabaseRule> rules = [];

        for (var i = 0; i < size; i++) {
            var tempRuleId = i == 0 ? ruleId : Guid.NewGuid();
            var tempDatabaseId = i == 0 ? databaseId : Guid.NewGuid();
            rules.Add(new NotionDatabaseRule {
                RuleId = tempRuleId,
                DatabaseId = tempDatabaseId,
                StartingState = "InProgress",
                EndingState = "Completed",
                OnDay = "Wednesday",
                DayOffset = 5
            });
        }

        return rules;
    }

    public static List<QueryObject> CreateQueryObjects(int size) {
        List<QueryObject> queryObjects = [];

        for (var i = 0; i < size; i++) {
            Guid? nextCursor = null;
            if (i < size - 1)
                nextCursor = Guid.NewGuid();

            queryObjects.Add(
                new QueryObject {
                    Results = Enumerable.Range(0, 2).Select(_ =>
                            new TaskObject {
                                Id = Guid.NewGuid(),
                                Properties = new PropertyObject {
                                    Status = new Status {
                                        Select = new Select {
                                            Name = "InProgress"
                                        }
                                    }
                                }
                            })
                        .ToList(),
                    NextCursor = nextCursor
                }
            );
        }

        return queryObjects;
    }
}