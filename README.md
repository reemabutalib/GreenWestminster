# Green Westminster

Green Westminster is a full-stack web application designed to help students and staff at Westminster University track, improve, and celebrate their sustainability efforts on campus.

---

## Features

- 🌱 **User Registration & Login**
- 🗺️ **Interactive Campus Recycling Map**
- 🏆 **Sustainability Challenges & Activities**
- 📈 **Leaderboard & Points System**
- 👤 **Customizable Avatars with Unlockable Items**
- 🛡️ **Admin Dashboard for Activity Management**
- 📱 **Mobile-First Responsive Design**
- ♿ **Accessibility: Keyboard, Screen Reader, and Contrast Support**

---

## Tech Stack

- **Frontend:** React (Vite), React Router, CSS
- **Backend:** ASP.NET Core Web API
- **Database:** PostgreSQL
- **Authentication:** JWT
- **Other:** RESTful API, file uploads, role-based access

---

## Getting Started

### Prerequisites

- Node.js (v18+ recommended)
- .NET 8 SDK
- PostgreSQL

---

### 1. Clone the Repository

```bash
git clone https://github.com/reemabutalib/green-westminster.git
cd green-westminster
```

---

### 2. Setup the Backend

```bash
cd Server
dotnet restore
# Update appsettings.json with your PostgreSQL connection string
dotnet ef database update
dotnet run
```

The backend will run on `http://localhost:80` by default.

---

### 3. Setup the Frontend

```bash
cd Client
npm install
npm run dev
```

The frontend will run on `http://localhost:5173` by default.

---

### 4. Environment Variables

- **Backend:** Configure your PostgreSQL connection string in `Server/appsettings.json`.
- **Frontend:** If needed, set API URLs in `Client/.env`.

---

## API Endpoints

- `POST /api/auth/login` — User login
- `POST /api/auth/register` — User registration
- `GET /api/users/leaderboard` — Leaderboard data
- `GET /api/challenges` — List challenges
- `POST /api/activities/{id}/complete` — Complete an activity (with file upload)
- ...and more

---

## Testing Endpoints

Use [Postman](https://www.postman.com/) or similar tools.  
**Example:** To complete an activity with an image:

- **POST** `http://localhost:80/api/activities/2/complete`
- **Body:** `form-data` with keys: `UserId`, `CompletedAt`, `Notes`, `image` (file)

---

## Accessibility

- All interactive elements are keyboard accessible.
- Sufficient color contrast.
- Screen reader support with ARIA labels.

---

## Customization

- **Avatars:** Place your avatar images in `Client/public/avatars/` and items in `Client/public/avatars/items/`.
- **Campus Maps:** Place map images in `Client/public/maps/`.

---

## Contributing

1. Fork the repo
2. Create your feature branch (`git checkout -b feature/your-feature`)
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

---

## License

MIT

---

## Contact

For questions or support, contact [reem.abutalib@gmail.com]