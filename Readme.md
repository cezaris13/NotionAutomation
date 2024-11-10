# Notion Automation backend

Automatic kanban board task moving based on the rules.

## Requirements:
First, you have to retrieve bearer token from notion (as it is used for authentication in this backend). You can do it here: https://www.notion.so/profile/integrations

## Preview:
![Swagger endpoints preview](/Images/Swagger.png)

to run it create `updateNotion.sh`
and add these lines:
```
curl -X 'Post' \
  'https://notionautomation-ofkv.onrender.com/api/updateNotionDatabases' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer SOME_BEARER_TOKEN'
```

You can test it out here: https://notionautomation-ofkv.onrender.com/swagger/index.html