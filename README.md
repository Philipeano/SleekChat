# SleekChat

[![Maintainability](https://api.codeclimate.com/v1/badges/c533f0a887566fb5e6ea/maintainability)](https://codeclimate.com/github/Philipeano/SleekChat/maintainability)
[![Test Coverage](https://api.codeclimate.com/v1/badges/c533f0a887566fb5e6ea/test_coverage)](https://codeclimate.com/github/Philipeano/SleekChat/test_coverage)

## Overview
SleekChat is a simple app that enables friends and colleagues create groups for messaging. This version of the app is a REST API that can be consumed with any HTTP client. The features are outlined below, while the full API documentation is available [here](https://sleekchat.azurewebsites.net/index.html).



## Main features
- New users can create accounts by signing up.
- Registered users can access the app's features by signing in.
- Authenticated users can create groups for messaging.
- Group owners/administrators can add other users to their groups.
- Group owners/administrators can remove a member from the group.
- A group member can exit the group.
- Members of a group can post messages for others to read.
- Members of a group receive notifications when a new message is posted.
- Message senders can update or delete their messages.



## Endpoints

Base URL for all endpoints: ```https://sleekchat.azurewebsites.net/api```
 
| Endpoint URL       | Method    | Functionality   |
| :----------------- | :------------- | :-------------- |
|**_USERS_**           |        |                               |
| /users               | ```GET```    | Fetch all registered users    |  
| /users/:userId       | ```GET```    | Fetch a specified user    |
| /users/register      | ```POST```   | Register a new user    |  
| /users/authenticate  | ```POST```   | Sign in a registered user  |  
| /users/:userId       | ```PUT```    | Update a specified user    |
| /users/:userId       | ```DELETE``` | Delete specified user    |
|**_GROUPS_**          |        |                               |
| /groups              | ```GET```    | Fetch all existing groups |  
| /groups/:groupId     | ```GET```    | Fetch a specified group |
| /groups              | ```POST```   | Create a new group |  
| /groups/:groupId     | ```PUT```    | Update a specified group |
| /groups/:groupId     | ```DELETE``` | Delete a specified group |
|**_MEMBERSHIPS_**                              |        |                               |
| /memberships                                  | ```GET```    | Fetch all memberships |  
| /memberships?memberId=:userId                 | ```GET```    | Fetch all memberships for the specified user |  
| /groups/:groupId/memberships                  | ```GET```    | Fetch all memberships for the specified group |
| /groups/:groupId/memberships                  | ```POST```   | Add a user to the specified group |  
| /groups/:groupId/memberships?memberId=:userId | ```DELETE``` | Remove a specified user from the group |
|**_MESSAGES_**                              |        |                               |
| /messages                                  | ```GET```    | Fetch all messages |  
| /messages?senderId=:userId                 | ```GET```    | Fetch all messages sent by the specified user |  
| /groups/:groupId/messages                  | ```GET```    | Fetch all messages posted in the specified group |
| /groups/:groupId/messages?senderId=:userId | ```GET```    | Fetch all messages posted in the group by this user |
| /groups/:groupId/messages                  | ```POST```   | Post a new message within the specified group |  
| /groups/:groupId/messages/:messageId       | ```PUT```    | Update a message previously sent to the group |
| /groups/:groupId/messages/:messageId       | ```DELETE``` |  Delete a message previously sent to the group |
|**_NOTIFICATIONS_**              |        |                               |
| /notifications                  | ```GET```    | Fetch all notifications |  
| /notifications/:notificationId  | ```GET```    | Fetch a specific notification |
| /notifications/:notificationId  | ```PATCH```    | Update a specific notification received by this user |
| /notifications/:notificationId  | ```DELETE``` | Delete a specific notification received by this user |



## Built with

- Primary language: ```C#``` 
- Server technology: ```ASP.Net Core```
- Database system: ```Microsoft SQL Server``` & ```Azure SQL Database```
- ORM: ```Entity Framework Core```
- API documentation: ```Swashbuckle.AspNetCore```
- Target Runtime: ```.Net Core 3.1.1```



## License
[MIT © Philip Newman.](https://opensource.org/licenses/MIT)
