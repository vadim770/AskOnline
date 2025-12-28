# AskOnline

AskOnline is a feature-rich Q&A platform, similar to Stack Overflow, where users can ask questions, provide answers, and engage with a community of peers.

## Features

*   **Ask and Answer:** Users can post questions and provide answers to existing questions.
*   **Voting System:** Upvote or downvote questions and answers to highlight the most useful content.
*   **Tagging:** Organize questions with tags to improve discoverability.
*   **User Profiles:** View user activity and contributions.
*   **Search:** Full-text search for questions and answers.

## Tech Stack

*   **Backend:** C# with ASP.NET Core 9.0
*   **Frontend:** React (with Vite)
*   **Database:** Microsoft SQL Server
*   **ORM:** Entity Framework Core
*   **Authentication:** JWT (JSON Web Tokens)

## Prerequisites

Before you begin, ensure you have the following installed on your system:

*   [Git](https://git-scm.com/)
*   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
*   [Node.js and npm](https://nodejs.org/en/)
*   [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (SQL Server Express or the development edition are good free options. The project is pre-configured to work with LocalDB, which is included with Visual Studio).

## Installation

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/your-username/AskOnline.git
    cd AskOnline
    ```

2.  **Configure the Backend:**
    The backend project is located in the `AskOnline/` directory. The database connection string is configured in `AskOnline/AskOnline/appsettings.json`.

    By default, it uses SQL Server LocalDB:
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=(localdb)\MSSQLLocalDB;Database=QnADb;Trusted_Connection=True;"
    }
    ```
    If you are not using SQL Server LocalDB, update this string to point to your SQL Server instance.

3.  **Apply Database Migrations:**
    The database schema is managed by Entity Framework Core. To create the database and apply the schema, run the following command from the `AskOnline/AskOnline` directory:

    ```bash
    # Navigate to the C# project directory
    cd AskOnline/AskOnline

    # Apply EF Core migrations
    dotnet ef database update
    ```

4.  **Set up the Frontend:**
    The frontend React application is located in the `AskOnline/AskOnline/askonline-front` directory.

    ```bash
    # From the AskOnline/AskOnline directory, navigate to the frontend folder
    cd askonline-front

    # Install npm dependencies
    npm install
    ```

## Running the Application

You will need to run both the backend API and the frontend development server simultaneously.

1.  **Run the Backend API:**
    You can run the backend using the .NET CLI or Visual Studio.

    *   **.NET CLI:**
        Navigate to the `AskOnline/AskOnline` directory and run:
        ```bash
        dotnet run
        ```
        The API will start, typically on a port like `https://localhost:7123`. Check the console output for the exact URL.

    *   **Visual Studio:**
        Open the `AskOnline/AskOnline.sln` file in Visual Studio and press the "Run" button (or F5).

2.  **Run the Frontend:**
    In a separate terminal, navigate to the `AskOnline/AskOnline/askonline-front` directory and run:
    ```bash
    npm run dev
    ```
    The React development server will start, typically on `http://localhost:5173`. Open this URL in your browser to use the application.

## Contributing

Contributions are welcome! If you have suggestions for improvements, please feel free to open an issue or submit a pull request.

