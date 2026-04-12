# V bodlÃ¡ÄÃ­ Web Project Specification

Version: 1.0  
Last updated: 2026-04-11  
Document language: English (with required Czech labels/content where relevant)  
Website language: Czech only (`cs-CZ`)

## 1. Purpose Of This Document

This document is the primary source of truth for building, testing, and maintaining the **V bodlÃ¡ÄÃ­** website.

It is written for:
- developers joining the project,
- AI agents/models implementing and extending the system,
- project owner/stakeholders reviewing scope and decisions.

If implementation details are unclear, this specification takes priority over ad-hoc assumptions. Final MVP decisions are listed in section **21. Resolved Decisions For MVP**.

## 2. Product Context

The website presents services provided by **Veronika BodlÃ¡kovÃ¡** under the personal brand **V bodlÃ¡ÄÃ­**.

Core service pillars shown on the website:
1. **Breathwork v bodlÃ¡ÄÃ­** (prÃ¡ce s dechem)
2. **KonÄ› v bodlÃ¡ÄÃ­** (sebepoznÃ¡nÃ­/transformace facilitovanÃ¡ koÅˆmi)
3. **Veterina v bodlÃ¡ÄÃ­** (veterinÃ¡rnÃ­ oÅ¡etÅ™enÃ­ s osobnÃ­m a lidskÃ½m pÅ™Ã­stupem, vÄetnÄ› domÃ¡cÃ­ch nÃ¡vÅ¡tÄ›v)

### Additional context about Veronika and the brand:
- Veronika is a veterinary doctor and owns a veterinary clinic in Hlinsko, Czech Republic.
- The clinic website (`veterinahlinsko.cz`) is context only and is **not** the product being built here.
- This new web presents her integrated work in self-development and human-animal relational care.
- Veronika presents herself on social media (Facebook, Instagram) at this point, but the new website will be the main hub for her services and content.

## 3. Product Goals

Primary goals:
- Clearly present the three service pillars under one coherent brand.
- Help visitors understand what each service is, for whom it is, and how it works.
- Convert interest into action via course registrations (for Breathwork + KonÄ›), direct inquiries (contact form), and newsletter subscriptions.
- Give Veronika a simple admin interface to manage course offerings.

Secondary goals:
- Build trust via testimonials, personal story, and calm/clear communication.
- Provide technically simple but reliable foundations for future growth.

## 4. Non-Goals (MVP)

Out of scope for now:
- user accounts for participants,
- client portal (my courses/materials),
- online payment gateway,
- full automated post-registration operations (beyond email notifications),
- multilingual support,
- full Facebook API synchronization (creation/editing of posts directly from admin).

These are possible future extensions and should not be implemented in MVP unless explicitly approved.

## 5. Target Users And Main Use Cases

### 5.1 Visitor segments

- People interested in breathwork and self-development practices.
- People interested in horse-facilitated transformation/self-awareness.
- Pet owners seeking compassionate, personal veterinary support (including at-home care/euthanasia).
- Returning visitors tracking new course dates.

### 5.2 Admin segment

- Veronika (primary admin, non-technical user) managing course data and communications.

### 5.3 Core user journeys

1. Visitor lands on homepage, explores services, opens a service detail page.
2. Visitor finds an upcoming course, opens course detail, submits registration form.
3. Visitor subscribes to newsletter to receive future course announcements.
4. Visitor sends direct inquiry via contact form.
5. Admin logs in, creates/edits/hides/deletes courses.

## 6. Information Architecture

The website is content-heavy, scrolling-oriented, and intentionally simple.

Top-level structure:
- **Homepage** (`/`) with service switcher and high-level content.
- **Service detail pages** (one long page per service).
- **Course detail page** (one long page per course).
- **Admin area** for course/newsletter management.
- **Legal pages** for privacy/cookies/terms as required by GDPR.

No classic multi-level navigation menu is required in MVP; navigation is primarily by:
- top service switcher,
- in-page sections,
- contextual CTA buttons,
- footer links.

### 6.1 Static Vs Dynamic Content Map

Mostly static in MVP:
- service explanations,
- personal/about sections,
- FAQ content,
- testimonial content,
- legal pages.

Dynamic in MVP:
- course data and listing/filtering,
- course detail pages generated from course records,
- course registration records,
- contact form submissions,
- newsletter subscriptions and sends.

## 7. Detailed Page Specification

### 7.1 Homepage (`/`)

Purpose: introduce brand, services, trust signals, and conversion entry points.

Main sections:
1. Hero:
- Brand: **V bodlÃ¡ÄÃ­**
- Intro text (Czech).
2. Service switcher cards (top â€œrozcestnÃ­kâ€):
- **Breathwork v bodlÃ¡ÄÃ­**
- **KonÄ› v bodlÃ¡ÄÃ­**
- **Veterina v bodlÃ¡ÄÃ­**
- each card links to its service detail page.`n3. About/identity block:
- introduces Veronika and integration of all three areas.
4. Upcoming courses block (dynamic):
- combined list of published upcoming courses from Breathwork + KonÄ›,
- sorted by nearest date ascending,
- includes filter control by type (`VÅ¡e`, `Breathwork`, `KonÄ›`),
- visually optimized for horizontal card browsing when needed.
5. Testimonials (`Ohlasy`) block (initially static content).
6. Contact form block (`Kontakt`).
7. Newsletter subscription block (`Newsletter`).
8. Footer:
- social links (Facebook, Instagram),
- contact details,
- legal links.
9. Floating/back-to-top control:
- button text in Czech, e.g. **â€žJÃ­t nahoruâ€œ**,
- appears after meaningful scroll depth.

### 7.2 Service Detail Pages

Three pages:
- Breathwork detail (`/breathwork-v-bodlaci` or equivalent final slug)
- KonÄ› detail (`/kone-v-bodlaci` or equivalent final slug)
- Veterina detail (`/veterina-v-bodlaci` or equivalent final slug)`n
Common structure pattern:
1. Header with service name and back link (`â† ZpÄ›t na hlavnÃ­ strÃ¡nku`).
2. Service introduction/value proposition.
3. â€œFor whom it isâ€ / â€œWhat it bringsâ€ content.
4. â€œHow a session/course worksâ€ step-by-step section.
5. FAQ block (`ÄŒastÃ© otÃ¡zky`).
6. Contact CTA/form for service-specific inquiry.
7. Newsletter block (can be global or service-scoped).
8. Footer.

Dynamic behavior by service:
- **Breathwork v bodlÃ¡ÄÃ­**: show upcoming breathwork courses (dynamic).
- **KonÄ› v bodlÃ¡ÄÃ­**: show upcoming horse-facilitated courses (dynamic).
- **Veterina v bodlÃ¡ÄÃ­**: no course catalog in MVP; inquiry-driven content and CTA, with emphasis on consultations, home visits, and compassionate at-home farewell/euthanasia support.

### 7.3 Course Detail Page (`/kurzy/{slug}`)

Purpose: convert a motivated visitor into a course registration.

Sections:
1. Course hero/title.
2. Core summary (short description and context).
3. Key course metadata:
- date,
- time,
- location,
- price,
- optional capacity.
4. â€œWhat to expectâ€ (content/process).
5. Registration form (`PÅ™ihlÃ¡Å¡enÃ­ na kurz`) - should really draw attention to it, visually.
6. Related/upcoming courses block (â€œDalÅ¡Ã­ termÃ­ny ...â€).
7. Footer.

Registration form minimum fields:
- `JmÃ©no` (required),
- `E-mail` (required),
- `PoznÃ¡mka` (optional),
- consent checkbox text (final legal wording TBD, required).
- validate inputs and show user-friendly error messages in Czech.

On submit:
- send notification email to Veronika with registration details,
- send confirmation email to participant,
- persist registration in DB for audit/history.

### 7.4 Admin Interface

Admin-only pages (protected by authentication):
- login page,
- course list page,
- create/edit course page,
- newsletter subscriber overview/export (minimum read/export support),
- read-only message inbox view for contact forms.

Admin course actions:
- create course,
- update course,
- delete course (soft delete preferred),
- preview public card/detail rendering,
- option to save as draft (not visible),
- or publish (visible) with confirmation dialog, informing that subscribers will be notified.

## 8. Content Model And Business Rules

### 8.1 Course domain rules (MVP)

- Every course is an individual item (no master course with multiple dates).
- No template system in MVP.
- Possible future enhancement: â€œduplicate existing courseâ€ action.
- Course types in MVP are `breathwork` and `kone` (horse-facilitated transformation).
- Published courses appear on public pages.
- Courses in draft are not shown publicly.
- Sorting is always nearest upcoming first.

### 8.2 Proposed course fields

Required:
- `id` (UUID)
- `type` (`breathwork` | `kone`)
- `title`
- `slug`
- `start_datetime` (Europe/Prague timezone)
- `city_or_area`
- `price_czk` (or explicit â€œon requestâ€ policy if agreed)
- `is_published`
- `created_at`
- `updated_at`
- `published_at` (nullable, set when `is_published` changes to true)

Optional fields included in MVP:
- `end_datetime`
- `venue_name`
- `address_text`
- `capacity_total`
- `description_short`
- `description_full`
- `registration_deadline`
- `note_for_confirmation_email`

### 8.3 Newsletter rules

- Visitor can subscribe with email via public form.
- Subscription is saved in DB.
- On new course publish (or publish visibility change), system sends announcement email to subscribers.
- Deduplication is required: one subscriber should not receive duplicate notifications for the same course event.

Interest segmentation:
- Mockup suggests service checkboxes.
- Segmentation is enabled in MVP by service preferences (`Breathwork`, `Koně`, `Veterina`).

### 8.4 Contact form rules

- Public contact form sends email to Veronika.
- Contact message should be persisted in DB (recommended for traceability).
- Anti-spam protection required (honeypot + rate limiting).

## 9. Email Flows

MVP email events:
1. Contact form submission:
- To Veronika: message details.
2. Course registration submission:
- To Veronika: registrant details (`JmÃ©no`, `E-mail`, `PoznÃ¡mka`, course info).
- To registrant: confirmation email in Czech.
3. Newsletter campaign on newly published course:
- To subscribers: new course summary + CTA link.

Email requirements:
- configurable SMTP provider integration,
- sender identity and domain authentication (SPF/DKIM/DMARC) handled by deployment,
- template-based messages in Czech,
- basic delivery/error logging.

## 10. Design And Style Direction

Design direction is not finalized; MVP should follow these constraints:

Visual tone:
- natural, grounded, light, human,
- calm and trustworthy (not clinical, not aggressive marketing),
- aligned with mindfulness/meditation/yoga/self-awareness atmosphere.

Palette direction:
- earthy/natural tones (greens, warm neutrals, soft browns, muted accents),
- avoid harsh saturated contrasts unless used intentionally for CTA.

Imagery direction:
- nature, horses, human-animal connection, calm environments,
- authentic photography preferred over generic stock look (will be supplied later).

Layout direction:
- long-scroll storytelling pages,
- section rhythm with clear breathing space,
- card-based content for services/courses/testimonials.

Interaction direction:
- simple transitions,
- clear CTA hierarchy,
- visible â€œback to topâ€ support.

Language/tone on website copy:
- Czech language only,
- warm, respectful, human-centered communication,
- clear and simple wording without jargon.

## 11. Technical Architecture

Mandatory stack (from project constraints):
- **Backend/UI**: ASP.NET + Razor Pages
- **Database**: PostgreSQL
- **Reverse proxy/web server**: Nginx
- **App process management**: systemd
- **Source hosting**: public GitHub repository
- **CI/CD**: GitHub Actions

Recommended architecture shape:
- Razor Pages app serving both static-heavy pages and dynamic course data.
- Service layer for business logic (courses, registrations, newsletter, emails).
- Repository/data access layer over PostgreSQL.
- Background job mechanism for newsletter dispatch and retry logic (can start simple).

## 12. Hosting And Environments

Expected environments:
- `dev` (local + possibly Azure free tier for experiments),
- `staging` (same host as production, separate database, service and config, and only when needed for testing (shutdown on deploy to production)),
- `production` (cost-efficient VPS/cloud provider such as Hetzner).

Expected production topology (MVP):
- single VM/VPS,
- Nginx in front of Kestrel,
- PostgreSQL on same server initially,
- TLS via Letâ€™s Encrypt (or equivalent).

Traffic expectation:
- tens to low hundreds of daily visitors.
- prioritize reliability and simplicity over complex scaling.

## 13. Security, Privacy, GDPR

Required controls:
- HTTPS only,
- secure admin authentication (hashed password, strong session handling),
- authorization enforcement on all admin endpoints,
- CSRF protection on forms,
- server-side validation for all inputs,
- output encoding/sanitization where needed,
- basic abuse controls (rate limiting, anti-spam).

GDPR/privacy baseline:
- privacy policy page (`ZÃ¡sady zpracovÃ¡nÃ­ osobnÃ­ch ÃºdajÅ¯`),
- cookie policy page (`ZÃ¡sady cookies`),
- explicit consent language where legally required (newsletter, course terms),
- data minimization: only collect required fields,
- ability to delete/export personal data upon request (manual process acceptable initially),
- retention policy definition required before production,
- ability to unsubscribe from newsletter in every email.

Cookies:
- if only essential cookies are used, minimal consent notice may be enough (legal review needed),
- if analytics/marketing cookies are used, consent banner with preference management is required.

## 14. SEO And Discoverability

MVP SEO baseline:
- semantic HTML and heading hierarchy,
- per-page title and meta description,
- canonical URLs,
- Open Graph/Twitter metadata,
- XML sitemap and robots.txt,
- Czech keyword-focused copy for services and local intent (Hlinsko/region where relevant),
- clean URL slugs in Czech-compatible format.

Local trust signals:
- consistent contact identity,
- service locality and context,
- social links in footer.

## 15. Accessibility (A11y)

Target: WCAG 2.1 AA practical compliance for MVP.

Minimum requirements:
- keyboard navigability across all interactive elements,
- visible focus states,
- sufficient color contrast,
- accessible labels and error messages for forms,
- proper semantics (`button`, `label`, `fieldset`, headings),
- screen-reader friendly form validation messages.

## 16. Performance And Quality Targets

Given low expected traffic, quality priorities are:
- fast first-load for content pages,
- stable form submission and email operations,
- maintainable code for future extension.

Suggested baseline targets (to validate later):
- LCP under 2.5s on major pages (typical mobile conditions),
- no blocking JS required for reading key static content,
- optimized/compressed images with lazy loading where appropriate.

## 17. Testing Strategy

Recommended MVP coverage:
- unit tests for core business rules (course visibility/sorting, email trigger decisions),
- integration tests for form submission endpoints,
- smoke tests for public page rendering and admin auth,
- manual QA checklist for critical journeys (browse service pages, submit contact form, subscribe newsletter, register to a course, admin creates/edits/hides course).

## 18. Observability And Operations

Minimum operational requirements:
- structured application logs,
- error logging with actionable context,
- email send/failure logs,
- DB backup strategy (daily snapshot + retention policy),
- deployment rollback procedure in CI/CD workflow.

## 19. Future Extensions (Not MVP)

Potential future roadmap:
- online payments (gateway integration),
- automated payment instructions and registration workflow states,
- participant account system and client portal,
- course materials access,
- richer CRM-style communication history,
- course duplication/template tooling,
- broader dynamic content management (beyond courses).

Facebook-related options (explicitly open):
- embedded posts (simple but low design quality),
- direct FB API synchronization (powerful but complex),
- middle-ground helper: generate admin-ready FB post text for copy/paste.

MVP includes helper-generated Facebook post text in admin; direct API synchronization remains a future extension.

## 20. Canonical Czech Labels (Current Draft)

Brand and service naming to preserve:
- `V bodlÃ¡ÄÃ­`
- `Breathwork v bodlÃ¡ÄÃ­`
- `KonÄ› v bodlÃ¡ÄÃ­`
- `Veterina v bodlÃ¡ÄÃ­`

Important UI terms (examples for consistency):
- `Zjistit vÃ­ce`
- `Detail kurzu`
- `NejbliÅ¾Å¡Ã­ kurzy`
- `ÄŒastÃ© otÃ¡zky`
- `Kontakt`
- `Newsletter`
- `PÅ™ihlÃ¡Å¡enÃ­ na kurz`
- `ZÃ¡vaznÄ› pÅ™ihlÃ¡sit`
- `JÃ­t nahoru`

Final copy deck may refine wording, but naming consistency must be preserved.

## 21. Resolved Decisions For MVP

The following decisions are finalized and must be implemented as defined:

1. Domain and routing:
- single main domain with path-based routing,
- no service subdomains in MVP.

2. Admin authentication:
- local ASP.NET Identity only (no external OAuth in MVP),
- one primary admin account model, extensible later.

3. URL slugs:
- ASCII slugs for services and course detail pages.

4. Course behavior:
- course statuses include draft/published/canceled,
- canceled courses are hidden from public listings,
- canceled courses remain accessible by direct URL with status notice,
- capacity is informational only in MVP (no automatic registration blocking),
- price is free-text in MVP.

5. Newsletter:
- single opt-in,
- service-preference segmentation (`Breathwork`, `Koně`, `Veterina`),
- unsubscribe link in every newsletter email,
- deduplication per course publish event.

6. Form protection:
- honeypot + server-side rate limiting,
- no CAPTCHA in MVP.

7. Email integration:
- provider-agnostic SMTP configuration in app settings/environment.

8. Public UX scope:
- horizontal course browsing only (no "all courses" page in MVP),
- no frozen/floating service switcher in MVP,
- testimonials remain static in MVP,
- copy tone is Czech `tykání`.

9. Facebook phase 1:
- no API synchronization,
- include admin helper that generates copy-ready Facebook post text.

10. Analytics and social:
- no analytics integration in MVP,
- social links are placeholders until final URLs are provided.

11. Legal data:
- legal/business identity is partially pending,
- explicit placeholder values are allowed only for non-production environments.

## 22. Frozen Assumptions

The following assumptions are locked for MVP implementation:
- website remains single-language Czech,
- dynamic content is limited to courses and form-related records/communications,
- veterinary service page is inquiry-based (not a course catalog),
- mockup structure informed layout direction, but `mockup/` is not implementation source code.

## 23. Change Control (Spec-First Rule)

This document is now frozen for MVP implementation.

Rules:
- implementation must follow this specification,
- the specification must not be changed during implementation work,
- if a new requirement or conflict appears, implementation pauses and a new spec update + planning cycle is required before coding continues.

## 24. AI Agent Working Conventions

When implementing against this document:
- treat this file as the product source of truth unless the user explicitly overrides it,
- keep public-facing copy and labels in Czech,
- keep MVP scope tight (avoid introducing non-goal features),
- if code and spec diverge, code must be adjusted to match spec,
- do not redefine product behavior in code outside this frozen specification.

