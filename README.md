# File archiving system

The file archive system is an filestorage and archiving system built for a university use case. Files of various types and sizes can be uploaded and archived while restricting access based on users levels and code. The app was built using C# and .Net Framework, Microsoft SQL Database and EntityFramework ORM.

#### App Features

> - Authentication and password recovery
> - File upload,copying , moving and deleting.
> - Folder creation, copying, moving and deleting.
> - One Time passwords for file and folder access
> - Restriction of folder and files access based on depts and accesslevel
> - Adding , deleting and updating Departments.
> - Adding, deleting and updating Faculties.
> - Adding deleting and updating employee profile
> - Adding and removing employees from access levels and roles.
> - Locking users out after the user has being inactive for the number of minutes specified.

#### installation Guide

- On the root directory of the WebUI Project rename the **apikeysDemo.config** file to **apikeys.config**
- You'll need a sendgrid api key so if you do not already have one , go to [send grid website](https://sendgrid.com/docs/ui/account-and-settings/api-keys/#creating-an-api-key) and follow the instructions to get one.
- In the _apikeys.config_ file add your sendgrid api key as the value to the _apikey_ variable.
- Add the email attached to your sendgrid account as the value for the _userEmail_ variable.
- Add your sendgrid Username as the value for the _user_ variable.
- Add the User inactivity lock out time by changing the value of the _LockOutTime_ to your prefered lockout time in minutes.
- Proceed to add corresponding values for admin info, please use a functional email for the admin mail.
