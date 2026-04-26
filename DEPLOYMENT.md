# FamLedger API — Deployment Runbook

Current target stack: **Render.com** (backend) · **Supabase** (Postgres) · **Vercel** (frontend, separate repo)

> Trade-off to know about: Render's free tier sleeps after 15 min of idle, so the
> first request after a quiet period takes 30–50 sec (cold start). Fine for an MVP;
> if that becomes painful, either move to Render's $7/mo plan or add an UptimeRobot
> 10-min ping to keep the instance warm.

---

## First-time deploy on Render

### 1. Push the repo to GitHub

Render deploys from Git. Make sure these files are committed and pushed:

- `Dockerfile`
- `.dockerignore`
- `render.yaml`
- `FamLedger.Api/Program.cs` (with the `/health` endpoint)

```bash
cd /path/to/FamLedger-backend
git status
git add Dockerfile .dockerignore render.yaml DEPLOYMENT.md FamLedger.Api/Program.cs
git commit -m "chore: add Docker + Render deployment artifacts"
git push origin master
```

### 2. Create the service on Render

1. Go to https://dashboard.render.com → **New + → Blueprint**
2. Connect your GitHub account if not already connected
3. Select the `FamLedger-backend` repo
4. Render reads `render.yaml` and proposes the service. Click **Apply**.
5. When prompted, enter the values for the 5 environment variables:

| Key | Value |
|---|---|
| `ConnectionStrings__DefaultConnection` | `Host=aws-1-ap-south-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.YOUR_PROJECT_REF;Password=YOUR_NEW_PASSWORD;SSL Mode=Require;Trust Server Certificate=true` |
| `JWT__Key` | Run `openssl rand -base64 48` locally, paste output |
| `JWT__Issuer` | `famledger-api` |
| `JWT__Audience` | `famledger-web` |
| `Cors__AllowedOrigins__0` | `https://famledger.vercel.app` (or whatever your Vercel URL is — update later) |

Notes:
- Double underscores `__` map to `:` in .NET config.
- Use the **Session pooler** connection string from Supabase (not direct).
- Rotate the Supabase DB password before this step if the old one has ever been pasted in chat / logs.

### 3. First build

Render kicks off a Docker build automatically. Watch the logs in the dashboard. First build takes ~5–8 min (nuget restore + docker layers).

Once it says **Live**, grab the public URL — it'll be something like `https://famledger-api.onrender.com`.

### 4. Smoke test

```bash
curl https://famledger-api.onrender.com/health
# → {"status":"ok"}
```

If that works, backend is live.

---

## Troubleshooting

### Build fails on `dotnet restore`
Usually a transient Nuget issue. Click **Manual Deploy → Deploy latest commit** in the dashboard to retry.

### Health check fails after build succeeds
Click **Logs** in the dashboard. Look for:
- `ConnectionStrings:DefaultConnection is not configured` → env var missing or name typo (check for double underscore)
- `Npgsql.NpgsqlException: ... SSL` → missing `SSL Mode=Require` in conn string
- `nodename nor servname provided` → wrong Supabase hostname; verify against Supabase dashboard → Settings → Database → Session pooler

### Cold start is annoying
After 15 min of idle, first request takes ~30–50s. Options:
1. **UptimeRobot keep-alive (free):** set up a monitor that pings `/health` every 10 min. Service never idles. Slightly against Render's spirit but widely used.
2. **Upgrade to Starter ($7/mo):** no sleep, always-on. Good once you have real users.

### CORS errors from frontend
Render dashboard → service → Environment → edit `Cors__AllowedOrigins__0` to match the live Vercel URL exactly (no trailing slash). Click **Save Changes**; Render redeploys automatically.

### Need to roll back
Dashboard → **Deploys** tab → pick a previous successful deploy → **Redeploy**.

---

## Day-2 operations

### Logs
Render dashboard → your service → **Logs** tab. Live tail + search.

### Restart
Dashboard → **Manual Deploy → Clear build cache & deploy** (full rebuild) or **Restart service** (no rebuild).

### Update env vars
Dashboard → **Environment** tab → edit → save. Service auto-redeploys.

### Deploy a new version
Just `git push origin master`. Render auto-deploys on every push to the connected branch.

---

## Monthly cost estimate

With the free tier:

- Service: **$0** (750 hours/mo included; sleeps when idle so you never hit the cap)
- Bandwidth: **$0** (100GB/mo free, you won't come close)
- Supabase: **$0** (500MB DB, pauses after 7 days of inactivity — just visit the dashboard to unpause)
- Vercel: **$0** (Hobby tier)

**Net cost: $0/mo** until you outgrow it.

First thing to upgrade when you have real users: Render Starter at $7/mo to kill the cold-start problem.
