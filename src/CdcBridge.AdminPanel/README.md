# CDC Bridge Admin Panel

A modern, responsive admin panel for managing CDC Bridge - a Change Data Capture bridge system built with Blazor WebAssembly and MudBlazor.

## Features

### 📊 Dashboard
- Real-time metrics visualization
- Event statistics (Total, Pending, Successful, Failed)
- Average delivery time monitoring
- Per-receiver metrics table

### 📝 Events Management
- View all CDC events with pagination
- Filter by tracking instance, receiver, and status
- See event details including operation type and delivery statuses
- Color-coded status indicators

### 📋 Logs Viewer
- Browse system logs with filtering
- Filter by log level and message search
- View detailed log information including exceptions
- Color-coded log levels

### ⚙️ Configuration Viewer
- View tracking instances configuration
- View receivers configuration
- Display all system settings

### 🔑 API Keys Management
- Create new API keys with different permission levels (ReadOnly, ReadWrite, Admin)
- View all API keys with their status
- Activate/Deactivate API keys
- Delete API keys
- Secure access with master password

## Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- CDC Bridge Host API running

### Building and Running

1. **Build the project:**
```bash
cd src/CdcBridge.AdminPanel
dotnet build
```

2. **Run the application:**
```bash
dotnet run
```

The application will be available at `https://localhost:5001` (or the port specified in launchSettings.json).

### Configuration

Before using the admin panel, you need to configure the API connection:

1. Navigate to the **Settings** page
2. Enter the **API Base URL** (e.g., `https://localhost:5001`)
3. (Optional) Enter an **API Key** if you already have one
4. Click **Save Settings**
5. Reload the page for changes to take effect

### Creating Your First API Key

To use most features, you need an API key:

1. Navigate to the **API Keys** page
2. Fill in the form:
   - **Name**: A descriptive name for the key
   - **Owner**: (Optional) The owner of the key
   - **Permission**: Choose the appropriate permission level
   - **Expires In Days**: (Optional) Set an expiration period
   - **Master Password**: Enter the master password configured in the CDC Bridge Host
3. Click **Create API Key**
4. **IMPORTANT**: Copy the generated API key immediately - you won't be able to see it again!
5. Go to **Settings** and paste the API key, then save

## Architecture

### Technology Stack
- **Framework**: Blazor WebAssembly (.NET 9.0)
- **UI Components**: MudBlazor 8.13.0
- **HTTP Client**: Refit 7.2.22
- **State Management**: Blazored.LocalStorage 4.5.0

### Project Structure
```
CdcBridge.AdminPanel/
├── Pages/              # Razor pages/components
│   ├── Home.razor     # Dashboard
│   ├── Events.razor   # Events management
│   ├── Logs.razor     # Logs viewer
│   ├── Configuration.razor  # Configuration viewer
│   ├── ApiKeys.razor  # API keys management
│   └── Settings.razor # Settings page
├── Layout/            # Layout components
│   ├── MainLayout.razor
│   └── NavMenu.razor
├── Services/          # Application services
│   └── AuthenticationStateService.cs
├── wwwroot/          # Static assets
└── Program.cs        # Application entry point
```

## Features in Detail

### Authentication
The admin panel uses API key-based authentication:
- API keys are stored securely in browser local storage
- Master password is required for API key management
- Different permission levels control access to features

### Responsive Design
- Mobile-friendly layout
- Collapsible navigation drawer
- Adaptive tables and cards
- Touch-friendly controls

### Real-time Updates
- Dashboard metrics update on page load
- Manual refresh available for all data views
- Efficient pagination for large datasets

## Security Considerations

- API keys are stored in browser local storage (consider your security requirements)
- Master password is required for API key management operations
- API key management operations are restricted to localhost on the backend
- Always use HTTPS in production

## Development

### Adding New Features
1. Create a new page in the `Pages` folder
2. Add navigation link in `NavMenu.razor`
3. Implement API calls using the injected Refit interfaces
4. Use MudBlazor components for consistent UI

### Debugging
```bash
dotnet run --configuration Debug
```

## Troubleshooting

### Cannot Connect to API
- Verify the API Base URL in Settings
- Ensure the CDC Bridge Host is running
- Check that CORS is properly configured on the Host

### API Key Not Working
- Verify the API key is valid and active
- Check that the key has the required permissions
- Ensure the key hasn't expired

### Missing Data
- Check that tracking instances are configured
- Verify receivers are properly set up
- Look at the Logs page for any error messages

## License

This project is part of CDC Bridge and follows the same license.
