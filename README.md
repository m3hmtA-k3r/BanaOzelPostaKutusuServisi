# PostaKutusuServisi

A web-based messaging application built with ASP.NET Core 8 MVC and ASP.NET Core Identity.

Users register, verify their email address, and exchange messages with each other. The application covers the full lifecycle of a message: inbox, sent items, drafts, starred messages, trash, and reporting.

This project is based on a case study from the training I received at M&Y Yazılım Eğitim Akademi, under the guidance of Murat Yücedağ and Erhan Gündüz. Rather than following the reference project line by line, I rebuilt it from scratch by reading and understanding it first, then extended it with features that were not part of the original.

<img width="1808" height="850" alt="image" src="https://github.com/user-attachments/assets/a6ed0f7d-f9fe-41bc-acb1-ab0c53430f69" />

## Features

### Authentication and Account

- Registration with unique email validation
- Login with either username or email address
- Email confirmation, with the ability to resend the confirmation link
- Forgot password and password reset over a token-based link
- Account lockout after five failed login attempts, for fifteen minutes
- Custom Identity error messages through a custom error describer
- Profile management: name, surname, profile photo
- Password change from within the profile page

### Messaging

- Inbox, sent items, drafts, starred messages, and trash
- Compose, reply, and forward
- Save a message as a draft, edit it later, or delete it
- Mark messages as read, with unread messages highlighted
- Unread message count shown as a badge in the sidebar
- Star and unstar messages
- Two-sided soft delete: a message is only removed from the database view of the user who deleted it, and the other side keeps their copy
- Restore messages from trash
- Search, filter, and pagination across all message lists
- Personal categories, so users can organise their own messages
- Report a message, with a reason and an optional description

### Administration

- Role-based authorization with Admin and User roles, seeded at application startup
- Dashboard with system statistics
- User management: list all users, activate or deactivate an account, grant or revoke admin rights
- Report management: review reported messages and mark them as resolved

### Email Delivery

- Two interchangeable implementations behind a single interface
- SmtpMailService sends real email over SMTP
- FileMailService writes email to disk instead of sending it, which makes local development possible without an SMTP server

## Tech Stack

- Framework: ASP.NET Core 8 MVC
- Authentication: ASP.NET Core Identity with an integer primary key
- Database: SQL Server + Entity Framework Core 8
- Database Approach: Code First + Migrations
- UI: Tailwind CSS v4, compiled through the Tailwind CLI as part of the build
- Security: global anti-forgery validation applied to every POST request

## Architecture

The interface is built from ViewComponents rather than large monolithic views, so each part of the page loads its own data and can be reused across pages.

- Layout components handle the header, the sidebar, and the user avatar
- Message components handle the list, the empty state, the page title, the filter bar, and pagination
- The mail service is injected through an interface, so the implementation can be swapped between file output and real SMTP without touching the calling code
- Message ownership is verified inside the controller before a message is displayed, so a user cannot read another user's message by changing the id in the URL

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (Express or LocalDB)
- Node.js, required by the Tailwind CLI during the build

### Setup

```bash
git clone https://github.com/m3hmtA-k3r/BanaOzelPostaKutusuServisi.git
cd BanaOzelPostaKutusuServisi/PostaKutusuServisi
npm install
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=.\SQLEXPRESS;database=PostaKutusuDb;integrated security=true;trustServerCertificate=true"
dotnet ef database update
dotnet run
```

`dotnet build` also builds the Tailwind stylesheet, which is why Node.js is required.

### Email delivery

`Program.cs` decides which mail service is used:

- __`FileMailService`:__ writes every email to the `sent-emails` folder. No SMTP server is needed, which makes it the easiest choice for local development.
- __`SmtpMailService`:__ sends real email. Set the SMTP values with User Secrets, never in `appsettings.json`:

```bash
dotnet user-secrets set "Smtp:User" "you@example.com"
dotnet user-secrets set "Smtp:Password" "<app password>"
dotnet user-secrets set "Smtp:From" "you@example.com"
```

### First administrator

Register a normal account through the site, then set its email address as `AdminEmail`:

```bash
dotnet user-secrets set "AdminEmail" "you@example.com"
```

On the next start, that account is given the Admin role. After that, other admins can be appointed from the admin panel.

## What I learned

- __Hiding a link is not authorization.__ In the reference project, the message detail page loaded a message by its id alone. Anyone could read another user's message by changing the number in the URL. In my version, the controller checks that the signed-in user is the sender or the receiver before the message is shown.
- __Deleting shared data needs two owners.__ A message belongs to two people. If the sender deletes it, the receiver should still have it. I added a separate deleted flag for each side, and the message disappears only from the view of the person who deleted it.
- __Secrets do not belong in configuration files.__ An early version kept the SMTP password in `appsettings.json`. Before publishing, I revoked that password and moved all credentials to User Secrets. The repository now only shows the shape of the settings.

## Status

Complete. All 38 items of the use case document are implemented.

## Acknowledgements

Thanks to Murat Yücedağ and Erhan Gündüz at M&Y Yazılım Eğitim Akademi for the training and the case study this project grew out of.

## About me

Mehmet Asker, a self-taught full stack developer with a background in operations management. More projects: [github.com/m3hmtA-k3r](https://github.com/m3hmtA-k3r)
