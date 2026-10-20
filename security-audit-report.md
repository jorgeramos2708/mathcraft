# Security Audit Report v0.4

## Findings
1. SaveSystem deserialization without schema validation - Medium
2. RPC rate limiting missing - Medium
3. Reward unlock client-side - High

## Remediations
- Add TryParse and fallback
- Add rate limiter
- Server-authoritative unlocks
