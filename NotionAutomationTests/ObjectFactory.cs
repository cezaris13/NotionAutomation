using System.Net;
using System.Text;
using System.Text.Json;
using NotionAutomation.DataTypes;
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
                DateCondition = DateCondition.OnOrBefore,
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

    public static NotionDatabaseRuleObject CreateNotionDatabaseRuleObject() {
        return new NotionDatabaseRuleObject {
            StartingState = "Completed",
            EndingState = "InProgress"
        };
    }

    public static List<TaskObject> CreateTaskObject(int size) {
        var taskObjects = new List<TaskObject>();

        for (var i = 0; i < size; i++) {
            taskObjects.Add(new TaskObject {
                    Id = Guid.NewGuid(),
                    Properties = new PropertyObject {
                        Status = new Status {
                            Select = new Select {
                                Name = "Status"
                            }
                        }
                    }
                }
            );
        }

        return taskObjects;
    }

    public static HttpResponseMessage CreateResponse(HttpStatusCode statusCode = HttpStatusCode.OK,
        object content = null) {
        return new HttpResponseMessage {
            StatusCode = statusCode,
            Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
        };
    }
}