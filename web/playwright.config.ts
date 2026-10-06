import { defineConfig, devices } from '@playwright/test'

// The end-to-end tests start their own API on port 5055 (port 5000 is often taken by
// macOS AirPlay) and their own frontend on port 5174, so they never touch a dev session.
// The API uses the in-memory database unless E2E_DB_CONNECTION is set, for example to a
// PostgreSQL connection string.
const apiUrl = 'http://127.0.0.1:5055'
const webUrl = 'http://127.0.0.1:5174'

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  workers: 1,
  retries: 0,
  reporter: [['list']],
  use: {
    baseURL: webUrl,
    trace: 'retain-on-failure',
    ...devices['Desktop Chrome'],
  },
  webServer: [
    {
      command: `dotnet run --project ../src/DVLD.Api --no-launch-profile --urls ${apiUrl}`,
      url: `${apiUrl}/openapi/v1.json`,
      reuseExistingServer: false,
      timeout: 120_000,
      env: {
        ASPNETCORE_ENVIRONMENT: 'Development',
        ConnectionStrings__DefaultConnection: process.env.E2E_DB_CONNECTION ?? 'InMemory',
      },
    },
    {
      command: 'npm run dev -- --host 127.0.0.1 --port 5174 --strictPort',
      url: webUrl,
      reuseExistingServer: false,
      env: { DVLD_API_URL: apiUrl },
    },
  ],
})
