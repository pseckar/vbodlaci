# AGENTS Guide

## Primary Rule

All implementation work in this repository is **spec-driven**.

1. `SPECIFICATION.md` is the canonical product contract.
2. Code must follow spec; code must not redefine product behavior.
3. If a new requirement appears, stop implementation and start a new spec update + planning cycle.

## Required Workflow For Any Change

1. Based on required changes, FIRST update the spec to reflect the new behavior, if applicable (e.g. no need to update it if bug fixing).
2. Read other relevant spec sections before editing code to get the context.
3. Do all necessary changes and track them back to the spec to double-check it was followed.
4. Do not expand scope beyond what is defined within the spec.
5. Make sure that relevant tests are updated and all tests are passing. See `README.md` for more information. 
