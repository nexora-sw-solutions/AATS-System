# AATS — API Endpoints Reference

> **Scope**: All domains.  
> **Base URL**: `/api/v1`  
> **Auth**: All endpoints (except `/auth/login`) require `Authorization: Bearer <token>`.  
> **Pagination**: List endpoints accept `?page=1&limit=10&sort=created_at&order=desc`.  
> **Soft Delete**: DELETE endpoints set `is_deleted = TRUE` (no hard deletes).

---

## 1. Authentication & Users

### Auth

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/auth/login` | Authenticate user (returns JWT) |
| `POST` | `/auth/logout` | Invalidate current session |
| `POST` | `/auth/refresh` | Refresh JWT token |
| `POST` | `/auth/forgot-password` | Submit password reset request |
| `PATCH` | `/auth/change-password` | Change current user's password |

### Users

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/users` | List all users (filterable: `?role=Admin&branch_id=...&is_active=true`) |
| `GET` | `/users/:id` | Get user by ID |
| `POST` | `/users` | Create new user (Admin only) |
| `PUT` | `/users/:id` | Update user details |
| `DELETE` | `/users/:id` | Soft-delete user |
| `PATCH` | `/users/:id/status` | Toggle `is_active` status |
| `POST` | `/users/:id/logo` | Upload user profile logo (→ Cloudflare R2) |
| `DELETE` | `/users/:id/logo` | Remove user profile logo |

---

## 2. Branches

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/branches` | List all branches |
| `GET` | `/branches/:id` | Get branch by ID |
| `POST` | `/branches` | Create new branch (Admin only) |
| `PUT` | `/branches/:id` | Update branch details |
| `PATCH` | `/branches/:id/status` | Toggle `is_active` status |

---

## 3. Clients

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/clients` | List clients (filterable: `?status=Active&branch_id=...&search=...`) |
| `GET` | `/clients/:id` | Get client by ID (includes revenue, outstanding balance) |
| `POST` | `/clients` | Create new client |
| `PUT` | `/clients/:id` | Update client details |
| `DELETE` | `/clients/:id` | Soft-delete client |
| `POST` | `/clients/:id/logo` | Upload client logo (→ Cloudflare R2, updates `logo_storage_key`) |
| `DELETE` | `/clients/:id/logo` | Remove client logo |
| `GET` | `/clients/:id/records` | Get all service records across modules for a client |
| `GET` | `/clients/:id/revenue-summary` | Aggregated revenue & outstanding balances |

---

## 4. Accounts & Audit

All 6 service modules under this domain share an identical CRUD pattern. Each supports filtering by `branch_id`, `client_id`, `payment_status`, `process`, and date range (`date_from`, `date_to`).

### 4.1 Audit & Assurance

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/audit-assurance` | List records |
| `GET` | `/audit-assurance/:id` | Get record by ID |
| `POST` | `/audit-assurance` | Create record |
| `PUT` | `/audit-assurance/:id` | Update record |
| `DELETE` | `/audit-assurance/:id` | Soft-delete record |
| `PATCH` | `/audit-assurance/:id/process` | Update process step (`Bookkeep` → `Draft Account` → `Finalize` → `Handover` → `Return` → `Submit`) |
| `PATCH` | `/audit-assurance/:id/payment` | Update payment details (status, option, amounts) |

### 4.2 Forensic Audit

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/forensic-audit` | List records |
| `GET` | `/forensic-audit/:id` | Get record by ID |
| `POST` | `/forensic-audit` | Create record |
| `PUT` | `/forensic-audit/:id` | Update record |
| `DELETE` | `/forensic-audit/:id` | Soft-delete record |
| `PATCH` | `/forensic-audit/:id/process` | Update process step (`Reporting` → `Meeting Complete`) |
| `PATCH` | `/forensic-audit/:id/payment` | Update payment details |

### 4.3 Internal Audit

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/internal-audit` | List records |
| `GET` | `/internal-audit/:id` | Get record by ID |
| `POST` | `/internal-audit` | Create record |
| `PUT` | `/internal-audit/:id` | Update record |
| `DELETE` | `/internal-audit/:id` | Soft-delete record |
| `PATCH` | `/internal-audit/:id/process` | Update process step (`Reporting` → `Meeting Complete`) |
| `PATCH` | `/internal-audit/:id/payment` | Update payment details |

### 4.4 Management Account

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/management-account` | List records |
| `GET` | `/management-account/:id` | Get record by ID |
| `POST` | `/management-account` | Create record |
| `PUT` | `/management-account/:id` | Update record |
| `DELETE` | `/management-account/:id` | Soft-delete record |
| `PATCH` | `/management-account/:id/process` | Update process step (`Bookkeep` → `Draft Account` → `Finalize` → `Handover`) |
| `PATCH` | `/management-account/:id/payment` | Update payment details |

### 4.5 Internal Control & Outsourcing

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/internal-control` | List records |
| `GET` | `/internal-control/:id` | Get record by ID |
| `POST` | `/internal-control` | Create record |
| `PUT` | `/internal-control/:id` | Update record |
| `DELETE` | `/internal-control/:id` | Soft-delete record |
| `PATCH` | `/internal-control/:id/process` | Update process step (`Reporting` → `Meeting Complete`) |
| `PATCH` | `/internal-control/:id/payment` | Update payment details |


### 4.6 Tax Account

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/tax-account` | List records (filterable: `?assigned_to=...&process=...`) |
| `GET` | `/tax-account/:id` | Get record by ID |
| `POST` | `/tax-account` | Create record |
| `PUT` | `/tax-account/:id` | Update record |
| `DELETE` | `/tax-account/:id` | Soft-delete record |
| `PATCH` | `/tax-account/:id/process` | Update process step (`Bookkeep` → `Tax Amount` → `Finalize` → `Tax Paid` → `Submit`) |
| `PATCH` | `/tax-account/:id/payment` | Update payment details |
| `PATCH` | `/tax-account/:id/assign` | Assign staff member |

### 4.7 Other Audit Records

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/accounts-audit/others` | List other audit records |
| `GET` | `/accounts-audit/others/:id` | Get record by ID |
| `POST` | `/accounts-audit/others` | Create record |
| `PUT` | `/accounts-audit/others/:id` | Update record |
| `DELETE` | `/accounts-audit/others/:id` | Soft-delete record |
| `PATCH` | `/accounts-audit/others/:id/payment` | Update payment details |

---

## 5. Tax

### 5.1 Tax Filing (CIT / IIT / VAT / SSCL / WHT / Others)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/tax-filing` | List all filings (filterable: `?tax_type=CIT&payment_status=...&branch_id=...`) |
| `GET` | `/tax-filing/:id` | Get filing by ID |
| `POST` | `/tax-filing` | Create filing (body includes `tax_type`) |
| `PUT` | `/tax-filing/:id` | Update filing |
| `DELETE` | `/tax-filing/:id` | Soft-delete filing |
| `GET` | `/tax-filing/cit` | List CIT filings only |
| `GET` | `/tax-filing/iit` | List IIT filings only |
| `GET` | `/tax-filing/vat` | List VAT filings only |
| `GET` | `/tax-filing/sscl` | List SSCL filings only |
| `GET` | `/tax-filing/wht` | List WHT filings only |
| `GET` | `/tax-filing/others` | List Others filings only |

---

## 6. Secretarial & Advisory

All sub-modules under this domain support filtering by `branch_id`, `client_id`, `payment_status`, and date range (`date_from`, `date_to`).

### 6.1 Company Registration

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/company-registration` | List records (filterable: `?process=...&payment_status=...`) |
| `GET` | `/company-registration/:id` | Get record by ID (includes officers, TIN data) |
| `POST` | `/company-registration` | Create record (body includes officers array) |
| `PUT` | `/company-registration/:id` | Update record |
| `DELETE` | `/company-registration/:id` | Soft-delete record |
| `PATCH` | `/company-registration/:id/process` | Update process step (`Name Approve` → `Forms Preparation` → `Signature` → `Payment` → `Incorporation` → `Seal` → `Certified copy` → `Document hand over`) |
| `PATCH` | `/company-registration/:id/payment` | Update payment details |

#### Company Officers (nested under registration)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/company-registration/:id/officers` | List officers for a registration |
| `POST` | `/company-registration/:id/officers` | Add officer (type: `director`, `secretary`, `shareholder`, `other`) |
| `PUT` | `/company-registration/:id/officers/:officerId` | Update officer |
| `DELETE` | `/company-registration/:id/officers/:officerId` | Remove officer |

### 6.2 EPF / ETF

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/epf-etf` | List records (filterable: `?process=...`) |
| `GET` | `/epf-etf/:id` | Get record by ID (includes staff list summary) |
| `POST` | `/epf-etf` | Create record |
| `PUT` | `/epf-etf/:id` | Update record |
| `DELETE` | `/epf-etf/:id` | Soft-delete record |
| `PATCH` | `/epf-etf/:id/payment` | Update payment details |

#### EPF/ETF Staff (nested under record)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/epf-etf/:id/staff` | List all staff for this EPF/ETF record (filterable: `?search=...&process=...`) |
| `GET` | `/epf-etf/:id/staff/:staffId` | Get staff member details |
| `POST` | `/epf-etf/:id/staff` | Add staff member |
| `PUT` | `/epf-etf/:id/staff/:staffId` | Update staff details |
| `DELETE` | `/epf-etf/:id/staff/:staffId` | Remove staff member |
| `PATCH` | `/epf-etf/:id/staff/:staffId/process` | Update staff process step (`Submit` → `Complete`) |

### 6.3 Trade Marks

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/trade-marks` | List records |
| `GET` | `/trade-marks/:id` | Get record by ID |
| `POST` | `/trade-marks` | Create record |
| `PUT` | `/trade-marks/:id` | Update record |
| `DELETE` | `/trade-marks/:id` | Soft-delete record |
| `PATCH` | `/trade-marks/:id/payment` | Update payment details |

### 6.4 Trade Licenses

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/trade-licenses` | List records |
| `GET` | `/trade-licenses/:id` | Get record by ID |
| `POST` | `/trade-licenses` | Create record |
| `PUT` | `/trade-licenses/:id` | Update record |
| `DELETE` | `/trade-licenses/:id` | Soft-delete record |
| `PATCH` | `/trade-licenses/:id/payment` | Update payment details |

### 6.5 Import / Export Clearance

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/import-export-clearance` | List records (filterable: `?tin=...`) |
| `GET` | `/import-export-clearance/:id` | Get record by ID |
| `POST` | `/import-export-clearance` | Create record |
| `PUT` | `/import-export-clearance/:id` | Update record |
| `DELETE` | `/import-export-clearance/:id` | Soft-delete record |
| `PATCH` | `/import-export-clearance/:id/payment` | Update payment details |

### 6.6 HR Management Consulting

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/hr-management-consulting` | List records |
| `GET` | `/hr-management-consulting/:id` | Get record by ID |
| `POST` | `/hr-management-consulting` | Create record |
| `PUT` | `/hr-management-consulting/:id` | Update record |
| `DELETE` | `/hr-management-consulting/:id` | Soft-delete record |
| `PATCH` | `/hr-management-consulting/:id/payment` | Update payment details |

### 6.7 Business Plan & Valuation

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/business-plan-valuation` | List records |
| `GET` | `/business-plan-valuation/:id` | Get record by ID |
| `POST` | `/business-plan-valuation` | Create record |
| `PUT` | `/business-plan-valuation/:id` | Update record |
| `DELETE` | `/business-plan-valuation/:id` | Soft-delete record |
| `PATCH` | `/business-plan-valuation/:id/payment` | Update payment details |

### 6.8 BOI Registration

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/boi` | List records (filterable: `?country=...`) |
| `GET` | `/boi/:id` | Get record by ID |
| `POST` | `/boi` | Create record |
| `PUT` | `/boi/:id` | Update record |
| `DELETE` | `/boi/:id` | Soft-delete record |
| `PATCH` | `/boi/:id/payment` | Update payment details |

### 6.9 Other Secretarial Records

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/secretarial-advisory/others` | List records |
| `GET` | `/secretarial-advisory/others/:id` | Get record by ID |
| `POST` | `/secretarial-advisory/others` | Create record |
| `PUT` | `/secretarial-advisory/others/:id` | Update record |
| `DELETE` | `/secretarial-advisory/others/:id` | Soft-delete record |
| `PATCH` | `/secretarial-advisory/others/:id/payment` | Update payment details |

---

## 7. Nexora Services

### Service Catalog (Lookup)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/nexora/services` | List available Nexora services |
| `GET` | `/nexora/services/:id` | Get service by ID |
| `POST` | `/nexora/services` | Create service type (Admin only) |
| `PUT` | `/nexora/services/:id` | Update service type |
| `DELETE` | `/nexora/services/:id` | Soft-delete service type |

### Service Requests

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/nexora/requests` | List requests (filterable: `?service_id=...&branch_id=...`) |
| `GET` | `/nexora/requests/:id` | Get request by ID |
| `POST` | `/nexora/requests` | Create new service request |
| `PUT` | `/nexora/requests/:id` | Update request |
| `DELETE` | `/nexora/requests/:id` | Soft-delete request |

---

## 8. Payments & Cheques

### Payments (Polymorphic)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/payments` | List all payments (filterable: `?record_type=...&record_id=...&status=...`) |
| `GET` | `/payments/:id` | Get payment by ID (includes cheque details if applicable) |
| `POST` | `/payments` | Create payment record |
| `PUT` | `/payments/:id` | Update payment |
| `DELETE` | `/payments/:id` | Delete payment |
| `GET` | `/payments/by-record/:record_type/:record_id` | Get all payments for a specific service record |

### Cheque Details

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/cheques/:id` | Get cheque details |
| `POST` | `/cheques` | Create cheque details (linked to a payment) |
| `PUT` | `/cheques/:id` | Update cheque details |
| `DELETE` | `/cheques/:id` | Delete cheque details |
| `PATCH` | `/cheques/:id/status` | Update cheque status (`Pending` / `Cleared` / `Return`) |

---

## 9. Documents (Cloudflare R2)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/documents` | List documents (filterable: `?record_type=...&record_id=...&category=...`) |
| `GET` | `/documents/:id` | Get document metadata |
| `GET` | `/documents/:id/download` | Get pre-signed download URL from R2 |
| `POST` | `/documents/upload` | Upload file to R2 (multipart, body: `record_type`, `record_id`, `category`) |
| `PUT` | `/documents/:id` | Update document metadata (description, category) |
| `DELETE` | `/documents/:id` | Soft-delete document & remove from R2 |
| `GET` | `/documents/by-record/:record_type/:record_id` | Get all documents for a specific record |

---

## 10. Activity Logs

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/activity-logs` | List logs (filterable: `?user_id=...&module=...&action=...&date_from=...&date_to=...`) |
| `GET` | `/activity-logs/:id` | Get log entry by ID |
| `GET` | `/activity-logs/by-record/:record_type/:record_id` | Activity history for a specific record |

> Activity logs are **write-only from the backend** — created automatically when users perform CRUD actions.

---

## 11. Sync (Dotmim.Sync)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/sync/initialize` | Initialize sync session for a device |
| `POST` | `/sync/upload` | Upload client-side changes (batch) |
| `GET` | `/sync/download` | Download server-side changes since last sync |
| `GET` | `/sync/conflicts` | List unresolved sync conflicts |
| `POST` | `/sync/conflicts/:id/resolve` | Resolve a specific conflict |
| `GET` | `/sync/status` | Current sync status for the authenticated device |

---

## 12. Dashboard & Analytics

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/dashboard/summary` | Overview stats (total clients, revenue, pending payments, records by module) |
| `GET` | `/dashboard/revenue` | Revenue breakdown (`?period=monthly&year=2026`) |
| `GET` | `/dashboard/records-by-status` | Record counts grouped by payment status per module |
| `GET` | `/dashboard/recent-activity` | Latest 20 activity log entries |
| `GET` | `/dashboard/branch-summary` | Per-branch client/revenue stats |

---

## Common Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `page` | `int` | Page number (default: 1) |
| `limit` | `int` | Items per page (default: 10, max: 100) |
| `sort` | `string` | Sort field (e.g., `created_at`, `record_date`) |
| `order` | `string` | Sort order: `asc` or `desc` |
| `search` | `string` | Global text search (client name, company, record code) |
| `branch_id` | `UUID` | Filter by branch |
| `client_id` | `UUID` | Filter by client |
| `payment_status` | `string` | Filter: `Paid`, `Unpaid`, `Partial` |
| `date_from` | `date` | Start date filter (ISO 8601) |
| `date_to` | `date` | End date filter (ISO 8601) |

---

## Standard Response Format

```json
{
  "success": true,
  "data": { ... },
  "meta": {
    "page": 1,
    "limit": 10,
    "total": 156,
    "total_pages": 16
  }
}
```

### Error Response

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Client name is required",
    "details": [
      { "field": "client_name", "message": "This field is required" }
    ]
  }
}
```
