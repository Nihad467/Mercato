# Mercato frontend

React + Vite storefront and admin panel for the Mercato API. The Azerbaijani interface includes product discovery, account access, wishlist, cart, checkout, reviews, AI recommendations, product/category management, orders, and coupons.

## Run locally

Start the API in one terminal:

```powershell
dotnet run --project src/Mercato.API --launch-profile https
```

Start the frontend in a second terminal:

```powershell
cd frontend
pnpm install
pnpm dev
```

Open `http://localhost:5173`. Vite forwards `/api` requests to the HTTPS API at `https://localhost:7194`.

## Build for the API host

```powershell
cd frontend
pnpm install
pnpm build
```

The build is written to `src/Mercato.API/wwwroot`. The API then serves the storefront from its root URL. The checkout uses the backend's Mock payment provider; configure a real provider before using this flow for live payments.
