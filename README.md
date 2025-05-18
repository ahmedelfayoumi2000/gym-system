
# GymSystem 🏋️‍♂️

*An advanced gym management solution built with ASP.NET Core to streamline gym operations for Admins, Trainers, Receptionists, and Members.*

## 📖 Overview

GymSystem is a robust and scalable RESTful API designed to manage gym operations efficiently. It caters to various user roles, including Admins, Trainers, Receptionists, and Members, providing seamless management of gym-related activities such as member registration, workout and nutrition plans, subscriptions, scheduling, and financial reporting. The system leverages modern technologies and architectural patterns to ensure performance, security, and maintainability.

This project was developed as a graduation project to showcase expertise in building enterprise-grade applications using ASP.NET Core, Clean Architecture, and a variety of third-party integrations like Gemini API, Azure Key Vault, and Cloudinary.

---

## ✨ Key Features

### Core Functionalities
- **Member Management:** Register, update, and manage members with role-based access (Admins, Trainers, Receptionists, Members).
- **Workout & Nutrition Plans:** AI-generated workout and nutrition plans using Gemini API, with validation for accuracy.
- **Subscriptions & Scheduling:** Manage gym subscriptions, classes, schedules, and track member attendance.
- **Financial Reporting:** Generate detailed financial reports for Admins, including payment tracking and drawer closing.

### Advanced Features
- **AI Integration:** Utilize Gemini API to generate personalized workout and nutrition plans for members.
- **Performance Optimization:** Implement Redis caching for faster data retrieval and Serilog for structured logging.
- **Security:** Secure API keys with Azure Key Vault, implement Refresh Tokens for authentication, and use HTTPS for data encryption.
- **Additional Features:** Support pagination, filtering, sorting, Soft Delete, BMI calculation, notifications, and image uploads via Cloudinary.

---

## 🛠️ Technologies Used

- **Backend:** ASP.NET Core 8 (RESTful API)
- **Database:** Entity Framework Core (SQL Server)
- **Architecture Patterns:** Clean Architecture, Repository Pattern, Unit of Work, Specification Pattern
- **Mapping:** AutoMapper (for object-object mapping)
- **Caching:** Redis (for performance optimization)
- **Logging:** Serilog (structured logging)
- **Security:** Azure Key Vault (API key management), JWT with Refresh Tokens (authentication)
- **Third-Party Integrations:**
  - Gemini API (AI-generated workout/nutrition plans)
  - Cloudinary (image uploads)
  - SMTP (email notifications)
- **Development Tools:** Visual Studio 2022, Postman (API testing)

---

## 📂 Project Structure

The project follows Clean Architecture principles to ensure separation of concerns, scalability, and maintainability. The solution is organized into the following layers:

```
GymSystem/
├── GymSystem.API/                  # API layer (Controllers, Configurations)
│   ├── Controllers/               # API endpoints for users, payments, subscriptions, etc.
│   ├── Program.cs                 # Entry point and middleware configurations
│   └── appsettings.json           # Configuration settings (e.g., connection strings, API keys)
├── GymSystem.BLL/                  # Business Logic Layer (Services)
│   ├── Services/                  # Business logic for accounts, payments, subscriptions, etc.
│   └── DTOs/                      # Data Transfer Objects for API communication
├── GymSystem.DAL/                  # Data Access Layer (Repositories, DbContext)
│   ├── Repositories/              # Repository implementations for data access
│   ├── Data/                      # Database context (GymSystemDbContext) and migrations
│   └── Models/                    # Entity models (User, Subscription, Payment, etc.)
├── GymSystem.Common/               # Shared utilities and constants
└── GymSystem.Tests/                # Unit tests for services and repositories
```

---

## 🚀 Getting Started

Follow these steps to set up and run the project locally.

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Redis](https://redis.io/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (or any compatible IDE)
- API keys for:
  - Gemini API
  - Azure Key Vault
  - Cloudinary
  - SMTP server (for email notifications)

### Installation
1. **Clone the Repository:**
   ```bash
   git clone https://github.com/ahmedelfayoumi2000/gym-system.git
   cd gym-system
   git checkout develop
   ```

2. **Set Up the Database:**
   - Update the connection string in `GymSystem.API/appsettings.json`:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server;Database=GymSystemDb;Trusted_Connection=True;"
     }
     ```
   - Apply migrations to create the database:
     ```bash
     cd GymSystem.DAL
     dotnet ef database update
     ```

3. **Configure Environment Variables:**
   - Update `appsettings.json` with your API keys:
     ```json
     "GeminiApi": {
       "ApiKey": "your_gemini_api_key"
     },
     "Cloudinary": {
       "CloudName": "your_cloud_name",
       "ApiKey": "your_api_key",
       "ApiSecret": "your_api_secret"
     },
     "Smtp": {
       "Host": "smtp.your-email-provider.com",
       "Port": 587,
       "Username": "your_email",
       "Password": "your_password"
     }
     ```
   - Configure Redis connection:
     ```json
     "Redis": {
       "ConnectionString": "localhost:6379"
     }
     ```

4. **Run the Application:**
   - Start Redis server locally (or use a hosted instance).
   - Run the API:
     ```bash
     cd GymSystem.API
     dotnet run
     ```
   - The API will be available at `https://localhost:5001` (or the port specified in `appsettings.json`).

5. **Test the API:**
   - Use Postman or Swagger to test the endpoints.
   - Swagger documentation is available at `https://localhost:5001/swagger`.

---

## 🛠️ API Endpoints

Below is the complete list of API endpoints provided by GymSystem. All endpoints require authentication (JWT Bearer token) unless specified. For interactive documentation, refer to the Swagger UI at `/swagger`.

### Authentication (AccountController)
- **POST /api/auth/register**  
  Register a new user (Admin, Trainer, Receptionist, or Member).  
  **Body:**  
  ```json
  {
    "email": "user@example.com",
    "password": "Password123!",
    "role": "Member",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "1234567890"
  }
  ```

- **POST /api/auth/login**  
  Log in and receive a JWT token with Refresh Token.  
  **Body:**  
  ```json
  {
    "email": "user@example.com",
    "password": "Password123!"
  }
  ```

- **POST /api/auth/refresh-token**  
  Refresh an existing JWT token using a refresh token.  
  **Body:**  
  ```json
  {
    "token": "your_jwt_token",
    "refreshToken": "your_refresh_token"
  }
  ```

- **GET /api/auth/user/{id}**  
  Retrieve user details by ID (Admins or the user themselves).  
  **Path Parameter:** `id` (e.g., `1`)

- **PUT /api/auth/user/{id}**  
  Update user details (Admins or the user themselves).  
  **Path Parameter:** `id` (e.g., `1`)  
  **Body:**  
  ```json
  {
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "phoneNumber": "1234567890"
  }
  ```

### Members (UserController)
- **GET /api/users**  
  Retrieve a paginated list of members (Admins only).  
  **Query Parameters:**  
  - `page` (e.g., `1`)  
  - `pageSize` (e.g., `10`)  
  - `sortBy` (e.g., `firstName`)  
  - `filter` (e.g., `role:Member`)

- **GET /api/users/{id}**  
  Retrieve a specific member by ID (Admins only).  
  **Path Parameter:** `id` (e.g., `1`)

- **POST /api/users**  
  Add a new member (Admins only).  
  **Body:**  
  ```json
  {
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "phoneNumber": "1234567890",
    "role": "Member"
  }
  ```

- **PUT /api/users/{id}**  
  Update a member's details (Admins only).  
  **Path Parameter:** `id` (e.g., `1`)  
  **Body:**  
  ```json
  {
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "phoneNumber": "1234567890"
  }
  ```

- **DELETE /api/users/{id}**  
  Soft delete a member (Admins only).  
  **Path Parameter:** `id` (e.g., `1`)

### Subscriptions (SubscriptionController)
- **GET /api/subscriptions**  
  Retrieve a paginated list of subscriptions (Admins/Receptionists).  
  **Query Parameters:**  
  - `page` (e.g., `1`)  
  - `pageSize` (e.g., `10`)  
  - `sortBy` (e.g., `startDate`)  
  - `filter` (e.g., `memberId:1`)

- **GET /api/subscriptions/{id}**  
  Retrieve a specific subscription by ID (Admins/Receptionists).  
  **Path Parameter:** `id` (e.g., `1`)

- **POST /api/subscriptions**  
  Create a new subscription for a member (Admins/Receptionists).  
  **Body:**  
  ```json
  {
    "memberId": 1,
    "startDate": "2025-05-18",
    "endDate": "2025-06-18",
    "planType": "Monthly",
    "price": 150.00
  }
  ```

- **PUT /api/subscriptions/{id}**  
  Update a subscription's details (Admins/Receptionists).  
  **Path Parameter:** `id` (e.g., `1`)  
  **Body:**  
  ```json
  {
    "startDate": "2025-05-18",
    "endDate": "2025-07-18",
    "planType": "Monthly",
    "price": 150.00
  }
  ```

- **DELETE /api/subscriptions/{id}**  
  Soft delete a subscription (Admins/Receptionists).  
  **Path Parameter:** `id` (e.g., `1`)

### Payments (PaymentController)
- **GET /api/payments**  
  Retrieve a paginated list of payments (Admins only).  
  **Query Parameters:**  
  - `page` (e.g., `1`)  
  - `pageSize` (e.g., `10`)  
  - `sortBy` (e.g., `paymentDate`)  
  - `filter` (e.g., `memberId:1`)

- **POST /api/payments**  
  Add a new payment (Admins/Receptionists).  
  **Body:**  
  ```json
  {
    "memberId": 1,
    "amount": 150.00,
    "paymentDate": "2025-05-18",
    "paymentMethod": "Cash"
  }
  ```

- **POST /api/payments/close-drawer**  
  Close the drawer and generate a financial report (Admins only).  
  **Body:**  
  ```json
  {
    "startDate": "2025-05-01",
    "endDate": "2025-05-18"
  }
  ```

- **DELETE /api/payments/{id}**  
  Soft delete a payment (Admins only).  
  **Path Parameter:** `id` (e.g., `1`)

### Attendance (AttendanceController)
- **GET /api/attendance**  
  Retrieve a paginated list of attendance records (Admins/Trainers).  
  **Query Parameters:**  
  - `page` (e.g., `1`)  
  - `pageSize` (e.g., `10`)  
  - `sortBy` (e.g., `checkInTime`)  
  - `filter` (e.g., `memberId:1`)

- **POST /api/attendance**  
  Record a member's attendance (Admins/Trainers/Receptionists).  
  **Body:**  
  ```json
  {
    "memberId": 1,
    "checkInTime": "2025-05-18T09:00:00",
    "checkOutTime": "2025-05-18T10:00:00"
  }
  ```

### Workout Plans (WorkoutPlanController)
- **POST /api/workout-plans**  
  Generate and save a new AI-generated workout plan for a member (Trainers).  
  **Body:**  
  ```json
  {
    "memberId": 1,
    "planDetails": "3 sets of 10 push-ups, 2 sets of 15 squats",
    "generatedByAI": true
  }
  ```

- **GET /api/workout-plans**  
  Retrieve a paginated list of workout plans (Trainers/Members).  
  **Query Parameters:**  
  - `page` (e.g., `1`)  
  - `pageSize` (e.g., `10`)  
  - `sortBy` (e.g., `createdAt`)  
  - `filter` (e.g., `memberId:1`)

### Nutrition Plans (NutritionPlanController)
- **POST /api/nutrition-plans**  
  Generate and save a new AI-generated nutrition plan for a member (Trainers).  
  **Body:**  
  ```json
  {
    "memberId": 1,
    "planDetails": "Breakfast: Oatmeal with berries, Lunch: Grilled chicken with salad",
    "generatedByAI": true
  }
  ```

- **GET /api/nutrition-plans**  
  Retrieve a paginated list of nutrition plans (Trainers/Members).  
  **Query Parameters:**  
  - `page` (e.g., `1`)  
  - `pageSize` (e.g., `10`)  
  - `sortBy` (e.g., `createdAt`)  
  - `filter` (e.g., `memberId:1`)

For a full list of endpoints, refer to the Swagger documentation at `/swagger`.

---

## ⚙️ Implementation Details

### Clean Architecture
- The project adheres to Clean Architecture principles, separating concerns into layers (API, BLL, DAL) to ensure scalability and testability.
- **Repository Pattern:** Used for data access to abstract database operations.
- **Unit of Work:** Manages transactions and ensures data consistency across repositories.
- **Specification Pattern:** Enables dynamic query building for filtering and sorting.

### Performance Optimization
- **Redis Caching:** Frequently accessed data (e.g., member profiles, schedules) is cached using Redis to reduce database load.
- **Query Optimization:** Entity Framework Core queries are optimized using eager loading and projections.

### Security
- **Authentication:** JWT with Refresh Tokens ensures secure and stateless authentication.
- **Authorization:** Role-based access control (RBAC) restricts endpoints based on user roles.
- **Data Protection:** API keys are stored securely in Azure Key Vault, and sensitive data (e.g., passwords) is hashed using BCrypt.

### Third-Party Integrations
- **Gemini API:** Used to generate AI-driven workout and nutrition plans, validated for accuracy before being assigned to members.
- **Cloudinary:** Handles image uploads for member profiles and gym assets.
- **SMTP:** Sends email notifications for subscription renewals and payment confirmations.

---


## 📈 Future Enhancements

- **Mobile App:** Develop a mobile app for members to access schedules and plans on the go.
- **Real-Time Notifications:** Implement WebSocket for real-time updates (e.g., class schedule changes).
- **Analytics Dashboard:** Add a dashboard for Admins with advanced analytics using Power BI integration.
- **Multi-Gym Support:** Extend the system to support multiple gym branches with centralized management.

---

## 🤝 Contributing

Contributions are welcome! To contribute:
1. Fork the repository.
2. Create a new branch (`git checkout -b feature/your-feature`).
3. Commit your changes (`git commit -m "Add your feature"`).
4. Push to the branch (`git push origin feature/your-feature`).
5. Open a Pull Request.

Please ensure your code follows the project's coding standards and includes tests for new features.

---

## 📜 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

## 📧 Contact

For inquiries or feedback, reach out to me at:  
- **Email:** ahmedelfayoumi2003@gmail.com
- **GitHub:** [ahmedelfayoumi2000](https://github.com/ahmedelfayoumi2000)  
- **LinkedIn:** [Ahmed Elfayoumi](https://www.linkedin.com/in/ahmed-elfayoumi)

---



*Built with 💡 and ☕ by Ahmed Elfayoumi*

---
