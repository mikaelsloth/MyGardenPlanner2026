using Xunit.v3;

// E2E-tests spawner egne processer og SQL-databaser med et fast CI-databasenavn —
// parallel kørsel af flere test-collections giver race conditions i provisioneringen.
[assembly: Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]