# Worship Flow API (.NET 10)

Clean Architecture API for Worship Flow. This repository now contains M1: users, authentication, profile, roles, permissions, availability, profile photos, device tokens, multi-tenant filtering, audit logs, seed data, and tests.

## M1 endpoints

- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users`
- `PUT /api/users/{id}`
- `PATCH /api/users/{id}/status`
- `DELETE /api/users/{id}`
- `POST /api/users/{id}/photo`
- `GET /api/profile`
- `PUT /api/profile`
- `POST /api/profile/photo`
- `GET /api/users/{id}/availability`
- `PUT /api/users/{id}/availability`
- `GET /api/roles`
- `PUT /api/users/{id}/roles`
- `GET /api/permissions`
- `PUT /api/users/{id}/permissions`

## Default development administrator

`WorshipFlowDbContext.SeedAsync` creates a default tenant and a development administrator for local and integration-test bootstrapping:

- Email: `admin@worshipflow.local`
- Password: `ChangeMe123!`
- Role: `Administrator`

Change this credential before any non-development deployment.
