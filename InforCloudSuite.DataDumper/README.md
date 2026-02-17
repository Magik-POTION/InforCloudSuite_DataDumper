<h1 align="center">PrepGlobal Syteline Service</h1>

## Authorization
- Log into Infor CloudSuite
- Select Applications
- Select OS
- Select Security
- Select Manage
- Select Users
- Create / search for a user
- Under security roles the user must have the following roles:

IONAPI-Administrator
IONAPI-User

## Generating API Key
- Log into Infor Cloudsuite
- Make note of configurations
- Select Applications
- Select OS
- Select API Gateway
- Select Authorized Apps
- Create New Authorized App
- Select a User with IONAPI-Admin & IONAPI-User roles
- Download the credential file and place in the root of the project
- Open the file in a text editor and add the following variable

```json
"cf": "Your configuration name here"
```

## Finding API endpoints and documentations
- Log into Infor Cloudsuite
- Select Applications
- Select OS
- Select API Gateway
- Select Available APIs
- This application uses REST v2

## How to use
- Run the program
- Input IDO name
- Input Order By Column name if necessary
- Input last bookmark value if continuing a processes over time
- Data will written / appended to a data.csv file