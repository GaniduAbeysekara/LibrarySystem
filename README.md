# Library Management System

This project is a library management system built with ASP.NET Core 6, utilizing Entity Framework Core for database access, SQL Server as the database provider, JWT for secure authentication and authorization, and BCrypt for password hashing.

## Table of Contents

- [Functionality Overview](#functionality-overview)
- [Technologies Used](#technologies-used)
- [Setup Instructions](#setup-instructions)
- [Usage](#usage)
- [API Documentation](#api-documentation)
- [Contributing](#contributing)
- [License](#license)

## Functionality Overview

### Super Admin:

- **Authentication**:

  - Login (JWT)
  - Edit own profile

- **User Management**:

  - Get users (super admin and other users)
  - Get profile details of other users
  - Add/register users
  - Soft delete users
  - Activate/inactivate users (lower priority)

- **Book Management**:
  - Add new books
  - Edit books (update ISBN is lower priority)
  - Delete books
  - Search/get books

### Users:

- **Authentication**:

  - Register
  - Login (JWT)
  - Edit own profile

- **Profile Management**:

  - Edit own profile details
  - Get own profile details

- **Book Management**:
  - Add new books
  - Edit books
  - Delete books
  - Search/get books

## Technologies Used

- **ASP.NET Core 6**: For building the web application and API.
- **Entity Framework Core**: ORM for database access.
- **SQL Server**: Database provider.
- **JWT (JSON Web Tokens)**: For secure authentication and authorization.

## Setup Instructions

1. **Clone the Project**: `git clone https://github.com/GaniduAbeysekara/LibrarySystem`
2. **Open Project in Visual Studio**: Open `.sln` file in Microsoft Visual Studio.
3. **Connect to SQL Server**:
   - Open SQL Server Management Studio.
   - Connect the SQL Management Studio.
   - Get the server name and update `appsettings.json`:
     before
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=LibrarySystemDatabase;Trusted_Connection=True;TrustServerCertificate=true"
     }
     ```
     after
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=SL099;Database=LibrarySystemDatabase;Trusted_Connection=True;TrustServerCertificate=true"
     }
     ```
4. **Database Migration**:
   - Open Package Manager Console in Visual Studio (`Tools > NuGet Package Manager > Package Manager Console`).
   - Run:
     ```bash
     Add-Migration [add a suitable migration name]
     Update-Database
     ```
5. **Run the Project**: Start the project in Visual Studio. Swagger UI will open automatically.
6. **Testing APIs**: Use Postman or any API testing tool to test the APIs.

## Usage

Describe how to use the project after setup. Include any specific instructions or examples.

## API Documentation

### Authentication:

- `POST /api/Auth/Register`: Register a new user.
- `POST /api/Auth/Login`: Login to get JWT token.
- `PUT /api/Auth/EditUser`: Edit user details.
- `DELETE /api/Auth/DeleteUser`: Delete a user account.

### Book Management:

- `GET /api/Book/searchebook`: Search and retrieve books.
- `POST /api/Book/createbook`: Add a new book.
- `DELETE /api/Book/deletebook/{isbn}`: Delete a book by ISBN.
- `PUT /api/Book/editbook/{isbn}`: Edit a book details.

### Additional APIs:

Include any additional APIs and their details here.

## Contributing

Contributions are welcome.

## License

This project is licensed under the MIT License.
