# Temporary reward configuration

`rewards.json` maps milestone ID to server-awarded Campus Core. `library_intro: 10` is a temporary acceptance-test value, pending the team's final decision. Unknown combat-win milestones are rejected; no arbitrary client reward is accepted.

Only the first combat win per `(session_id, milestone_id)` awards Core and increments quests completed. `campus_core_total` is lifetime awarded Core, not a spendable balance. B3 records upgrade telemetry; pricing, balance debits and purchase authorization are not implemented here.
