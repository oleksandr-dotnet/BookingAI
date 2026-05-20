# Booking System UI

React + TypeScript frontend for the Booking System API.

## Prerequisites

- Node.js 20+ recommended
- API running at `http://localhost:5070` (see repo root)
- PostgreSQL via `podman compose up -d` when using the full stack

## Setup

```powershell
cd booking-system-ui
npm install
```

Copy `.env.example` to `.env` and adjust `VITE_API_URL` if needed.

## Development

```powershell
npm run dev
```

Open http://localhost:5173

Start the API in another terminal:

```powershell
dotnet run --project BookingSystemAI.Api/BookingSystemAI.Api.csproj --launch-profile http
```

## Features

- Register as **Host** or **Client** (`role` in register body)
- JWT stored in `localStorage`; roles read from token claims
- **Public** apartment catalog with date filters and availability
- **Client**: book apartments, view my bookings (`/bookings`)
- **Host**: create apartments, view own listings (`/host/apartments`)

## Routes

| Path | Access |
|------|--------|
| `/` | Home |
| `/apartments` | Public catalog |
| `/bookings` | Client only |
| `/host` | Host only |
| `/login`, `/register` | Auth |

## Build

```powershell
npm run build
```
